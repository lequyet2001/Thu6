using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using Thu6.model;

namespace Thu6.Controllers
{
    [Route("api/dictionary")]
    [ApiController]
    public class DictionaryController : ControllerBase
    {
        private readonly IConfiguration _Configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public DictionaryController(IConfiguration config, IConfiguration redis)
        {
            _Configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }



        [HttpGet]
        [Route("get_list_dictionary")]
        public IActionResult GetListDictionary([FromHeader][Required] string token)
        {
            try
            {
            DataAccess dataAccess = new DataAccess(_Configuration);
             
                if (!IsValidToken(token))
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Token không hợp lệ."
                    });
                }
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper cryptoHelper = new CryptoHelper();
                var id_user = cryptoHelper.Decrypt(token, "CreateTokenLogin");
                var user = dataAccess.Query<Users>("select * from [user] where user_id=@id_user", new { id_user }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        data = id_user,
                        Message = "người dùng đã đang xuat."
                    });
                }
                var x = dataAccess.QueryList<Dictionary>("select * from [dictionary] where user_id=@id_user", new { id_user });
                var _ = dataAccess.Query<Dictionary>(@"update [dictionary] set last_view_at=@last_view_at where user_id=@id_user", new { last_view_at=DateTime.Now,id_user});
                var data = new List<object>();
                x.ForEach(e =>
                {
                    var a = new
                    {
                        e.Dictionary_id,
                        e.Dictionary_name,
                        e.Last_view_at
                    };
                    data.Add(a);
                });

                return Ok(new
                {
                    Status = 1,
                    Message = "thành công",
                    data 
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


        [HttpGet]
        [Route("load_dictionary")]
        public IActionResult GetDictionaryByID([FromHeader][Required] string token, [Required] string dictionaryID)
        {
            try
            {
                DataAccess dataAccess = new DataAccess(_Configuration);

                if (!IsValidToken(token))
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Token không hợp lệ."
                    });
                }
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper cryptoHelper = new CryptoHelper();
                var id_user = cryptoHelper.Decrypt(token, "CreateTokenLogin");
                var user = dataAccess.Query<Users>("select * from [user] where user_id=@id_user", new { id_user }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        data = id_user,
                        Message = "người dùng đã đang xuat."
                    });
                }
                var data = dataAccess.Query<Dictionary>("select * from [dictionary] where dictionary_id=@dictionaryID", new { dictionaryID }).FirstOrDefault();
                var _ = dataAccess.Query<Dictionary>(@"update [dictionary] set last_view_at=@last_view_at where user_id=@id_user", new { last_view_at = DateTime.Now, id_user });

                return Ok(new
                {
                    Status = 1,
                    Message = "thành công",
                    data = new
                    {
                        data.Dictionary_id,
                        data.Dictionary_name,
                        data.Last_view_at
                    }
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


        [HttpPost]
        [Route("add_dictionary")]
        public IActionResult AddDictionary([Required] string dictionaryName,string? cloneDictionaryId, [FromHeader][Required] string token)
        {
            try
            {
                DataAccess dataAccess = new DataAccess(_Configuration);

                if (!IsValidToken(token))
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Token không hợp lệ."
                    });
                }
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper cryptoHelper = new CryptoHelper();
                var id_user = cryptoHelper.Decrypt(token, "CreateTokenLogin");
                var user = dataAccess.Query<Users>("select * from [user] where user_id=@id_user", new { id_user }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        data = id_user,
                        Message = "người dùng đã đang xuat."
                    });
                }
                var dictionary_id=cryptoHelper.GenerateRandomId();
                if (cloneDictionaryId == null)
                {
                var data = dataAccess.Query<Dictionary>(@"insert into [dictionary] (dictionary_id,user_id,dictionary_name,last_view_at,modified_at,created_at) values(@dictionary_id,@id_user,@dictionaryName,@last_view_at,@last_view_at,@last_view_at) ", new { id_user ,dictionaryName, dictionary_id,last_view_at=DateTime.Now});

                return Ok(new
                {
                    Status = 1,
                    Message = "Tạo từ điển thành công",
                   
                });
                }
                else
                {
                    var findD = dataAccess.Query<Dictionary>(@"select * from [dictionary] where dictionary_id=@dictionary_id", new { dictionary_id = cloneDictionaryId }).FirstOrDefault();
                    var data = dataAccess.Query<Dictionary>(@"insert into [dictionary] (dictionary_id,user_id,dictionary_name,last_view_at,modified_at,created_at) values(@dictionary_id,@id_user,@dictionaryName,@last_view_at,@last_view_at,@last_view_at) ", new { id_user, dictionaryName, dictionary_id=findD.Dictionary_id, last_view_at = DateTime.Now });
                    return Ok(new
                    {
                        Status = 1,
                        Message = "Sao chép từ điển thành công",

                    });

                }

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

        [HttpPatch]
        [Route("update_dictionary")]
        public IActionResult UpdateDictionary([FromHeader][Required] string token , string dictionaryId,string dictionaryName)
        {
            try
            {
                DataAccess dataAccess = new DataAccess(_Configuration);

                if (!IsValidToken(token))
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        Message = "Token không hợp lệ."
                    });
                }
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper cryptoHelper = new CryptoHelper();
                var id_user = cryptoHelper.Decrypt(token, "CreateTokenLogin");
                var user = dataAccess.Query<Users>("select * from [user] where user_id=@id_user", new { id_user }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        data = id_user,
                        Message = "người dùng đã đang xuat."
                    });
                }
                var dictionary = dataAccess.Query<Dictionary>(@"select * from [dictionary] where user_id=@id_user and dictionary_id=@dictionaryId", new { id_user,dictionaryId}).FirstOrDefault();
                if(dictionary == null)
                {
                    return BadRequest(new
                    {
                        Status = 2,
                        data = id_user,
                        Message = "Id từ diển sai."
                    });
                }

                var _= dataAccess.Query<Dictionary>(@"update [dictionary] set dictionary_name=@dictionaryName where user_id=@id_user and dictionary_id=@dictionaryId", new { dictionaryId, id_user, dictionaryName }).FirstOrDefault();



                return Ok(new
                {

                    Status = 1,
                    Message = "Cập nhật từ điển thành công",
                }
                    
                    );
            }catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = ex.Message,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    ErrorCode = -1
                });
            }


        }
        [HttpDelete]
        [Route("delete_dictionary")]
        public IActionResult DeleteDictionary(string id)
        {

            try
            {
                return Ok();
            }catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = ex.Message,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    ErrorCode = -1
                });
            }
        }


    }
}
