using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Data.SqlClient;
using Thu6.Controllers.SendEmail;
using Thu6.model;

namespace Thu6.Controllers.Register_and_Login
{
    [Route("account/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;
        public LoginController(IConfiguration config, IConfiguration redis)
        {
            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
        }


        [HttpPost, AllowAnonymous]
        public async Task<ActionResult> LoginAsync(string Email, string Password)
        {
            try
            {
                using var conn = new SqlConnection(_config.GetConnectionString("Test"));
                var redisDatabase = _configRedis.GetDatabase();
                string value = redisDatabase.StringGet("user: " + Email);
                F f = new F();
                string token = f.GenerateSecureToken(64);
                string key = f.GenerateSecureToken(20);
                string query = "SELECT * FROM [user] WHERE email = @Email AND password = @Password";
                Users user = await conn.QueryFirstOrDefaultAsync<Users>(query, new { Email, Password });
                redisDatabase.StringSet(key + user.User_id, token);  //  

                if (user == null)
                {   
                    return BadRequest(new { 
                    message= "Invalid email or password.",
                    code=1001
                    }
                    );
                }
                if (user.Status == 0)
                {
                    SendActivateEmailController saec = new(_config, _config);
                    saec.SendActivateAccount(user.Email, value);
                    return BadRequest(new
                    {
                        message = "Tài khoản chưa được kích ",
                        code = 1004
                    }
                  );
                }

                string query2 = @"
            SELECT d.dictionary_id, d.user_id, d.dictionary_name, d.last_view_at
            FROM [dictionary] d
            JOIN [user] u ON u.user_id = d.user_id
            WHERE u.user_id = @User_id
            ORDER BY d.last_view_at DESC";

                Dictionary d = await conn.QueryFirstOrDefaultAsync<Dictionary>(query2, new { user.User_id });

                return Ok(new
                {
                    Status = 1,
                    Message = "Login Success",
                    code = 0,
                    ErroCode = 0,
                    Data = new
                    {
                        Token = key ,
                        user.User_id,
                        user.User_name,
                        user.Display_name,
                        user.Avata,
                        d?.Dictionary_id,
                        d?.Dictionary_name
                    }
                });
            }
            catch (SqlException ex)
            {
                // Handle SQL Server exception
                // You can log the error, return a specific error message, or perform any other necessary action
                return BadRequest("An error occurred while executing the SQL query." +ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                // You can log the error, return a specific error message, or perform any other necessary action
                return BadRequest("An unexpected error occurred." + ex.Message);
            }
        }



    }
}
