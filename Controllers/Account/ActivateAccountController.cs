
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Data.SqlClient;


using Dapper;

namespace Thu6.Controllers
{
    [Route("account/activate_account")]
    [ApiController]
    public class ActivateAccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;


        public ActivateAccountController(IConfiguration config, IConfiguration redis)
        {
            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }

        [HttpPut("verify")]
        public async Task<ActionResult> ActivateAccount(string Email,string verificationToken)
        {
            var redisDatabase = _configRedis.GetDatabase();

            using var conn = new SqlConnection(_config.GetConnectionString("Test"));
            string key = "user: " + Email;
            string value= redisDatabase.StringGet(key);
           
            if (value == verificationToken)
            {
                await conn.QueryAsync("UPDATE [user] SET [status] = 1 WHERE email=@Email", new { Email });
                return Ok("pass");
            }
                

                _configRedis.Close();
             return BadRequest("nopass"+ new {value });
        }

        [HttpGet]
        public ActionResult getAllKeyValue()
        {
            var redisDatabase = _configRedis.GetDatabase();

            var keys = _configRedis.GetServer("localhost", 6379).Keys();

            var values = new List<string>();
            var k = new List<string>();
            foreach (var key in keys)
            {
                var value = redisDatabase.StringGet(key);
                values.Add("["+key+"]:   "+value);
                values.Add("_________________________");
                         
            }
            return Ok(new { values });
        }

    }
}
