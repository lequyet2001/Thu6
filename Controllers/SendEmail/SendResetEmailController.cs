using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using StackExchange.Redis;

namespace Thu6.Controllers.SendEmail
{
    [Route("account/send_reset_email")]
    [ApiController]
    public class SendResetEmailController : ControllerBase
    {


        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;


        public SendResetEmailController(IConfiguration config, IConfiguration redis)
        {


            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }


        [HttpPost]
        public ActionResult SendResetPassword(string email, string body)
        {
            var redisDatabase = _configRedis.GetDatabase();
            redisDatabase.StringSet("user: " + email, body);
            // Create a new MailMessage object
            var smtpClient = new SmtpClient("smtp-mail.outlook.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            // Cung cấp thông tin đăng nhập của tài khoản Microsoft
            smtpClient.Credentials = new NetworkCredential("q2602@outlook.com", "Quyet6a1haha");

            // Tạo đối tượng MailMessage và cấu hình email
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("q2602@outlook.com");
            mailMessage.To.Add("quyet8c2@gmail.com");
            mailMessage.Subject = "Subject of the email";
            mailMessage.Body = @"https://localhost:7185/account/forgot_password/reset?" + "user=" + email + "&model=" + body;

            try
            {
                // Gửi email
                smtpClient.Send(mailMessage);
                // Email đã được gửi thành công
                return Ok(new { Status = 0, Message = "Success" });
            }
            catch (SmtpException ex)
            {
                // Xử lý lỗi gửi email

                return BadRequest(new { Status = -1, ex.Message });
            }



        }
    }
}
