using Dapper;
using Firebase.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Data.SqlClient;
using System.Security.Cryptography;
using Thu6.Controllers.SendEmail;
using Thu6.model;

namespace Thu6.Controllers.Account
{
    [Route("account/forgot_password")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;
        public ForgotPasswordController(IConfiguration config, IConfiguration redis)
        {
            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string Email)
        {

            using SqlConnection conn = new SqlConnection(_config.GetConnectionString("Test"));
            IEnumerable<Users> check = await conn.QueryAsync<Users>("select * from [user] where email=@Email", new { Email });

            if (check != null)
            {
                F f = new();
                string token = f.GenerateSecureToken(64);
                IDatabase redisDatabase = _configRedis.GetDatabase();
                _ = redisDatabase.StringSet("reset_Password: " + Email, token);
                SendResetEmailController srec = new(_config, _config);
                _ = srec.SendResetPassword(Email, token);
                return Ok(new
                {
                    Status = 1,
                    Message = "",
                    code = 0,
                    ErroCode = 0,
                    Data = 0
                });

            }


            return Ok(new
            {
                Status = 0,
                Message = "Tài khoản không hợp lệ (không tồn tại trong hệ thống)",
                code = 0,
                ErroCode = 1002,
                Data = 0
            });
        }
    }
}
