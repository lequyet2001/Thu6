using MailKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using StackExchange.Redis;
using Thu6.model;

[ApiController]
[Route("api/account")]
public class AuthController : ControllerBase
{
   
    private readonly IConfiguration _Configuration;
    private readonly ConnectionMultiplexer _configRedis;
    public AuthController(IConfiguration config, IConfiguration redis)
    {
        _Configuration = config;
        string redisConnectionString = redis.GetConnectionString("Redis");
        _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
      
    }

    [HttpPost]
    [Route("login")]
    public IActionResult Login(LoginModel model)
    {
        try
        {
            return NewMethod(model);
        } catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Exception = ex.Message,

                Message = "Đã xảy ra lỗi trong quá trình xử lý",
                status = -1
            });
        }
    }

    private IActionResult NewMethod([FromBody] LoginModel model)
    {
        var dataAccess = new DataAccess(_Configuration);
        var user = dataAccess.Query<Users>("SELECT * FROM [user]  WHERE email = @Email and password = @Password", new
        {
            model.Email,
            model.Password
        }).FirstOrDefault();



        if (user == null)
        {
            return BadRequest(new
            {
                status = 2,
                Message = "người dùng không tồn tại hoặc mật khẩu sai",
            });
        }
        else
        {
            var dictionary = dataAccess.Query<Dictionary>(@"select Top 1 * from dictionary where user_id=@User_id ORDER BY modified_at DESC", new {user.User_id });

            CryptoHelper cryptoHelper = new CryptoHelper();
            var id_login = user.User_id;
            var token = cryptoHelper.Encrypt(id_login, "CreateTokenLogin");
            var redisDatabase = _configRedis.GetDatabase();
            redisDatabase.StringSet(id_login, token);

            return Ok(new
            {
                Status = 1,
                Message = "Đăng nhập thành công",
                Data = new
                {
                    Token = token,
                    UserId = user.User_id,
                    UserName = user.Email,
                    DisplayName = user.Display_name,
                    user.Avatar,
                    user.DictionaryId,
                    user.DictionaryName,

                }
            });
        }
    }


    [HttpGet]
    [Route("logout")]
    public IActionResult Logout(string token)
    {
        try
        {
            return NewMethod2(token);
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

    private IActionResult NewMethod2(string token)
    {



        CryptoHelper cryptoHelper = new CryptoHelper();
        var id_login = cryptoHelper.Decrypt(token, "CreateTokenLogin");
        var redisDatabase = _configRedis.GetDatabase();
        redisDatabase.KeyDelete(id_login);


        return Ok(new
        {
            
            Status = 1,
            Message = "Đăng xuất thành công",
            Data = 0

        });
    }

}
