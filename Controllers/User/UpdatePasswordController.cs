
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

using Dapper;
using System.Data.SqlClient;
using Thu6.model;
using Thu6.Controllers.Register_and_Login;

namespace Thu6.Controllers.User
{
    [Route("user/update_password")]
    [ApiController]
    public class UpdatePasswordController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;

        public UpdatePasswordController(IConfiguration config, IConfiguration redis)
        {
            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }

        [HttpPut("{user_id}/{token}")]
        public async Task<ActionResult> ResetPassword(string oldPassword, string newPassword, string token, string user_id)
        {
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("Test")))
            {
                var redisDatabase = _configRedis.GetDatabase();
                string value = redisDatabase.StringGet(token + user_id);
                var check = await conn.QueryFirstOrDefaultAsync<Users>("select * from [user] where user_id=@user_id", new { user_id });
                if (value != null)
                {
                    if (check != null)
                    {
                        if (oldPassword == check.Password)
                        {
                            var user = await conn.QueryFirstOrDefaultAsync<Users>("UPDATE [user] SET [password] = @newPassword WHERE user_id=@user_id", new { user_id, newPassword });
                            return Ok("pass");
                        }
                        else return BadRequest("pass trung");
                    }
                    else return BadRequest("user id sai");

                }
                else return BadRequest("token sai");

            }
        }
   



    }
}
