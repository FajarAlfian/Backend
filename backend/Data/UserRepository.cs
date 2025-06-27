using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
        public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<int> CreateUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        //Read
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT user_id, username, email, password, created_at, updated_at
                    FROM ms_user
                    ORDER BY username";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            user_id = reader.GetInt32("user_id"),
                            username = reader.GetString("username"),
                            email = reader.GetString("email"),
                            password = reader.GetString("password"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(), // Use Utc
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime() // Use Utc
                        });
                    }
                }
            }
            return users;
        }
        //Read by ID
        public async Task<User?> GetUserByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT user_id, username, email, password, created_at, updated_at
                    FROM ms_user
                    WHERE user_id = @user_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@user_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                user_id = reader.GetInt32("user_id"),
                                username = reader.GetString("username"),
                                email = reader.GetString("email"),
                                password = reader.GetString("password"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(), // Use Utc
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime() // Use Utc
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Read by Email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT user_id, username, email, password, created_at, updated_at
                    FROM ms_user
                    WHERE email = @email";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                user_id = reader.GetInt32("user_id"),
                                username = reader.GetString("username"),
                                email = reader.GetString("email"),
                                password = reader.GetString("password"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(), // Use Utc
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime() // Use Utc
                            };
                        }
                    }
                }
            }
            return null;
        }
        // Create
        public async Task<int> CreateUserAsync(User user)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    INSERT INTO ms_user (username, email, password, created_at, updated_at)
                    VALUES (@username, @email, @password, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@username", user.username);
                    command.Parameters.AddWithValue("@email", user.email);
                    command.Parameters.AddWithValue("@password", user.password);
                    command.Parameters.AddWithValue("@created_at", user.created_at.ToUniversalTime()); // Use Utc
                    command.Parameters.AddWithValue("@updated_at", user.updated_at.ToUniversalTime()); // Use Utc
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        // Update
        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    UPDATE ms_user
                    SET username = @username,
                        email = @email,
                        password = @password,
                        updated_at = @updated_at
                    WHERE user_id = @user_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@user_id", user.user_id);
                    command.Parameters.AddWithValue("@username", user.username);
                    command.Parameters.AddWithValue("@email", user.email);
                    command.Parameters.AddWithValue("@password", user.password);
                    command.Parameters.AddWithValue("@updated_at", user.updated_at.ToUniversalTime()); // Use Utc
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        // Delete
        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = "DELETE FROM ms_user WHERE user_id = @user_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@user_id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}