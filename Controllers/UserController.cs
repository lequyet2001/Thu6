using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Thu6.model;

namespace Thu6.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IConfiguration _Configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public UserController(IConfiguration config, IConfiguration redis)
        {
            _Configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }



        [HttpPut]
        [Route("update_account")]
        public IActionResult UpdatePassword(updatePasswordModel model)
        {

            try
            {

                var dataAccess = new DataAccess(_Configuration);

                if (!IsValidToken(model.Token))
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Token không hợp lệ."
                    });
                }
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper cryptoHelper = new CryptoHelper();

                var id_user = cryptoHelper.Decrypt(model.Token, "CreateTokenLogin");
                var user=dataAccess.Query<Users>("select * from [user] where user_id=@id_user", new {id_user}).FirstOrDefault();
                if (user == null) {
                    return BadRequest(new
                    {
                        Status = 2,
                        data=id_user,
                        Message = "Token sai."
                    });
                }
                if (user?.Password != model.OldPassword)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Mật khẩu cũ sai."
                    });
                }
                if(user?.Password == model.NewPassword)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Mật khảu mới không được trùng với mật khẩu cũ."
                    });
                }

                string newPassword = model.NewPassword;
                user.Password = newPassword;
                dataAccess.UpdateUserWithPassword(user);

                return Ok(
                    new
                    {
                        Status = 1,
                        Message = "Đổi mật khẩu thành công"
                    }
                    );
                



            }catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    status = -1
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
            }
            else return false;

        }



        [HttpPatch]
        [Route("Update_user_infor")]
        public IActionResult UpdateUserInfor([FromForm]UpdateUserInforModel model)
        {
            try
            {
                var dataAccess = new DataAccess(_Configuration);

                if (!IsValidToken(model.Token))
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Token không hợp lệ."
                    });
                }
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper cryptoHelper = new CryptoHelper();

                var id_user = cryptoHelper.Decrypt(model.Token, "CreateTokenLogin");
                var user = dataAccess.Query<Users>("select * from [user] where user_id=@id_user", new { id_user }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        data = id_user,
                        Message = "Token sai."
                    });
                }


                user.Avatar = model.Avatar;
                user.Position = model.Position;
                user.Birthday = model.Birthday;
                user.Full_name = model.Full_name;
                user.Display_name = model.Display_name;

                _ = dataAccess.UpdateUserInfor(user);

                return Ok(new
                {
                    Status = 1,
                    Message = "Thay đổi thông tin người dùng thành công"
                });
            }
            catch(Exception ex) 
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    status = -1
                });
            }


        }
    }
}
