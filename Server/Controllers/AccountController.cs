using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.Authorization.Jwt;
using Server.Models;
using Server.Config;
using System.Security.Claims;
using Server.Services;
using MimeKit;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Server.Authorization.Roles;
using Server.Data;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly MailService _mailService;
        private readonly FcmsContext _context;

        public AccountController(
            IConfiguration configuration,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            JwtTokenGenerator jwtTokenGenerator,
            MailService mailService,
            FcmsContext context
            )
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mailService = mailService;
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Error = ModelState
                });
            }

            var user = new User
            {
                Name = register.Name,
                UserName = register.Email,
                Email = register.Email
            };
            var result = await _userManager.CreateAsync(user, register.Password);
            if(!result.Succeeded)
            {
                return BadRequest(new
                {
                    Error = result.Errors
                });
            }
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var validator = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailConfirmationToken));
            // Provide front-end url [To be implemented]
            var url = $"{Settings.BASE_URL}/user/confirmemail/{user.Id}?validator={validator}";
            var message = new MimeMessage();
            message.Sender =  MailboxAddress.Parse(_configuration["Email:UserName"]);
            message.To.Add(MailboxAddress.Parse(user.Email));
            message.Subject = "Confirm your email";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = @$"
                                        <h4>Hi, {user.Name}!</h4>
                                        <div>Click on the below link to confirm your email.</div>
                                        <div><a href='{url}' target='_blank'>{url}</a></div>
                                    ";

            message.Body = bodyBuilder.ToMessageBody();
            await _mailService.SendEmailAsync(message);
            return Ok(new
            {
                Message = "Registration successful. We have sent you a link via email. Click on that link."
            });
        }

        [HttpGet("ConfirmEmail/{userId}")]
        public async Task<IActionResult> ConfirmEmail(string userId, string validator)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return BadRequest(new
                {
                    Error = "User is not found."
                });
            }
            var emailConfirmationToken = Encoding.UTF8.GetString(Convert.FromBase64String(validator));
            var result = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);
            if(!result.Succeeded)
            {
                return BadRequest(new
                {
                    Error = result.Errors.Select(error => error.Description).ToList()
                });
            }

            return await SendResponseWithJwtToken(user);
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Error = ModelState
                });
            }

            var user = await _userManager.FindByEmailAsync(login.Email);
            if(user == null)
            {
                return BadRequest(new
                {
                    Error = "Your account does not exist. Please sign up."
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
            if(!result.Succeeded)
            {
                return BadRequest(new {
                    Error = "Your email or password is not valid. Try again!"
                });
            }

            return await SendResponseWithJwtToken(user, login.RememberMe);
        }

          [HttpPost("Logout")]
          [Authorize]
          public IActionResult Logout()
          {
            Response.Cookies.Delete("Bearer");
            
              return Ok(new
              {
                  Message = "You have been locked out successfully. Visit again."
              });
          }

        [HttpGet("Profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new
                {
                    Error = "User is not found."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userAddress = _context.UserAddress.FirstOrDefault(ua => ua.UserId == userId);

            return Ok(new {
                User = new {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    Address = userAddress,
                    Roles = roles
                }
            });
        }


        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;

            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return BadRequest(new
                {
                    Error = "User is not found."
                });
            }
            var result = await _userManager.ChangePasswordAsync(user, changePassword.OldPassword, changePassword.NewPassword);
            if(!result.Succeeded)
            {
                return BadRequest(new
                {
                    Error = result.Errors.Select(error => error.Description).ToList()
                });
            }
            return Ok(new
            {
                Message = "Your password has been changed successfully."
            });
        }


        [HttpPost("ChangeEmail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmail changeEmail)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new
                {
                    Error = "User is not found."
                });
            }


            var changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmail.NewEmail);
            var validator = Convert.ToBase64String(Encoding.UTF8.GetBytes(changeEmailToken));

            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_configuration["Email:UserName"]);
            message.To.Add(MailboxAddress.Parse(changeEmail.NewEmail));
            message.Subject = "Change email validation";

            //provide a front-end url
            var url = $"{Settings.BASE_URL}/api/Account/ConfirmChangeEmail/{user.Id}?email={changeEmail.NewEmail}&validator={validator}";
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = @$"
                                        <h4>Hi, {user.Name}!</h4>
                                        <div>Click on the below link to change your email.</div>
                                        <div><a href='{url}' target='_blank'>{url}</a></div>
                                    ";
            message.Body = bodyBuilder.ToMessageBody();
            await _mailService.SendEmailAsync(message);
            return Ok(new
            {
                Message = "We have sent you a link via email. Click on that link."
            });
        }

        [HttpGet("ConfirmChangeEmail/{userId}")]
        public async Task<IActionResult> ConfirmChangeEmail(string userId, string email, string validator)
        {            
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return BadRequest(new
                {
                    Error = "User is not found."
                });
            }

            var changeEmailToken = Encoding.UTF8.GetString(Convert.FromBase64String(validator));
            var result= await _userManager.ChangeEmailAsync(user, email, changeEmailToken);
            if(!result.Succeeded)
            {
                return BadRequest(new
                {
                    Error = result.Errors.Select(error => error.Description).ToList()
                });
            }

            user.UserName = email;
            await _userManager.UpdateAsync(user);
            await _userManager.UpdateNormalizedUserNameAsync(user);
 
            return Ok(new
            {
                Message = "Your email is changed successfully."
            });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
            {
                return BadRequest(new
                {
                    Error = "User is not found."
                });
            }

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var validator = Convert.ToBase64String(Encoding.UTF8.GetBytes(passwordResetToken));

            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_configuration["Email:UserName"]);
            message.To.Add(MailboxAddress.Parse(user.Email));
            message.Subject = "Reset password email validation";

            //Provide front-end url
            var url = $"{Settings.BASE_URL}/user/confirmresetpassword/{user.Id}?validator={validator}";
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = @$"
                                        <h4>Hi, {user.Name}!</h4>
                                        <div>Click on the below link to reset your password.</div>
                                        <div><a href='{url}' target='_blank'>{url}</a></div>
                                    ";
            message.Body = bodyBuilder.ToMessageBody();
            await _mailService.SendEmailAsync(message);

            return Ok(new
            {
                Message = "We have sent you an email. Click on that email to reset your password."
            });
        }


        [HttpPost("ConfirmResetPassword/{userId}")]
        public async Task<IActionResult> ConfirmResetPassword(string userId, string validator, [FromBody]ConfirmResetPassword confirmResetPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return BadRequest(new
                {
                    Error = "User is not found."
                });
            }

            var passwordResetToken = Encoding.UTF8.GetString(Convert.FromBase64String(validator));
            var result = await _userManager.ResetPasswordAsync(user, passwordResetToken, confirmResetPassword.NewPassword);
            if(!result.Succeeded)
            {
                return BadRequest(new
                {
                    Error = result.Errors.Select(error => error.Description).ToList()
                });
            }

            return Ok(new
            {
                Message = "Your password is reset successfully."
            });
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> UpdateUserFullName([FromBody]UpdateUserFullName updateUserFullName)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;

            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return BadRequest();
            }

            user.Name = updateUserFullName.Name;
            await _userManager.UpdateAsync(user);
            return Ok(user.Name);
        }

        private async Task<IActionResult> SendResponseWithJwtToken(User user, bool rememberMe = false)
        {
            var roles = new List<string>();
            if (user.EmailConfirmed && Settings.ADMIN_EMAILS.Any(email => string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                }

                if (!await _userManager.IsInRoleAsync(user, UserRoles.Admin))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                }
                roles.Add(UserRoles.Admin);
            }

            var claims = new List<Claim>();
            claims.Add(new Claim("UserId", user.Id));
            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var expires = DateTime.UtcNow.AddDays(365);
            var jwtToken = _jwtTokenGenerator.GetJwtToken(
                claims: claims,
                expires: expires
                );

            if(rememberMe)
            {
                Response.Cookies.Append("Bearer", jwtToken, new CookieOptions
                {
                    MaxAge = TimeSpan.FromDays(365)
                });
            }
            else
            {
                Response.Cookies.Append("Bearer", jwtToken);
            }

            var userAddress = _context.UserAddress.FirstOrDefault(ua => ua.UserId == user.Id);

            return Ok(new
            {
                JwtToken = jwtToken,
                Expires = expires,
                User = new
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    Address = userAddress,
                    Roles = roles
                }
            });
        }
    }
}
