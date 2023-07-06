using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Data.SqlClient;
using Thu6.model;

namespace Thu6.Controllers.User
{
    [Route("user/update_user_info")]
    [ApiController]
    public class UpdateUsertInforController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;

        public UpdateUsertInforController(IConfiguration config, IConfiguration redis)
        {
            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }

        [HttpPatch("{user_id}/{token}")]
        public async Task<ActionResult> UpdateUser(string token, string user_id, string displayName, string fullName, DateTime birthday, string position, string avatar)
        {
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("Test")))
            {
                var redisDatabase = _configRedis.GetDatabase();
                _ = redisDatabase.StringGet(token + user_id);
                var checkUser = await conn.QueryFirstOrDefaultAsync<Users>("select * from [user] where user_id=@user_id", new { user_id });
                if (checkUser != null)
                {
                    var updateUserInfor = await conn.QueryAsync<Users>("UPDATE [user] SET [display_name] = @displayName, [full_name]=@fullName, [birthday]=@birthday, [position]=@position, [avatar]=@avatar WHERE user_id=@user_id", new { displayName, fullName, birthday, position, avatar, user_id });
                    if (updateUserInfor != null)
                    {
                    return Ok(checkUser);

                    }
                    return BadRequest("fail");
                }



                return BadRequest("lol");
            }
        }



    }
}
