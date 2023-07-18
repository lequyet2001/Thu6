using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Thu6.model;

using System.Net.Mail;
using System.Net;
using System.Web;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Firebase.Auth;
using Microsoft.JSInterop;

namespace Thu6.Controllers


{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
       
        private readonly IConfiguration _Configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public AccountController(IConfiguration config, IConfiguration redis)
        {
            _Configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
          
        }


        [HttpGet("getAll")]
        public ActionResult getAllKeyValue()
        {
            var redisDatabase = _configRedis.GetDatabase();
            var dataAccess = new DataAccess(_Configuration);
            var keys = _configRedis.GetServer("localhost", 6379).Keys();

            var values = new List<string>();
            var k = new List<string>();
            foreach (var key in keys)
            {
                var value = redisDatabase.StringGet(key);
                values.Add(value);
                values.Add("_________________________");

            }
            return Ok(new { values });
        }

     



        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegisterModel model)
        {
            try
            {
                // Khởi tạo DataAccess


                var dataAccess = new DataAccess(_Configuration);
        
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper f = new CryptoHelper();
                string id = f.GenerateRandomId();

                // Kiểm tra xem người dùng có tồn tại trong cơ sở dữ liệu hay không
                var existingUser = dataAccess.Query<Users>("SELECT * FROM [user] WHERE email = @Email", new {model.Email }).FirstOrDefault();
                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Địa chỉ email đã được đăng ký"
                    });
                }

                // Thực hiện đăng ký tài khoản
                var newUser = new Users
                {
                    User_id = id,
                    Email = model.Email,
                    Password = model.Password,

                };

                var result = dataAccess.AddUser(newUser);
                if (result > 0)
                {
                    return Ok(new
                    {
                        Status = 1,
                        Data = dataAccess.Query<Users>("SELECT * FROM [user] WHERE email = @Email", new { model.Email }).FirstOrDefault(),
                      
                        Message = "Đăng ký tài khoản thành công"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Đăng ký tài khoản thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    status = -1
                });
            }
        }

        [HttpPost]
        [Route("send_activate_email")]
        public IActionResult SendActivateEmail(RegisterModel model)
        {
            try
            {

                var dataAccess = new DataAccess(_Configuration);




                var existingUser = dataAccess.Query<Users>("SELECT * FROM [user] WHERE email = @Email and password = @Password", new
                {
                    model.Email,
                    model.Password
                }).FirstOrDefault();
                if (existingUser?.Status == 1 || existingUser ==null)
                {
                    return BadRequest(new
                    {
                        code = 1002,
                        Status = 2,
                        Message = "Tài khoản không hợp lệ"
                    });
                }

                var redisDatabase = _configRedis.GetDatabase();
                CryptoHelper rSAEncryption = new CryptoHelper();
                var token = rSAEncryption.Encrypt(existingUser.User_id, "mFgyS6qJLNjwCXpk");
                redisDatabase.StringSet(existingUser.User_id, token);
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
              
                mailMessage.Body = token;
                smtpClient.Send(mailMessage);
                var n = rSAEncryption.Decrypt(token, "mFgyS6qJLNjwCXpk");
                return Ok(new
                {
                    data= n,
                    Status = 1,
                    Message = "Email kích hoạt đã được gửi"
                });


            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Exception = ex.Message,
                    
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    status = -1
                });
            }


        }

        [HttpGet]
        [Route("activate_account")]
        public IActionResult ActivateAccount([FromHeader] string token)
        {
            try
            {
                // Kiểm tra tính hợp lệ của mã thông báo
                if (!IsValidToken(token))
                {
                    return BadRequest(new 
                    {
                        Status = 2,
                        Message = "Token không hợp lệ"
                    });
                }

                // Kích hoạt tài khoản
                if (ActivateUserAccount(token))
                {
                    return Ok(new 
                    {
                        Status = 1,
                        Message = "Tài khoản đã được kích hoạt thành công"
                    });
                }
                else
                {
                    return BadRequest(new 
                    {
                        Status = 2,
                        Message = "Không thể kích hoạt tài khoản"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                {
                    Status =ex.Message,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    ErrorCode = -1
                });
            }
        }

        private bool IsValidToken(string token)
        {
            var redisDatabase = _configRedis.GetDatabase();

            var keys = _configRedis.GetServer("localhost", 6379).Keys();

            var values = new List<string>();
            var k = new List<string>();
            foreach (var key in keys)
            {
                var value = redisDatabase.StringGet(key);
                values.Add(value);
            }
            if (values.Contains(token))
            {
                return true;
            }else return false;

        }

        private bool ActivateUserAccount(string token)
        {
            
            CryptoHelper cryptoHelper = new CryptoHelper();
            var user_id = cryptoHelper.Decrypt(token, "mFgyS6qJLNjwCXpk");
            var dataAccess = new DataAccess(_Configuration);
            var user =dataAccess.Query<Users>("select * from [user] where user_id=@user_id", new {user_id}).FirstOrDefault();
            if (user != null)
            {
            user.Status = 1;
                dataAccess.UpdateUser(user);
                var redis = _configRedis.GetDatabase();
                redis.KeyDelete(user_id);
                return true;
            }return false;

        }


        [HttpGet]
        [Route("forgot_password")]
        public IActionResult ForgotPassword(string email)
        {
            try
            {
                
                // Kiểm tra tính hợp lệ của địa chỉ email
                if (!Helper.IsValidEmail(email))
                {
                    return BadRequest(new 
                    {
                        Status = 2,
                        Message = "Địa chỉ email không hợp lệ"
                    });
                }
                var dataAccess = new DataAccess(_Configuration);
                // Kiểm tra xem địa chỉ email có tồn tại trong hệ thống hay không
                var existingUser = dataAccess.Query<Users>("SELECT * FROM [user] WHERE email = @Email", new { Email = email }).FirstOrDefault();
                if (existingUser == null)
                {
                    return BadRequest(new 
                    {
                        Status = 2,
                        Message = "Địa chỉ email không tồn tại"
                    });
                }

                // Tạo mã thông báo (token) để định danh yêu cầu khôi phục mật khẩu
                CryptoHelper cryptoHelper = new CryptoHelper();

                var token = cryptoHelper.Encrypt(existingUser.User_id, "ResetPasswordWit");

                // Lưu mã thông báo vào cơ sở dữ liệu hoặc lưu vào bộ nhớ cache tạm thời
                var redisDatabase = _configRedis.GetDatabase();
                redisDatabase.StringSet(existingUser.User_id, token);

                // Gửi email chứa liên kết (URL) có chứa mã thông báo để người dùng có thể khôi phục mật khẩu
                var smtpClient = new SmtpClient("smtp-mail.outlook.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;

                // Cung cấp thông tin đăng nhập của tài khoản Microsoft
                smtpClient.Credentials = new NetworkCredential("q2602@outlook.com", "Quyet6a1haha");

                // Tạo đối tượng MailMessage và cấu hình email
                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("q2602@outlook.com");
                mailMessage.To.Add("quyet8c2@gmail.com");
                mailMessage.Subject = "Forgot Password ";
                mailMessage.Body = token;
                smtpClient.Send(mailMessage);

                return Ok(new 
                {
                    Status = 1,
                    data=0,
                    Message = "Email khôi phục mật khẩu đã được gửi"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                {
                    Status = ex.Message,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    ErrorCode = -1
                });
            }
        }


        [HttpPut]
        [Route("reset_password")]
        public IActionResult ResetPassword(string token, string newPassword)
        {
            try
            {
                // Kiểm tra tính hợp lệ của mã thông báo
                if (!IsValidToken(token))
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Token không hợp lệ"
                    });
                }
                var dataAccess = new DataAccess(_Configuration);
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper f = new CryptoHelper();
                string user_id = f.Decrypt(token, "ResetPasswordWit");
                var existingUser = dataAccess.Query<Users>("SELECT * FROM [user] WHERE user_id = @user_id", new { user_id }).FirstOrDefault();
                if (existingUser == null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Người dùng không tồn tại."
                    });
                }
                if(existingUser.Password == newPassword)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Mật khẩu mới không được trùng mật khẩu cũ."
                    });
                }

                 existingUser.Password = newPassword;
                dataAccess.UpdateUserWithPassword(existingUser);
                redisDatabase.KeyDelete(user_id);
                return Ok(new
                {
                    Status = 1,

                    Message = "Đổi mật khẩu thành công"
                });


            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    status = -1
                });
            }
        }



        
    }
}