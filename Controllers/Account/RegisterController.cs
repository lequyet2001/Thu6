using Dapper;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Data.SqlClient;
using System.Net.Mail;
using Thu6.Controllers.SendEmail;
using Thu6.model;
namespace Thu6.Controllers.Register
{
    [Route("account/register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ConnectionMultiplexer _configRedis;
        public RegisterController(IConfiguration config, IConfiguration redis)
        {
            _config = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }

        [HttpGet]
        public async Task<ActionResult<List<Users>>> GetAllUser()
        {
            using var conn = new SqlConnection(_config.GetConnectionString("Test"));
            var User = await conn.QueryAsync<Users>("select * from [user]");
            return Ok(User);
        }
        [HttpPost]
        public async Task<ActionResult<List<Users>>> Register(Users model)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_config.GetConnectionString("Test"));

                IDatabase redisDatabase = _configRedis.GetDatabase();
                F f = new F();
                string token = f.GenerateSecureToken(64);
                // Check if the user with the given email already exists
                string checkQuery = "SELECT COUNT(*) FROM [user] WHERE email = @Email";
                int userCount = await conn.ExecuteScalarAsync<int>(checkQuery, new { model.Email });

                if (userCount > 0)
                {
                    // User with the same email already exists
                    return BadRequest("User with the same email already exists.");
                }

                // Generate a random ID using GUID
                string randomId = Guid.NewGuid().ToString();

                // Insert the new user into the database
                string insertQuery = @"INSERT INTO [user] (user_id, user_name, password, email, status, created_at, modified_at)
                               VALUES (@User_id, @User_name, @Password, @Email, @Status, @Created_at, @Modified_at)";
                var parameters = new
                {
                    User_id = randomId,
                    model.User_name,
                    model.Password,
                    model.Email,
                    Status = 0, // Assuming 0 means inactive status
                    Created_at = DateTime.Now,
                    Modified_at = DateTime.Now
                };
                await conn.ExecuteAsync(insertQuery, parameters);

                // Send the activation email (uncomment this code when you have the SendActivateAccount method implemented)
                SendActivateEmailController saec = new(_config, _config);
                saec.SendActivateAccount(model.Email, token);

                // Return the list of all users (you may need to modify this depending on your implementation)
                var value = new { status = 1, data = new { }, Message = " Register Success!" };
                return Ok(value);
            }
            catch (SqlException ex)
            {
                // Handle any SQL exception that occurs during database operations
                Console.WriteLine($"Database error: {ex.Message}");
                return StatusCode(500, "Internal Server Error: "+ex.Message);
            }
            catch (SmtpException ex)
            {
                // Handle any SMTP exception that occurs during email sending
                Console.WriteLine($"Failed to send activation email: {ex.Message}");
                return StatusCode(500, "Failed to send activation email");
            }
        }





    }
}



