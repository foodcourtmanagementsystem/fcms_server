using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Server.Services
{
    public class MailService
    {
        private readonly IConfiguration _configuration;
        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(MimeMessage message)
        {     
           using(var smtp = new SmtpClient())
           {
                smtp.Connect(_configuration["Email:Host"], 
                    int.Parse(_configuration["Email:Port"]), 
                    SecureSocketOptions.StartTls);
                smtp.Authenticate(_configuration["Email:UserName"], _configuration["Email:Password"]);
                await smtp.SendAsync(message);
                smtp.Disconnect(true);
           }
        }
    }
}
