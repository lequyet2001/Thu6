using Dapper;
using HUST.Core.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Data;
using Dapper;
using Thu6.model;
using Newtonsoft.Json.Linq;

namespace Thu6.Controllers
{
    [Route("api/auditlog")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {

        private readonly IConfiguration _Configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public AuditLogController(IConfiguration config, IConfiguration redis)
        {
            _Configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);

        }



        [HttpGet]
        [Route("get_logs")]
        public IActionResult GetLogs(string token, string searchFilter,
            int pageIndex, int pageSize, DateTime dateFrom, DateTime dateTo) {
            try
            {
                var dataAccess = new DataAccess(_Configuration);
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
                        Message = "Token sai."
                    });
                }

                var full =dataAccess.QueryList<audit_log>(@"SELECT *
                                                                FROM (
                                                                  SELECT ROW_NUMBER() OVER (ORDER BY created_at DESC) AS RowNumber, *
                                                                  FROM audit_log
                                                                  WHERE action_type LIKE '%' + @SearchFilter + '%'
                                                                  AND CONVERT(date, created_at) BETWEEN @DateFrom AND @DateTo
                                                                ) AS TempTable
                                                                WHERE RowNumber > (@PageIndex - 1) * @PageSize AND RowNumber <= @PageIndex * @PageSize and user_id=@user_id;
                                                                ", 
                                        new 
                                        { 
                                            DateFrom=dateFrom,
                                            DateTo=dateTo,
                                            PageIndex=pageIndex ,
                                            PageSize=pageSize,
                                            SearchFilter=searchFilter,
                                            user_id=id_user
                                        });


                var ffull = dataAccess.Query<audit_log>(@"SELECT 
                                                                (SELECT COUNT(*) FROM audit_log
                                                                                WHERE user_id = @user_id AND action_type LIKE '%' + @SearchFilter + '%'
                                                                                AND CONVERT(date, created_at) BETWEEN @DateFrom AND @DateTo) AS totalRecord,
                                                                (CEILING(CAST((SELECT COUNT(*) FROM audit_log
                                                                              WHERE user_id = @user_id AND action_type LIKE '%' +  @SearchFilter + '%'
                                                                              AND CONVERT(date, created_at) BETWEEN @DateFrom AND @DateTo) AS decimal) / @PageSize)) AS totalPages;
                                                            ",
                                        new
                                        {
                                            DateFrom = dateFrom,
                                            DateTo = dateTo,
                                            PageIndex = pageIndex,
                                            PageSize = pageSize,
                                            SearchFilter = searchFilter,
                                            user_id = id_user
                                        }).FirstOrDefault();

                return Ok(new
                {
                    status = 1,
                    Message = "thanh cong",
                    data = new
                    {
                        data = full,
                       ffull.TotalPages,
                       ffull.TotalRecord
                    }
                }); ;
    

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

        [HttpPost]
        [Route("save_logs")]
        public IActionResult SaveLogs(SaveLogModel model,string token)
        {


            try
            {
                var dataAccess = new DataAccess(_Configuration);
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

                        Message = "người dùng đã đang xuat."
                    });
                }
                dataAccess.SaveLogs(model, id_user);




                return Ok(new
                {
                   
                    Status = 1,
                    Message = " thành công"
                });
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
