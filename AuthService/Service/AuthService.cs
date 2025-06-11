using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;


public class AuthService
{

    private readonly string _connectionString;

    public AuthService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("MySQL");
    }

    public async Task<bool> RegisterUser(RegisterRequest request)
    {
        using (var conn = new MySqlConnection(_connectionString)) {
            await conn.OpenAsync();

            // 检查用户名是否已存在
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", conn);
            checkCmd.Parameters.AddWithValue("@username", request.Username);

            if ((long)await checkCmd.ExecuteScalarAsync() > 0)
                return false;

            // 插入新用户 (实际应用中应对密码进行哈希处理)
            var insertCmd = new MySqlCommand(
                "INSERT INTO users (username, password, email, created_at) " +
                "VALUES (@username, @password, @email, UTC_TIMESTAMP())", conn);

            insertCmd.Parameters.AddWithValue("@username", request.Username);
            insertCmd.Parameters.AddWithValue("@password", request.Password); // 存储明文密码仅用于演示
            insertCmd.Parameters.AddWithValue("@email", request.Email);

            return await insertCmd.ExecuteNonQueryAsync() > 0;
        }
    }

    /// <summary>
    /// 登录验证//
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<User> AuthenticateUser(LoginRequest request)
    {
        using (var conn = new MySqlConnection(_connectionString)) {
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT id, username, email, created_at FROM users " +
                "WHERE username = @username AND password = @password", conn);

            cmd.Parameters.AddWithValue("@username", request.Username);
            cmd.Parameters.AddWithValue("@password", request.Password);

            using (var reader = await cmd.ExecuteReaderAsync()) {
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        // 安全读取数值类型（兼容INT/BIGINT等）
                        Id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 :
                    Convert.ToInt32(reader["id"]), // 或 reader.GetInt64() 如果使用BIGINT

                        // 字符串类型处理
                        Username = reader.IsDBNull(reader.GetOrdinal("username")) ?
                    string.Empty : reader.GetString(reader.GetOrdinal("username")),

                        Email = reader.IsDBNull(reader.GetOrdinal("email")) ?
                    string.Empty : reader.GetString(reader.GetOrdinal("email")),

                        // 日期类型处理
                        CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ?
                    DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("created_at"))
                    };
                }
            }
            return null;
        }
            
    }
}

