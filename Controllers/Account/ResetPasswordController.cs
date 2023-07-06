
using Microsoft.AspNetCore.Mvc;

using System.Data.SqlClient;
using StackExchange.Redis;
using Dapper;

namespace Thu6.Controllers.Account
{
    [Route("account/reset_password")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;


        public ResetPasswordController(IConfiguration config, IConfiguration redis)
        {
            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }




        [HttpPut("reset")]
        public async Task<ActionResult> ActivateAccount(string Email, string verificationToken, string Password)
        {
            var redisDatabase = _configRedis.GetDatabase();

            using var conn = new SqlConnection(_config.GetConnectionString("Test"));
            string key = "reset_Password: " + Email;
            string value = redisDatabase.StringGet(key);
      
            if (value == verificationToken)
            {
                _ = await conn.QueryAsync("UPDATE [user] SET [password] = @Password WHERE email=@Email", new { Email, Password });
                _ = redisDatabase.KeyDelete(key);
            }

            _configRedis.Close();

            return Ok(new { }); ;
        }
    }
}
