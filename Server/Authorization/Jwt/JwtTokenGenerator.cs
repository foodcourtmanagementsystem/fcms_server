using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Authorization.Jwt
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetJwtToken(List<Claim> claims, DateTime expires)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var secretKey = jwtSection.GetValue<string>("SecretKey");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                    issuer: jwtSection.GetValue<string>("Issuer"),
                    audience: jwtSection.GetValue<string>("Audience"),
                    claims: claims,
                    notBefore: DateTime.Now,
                    expires: expires,
                    signingCredentials: signingCredentials
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        }
    }
}
