using System.Net;
using System.Net.Mail;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Thu6.Controllers.SendEmail
{
    [Route("account/send_activate_email")]
    [ApiController]
    public class SendActivateEmailController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;


        public SendActivateEmailController(IConfiguration config, IConfiguration redis)
        {


            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }
        [HttpPost]
        public ActionResult SendActivateAccount(string Email, string body)
        {
            var redisDatabase = _configRedis.GetDatabase();
            redisDatabase.StringSet("user: " + Email, body);
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
            string newbody = HttpUtility.UrlEncode(body);
            string newEmail= HttpUtility.UrlEncode(Email).ToLowerInvariant();
            mailMessage.Body = "https://localhost:7185/account/activate_account/verify?" + "Email=" + newEmail + "&verificationToken=" + newbody;
                               
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
