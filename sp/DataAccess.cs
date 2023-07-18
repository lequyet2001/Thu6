using System.Data;
using System.Data.SqlClient;
using Dapper;
using Thu6.model;

public class DataAccess
{
    private readonly string _connectionString;

    public DataAccess(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Test");
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public T ExecuteScalar<T>(string query, object parameters = null)
    {
        using (var connection = CreateConnection())
        {
            return connection.ExecuteScalar<T>(query, parameters);
        }
    }

    public int Execute(string query, object parameters = null)
    {
        using (var connection = CreateConnection())
        {
            return connection.Execute(query, parameters);
        }
    }

    public IEnumerable<T> Query<T>(string query, object parameters = null)
    {
        using (var connection = CreateConnection())
        {
            return connection.Query<T>(query, parameters);
        }
    }
  

    // Các phương thức thực hiện các tác vụ thêm, sửa, xóa dữ liệu khác...

    // Ví dụ phương thức lấy danh sách người dùng
    public IEnumerable<Users> GetUsers()
    {
        var query = "SELECT * FROM user";
        return Query<Users>(query);
    }

    // Ví dụ phương thức thêm người dùng
    public int AddUser(Users user)
    {
        var query = "INSERT INTO [user] ( user_id,password, email) VALUES (@User_id, @Password, @Email)";
        return Execute(query, user);
    }
    public int UpdateUser(Users user)
    {
    
            var query = @"
            UPDATE [user]
            SET status = @Status
            WHERE user_id = @UserId";

            var parameters = new
            {
                Status=user.Status,
                UserId = user.User_id
            };

            return Execute(query, parameters);
        
    }
    public int UpdateUserWithPassword(Users user)
    {

        var query = @"
            UPDATE [user]
            SET password = @Password
            WHERE user_id = @UserId";

        var parameters = new
        {
            Password = user.Password,
            UserId = user.User_id
        };

        return Execute(query, parameters);

    }
    public int UpdateUserInfor(Users user)
    {

        var query = @"
            UPDATE [user]
            SET display_name=@Display_name,
                full_name=@Full_name,
                birthday=@Birthday,
                position=@Position,
                avatar=@Avatar
            WHERE user_id = @User_id";

        var parameters = new
        {
            user.User_id,
            user.Display_name,
            user.Full_name,
            user.Birthday,
            user.Position,
            user.Avatar
        };

        return Execute(query, parameters);

    }

}
