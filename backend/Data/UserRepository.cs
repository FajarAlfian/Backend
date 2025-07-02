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
        Task<bool> UpdatePasswordResetTokenAsync(int userId, string token);
        Task<bool> UpdatePasswordAndClearResetTokenAsync(int userId, string hashedPassword);
        Task<bool> DeleteUserAsync(int id);
        Task<User?> GetUserByResetTokenAsync(string token);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> UpdateEmailVerificationTokenAsync(int userId, string token);
        Task<bool> VerifyUserEmailAsync(int userId);
    
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
                    SELECT user_id, username, email, password, role, created_at, updated_at
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
                            role = reader.GetString("role"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
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
                    SELECT user_id, username, email, password, role, created_at, updated_at
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
                                role = reader.GetString("role"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
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
                    SELECT user_id, username, email, password, role, created_at, updated_at
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
                                role = reader.GetString("role"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
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
                    INSERT INTO ms_user 
                        (username, email, password, role, 
                        is_verified, email_verification_token, email_token_created_at, 
                        created_at, updated_at)
                    VALUES 
                        (@username, @email, @password, @role, 
                        @is_verified, @email_verification_token, @email_token_created_at, 
                        @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@username", user.username);
                    command.Parameters.AddWithValue("@email", user.email);
                    command.Parameters.AddWithValue("@password", user.password);
                    command.Parameters.AddWithValue("@role", user.role);
                    command.Parameters.AddWithValue("@is_verified", user.is_verified);
                    command.Parameters.AddWithValue("@email_verification_token", user.email_verification_token);
                    command.Parameters.AddWithValue("@email_token_created_at", user.email_token_created_at?.ToUniversalTime()??(object)DBNull.Value);
                    command.Parameters.AddWithValue("@created_at", user.created_at.ToUniversalTime());
                    command.Parameters.AddWithValue("@updated_at", user.updated_at.ToUniversalTime());

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
                        role = @role,
                        updated_at = @updated_at
                    WHERE user_id = @user_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@user_id", user.user_id);
                    command.Parameters.AddWithValue("@username", user.username);
                    command.Parameters.AddWithValue("@email", user.email);
                    command.Parameters.AddWithValue("@password", user.password);
                    command.Parameters.AddWithValue("@role", user.role);
                    command.Parameters.AddWithValue("@updated_at", user.updated_at.ToUniversalTime());
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

        // Update Password Reset Token
        public async Task<bool> UpdatePasswordAndClearResetTokenAsync(int userId, string hashedPassword)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"UPDATE ms_user 
                                SET password = @Password, 
                                    PasswordResetToken = NULL, 
                                    PasswordResetTokenCreatedAt = NULL 
                                WHERE user_id = @UserId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@UserId", userId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> UpdatePasswordResetTokenAsync(int userId, string token)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"UPDATE ms_user 
                                 SET PasswordResetToken = @Token, 
                                     PasswordResetTokenCreatedAt = @CreatedAt 
                                 WHERE user_id = @UserId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@UserId", userId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<User?> GetUserByResetTokenAsync(string token)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"SELECT user_id, username, email, password, role, created_at, updated_at, PasswordResetTokenCreatedAt
                                 FROM ms_user
                                 WHERE PasswordResetToken = @Token";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
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
                                role = reader.GetString("role"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime(),
                            };
                        }
                    }
                }
            }
            return null;
        }
        public async Task<bool> VerifyUserEmailAsync(int userId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"UPDATE ms_user 
                               SET is_verified = TRUE, 
                                   email_verification_token = NULL, 
                                   email_token_created_at = NULL 
                               WHERE user_id = @UserID";


                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Memperbarui token verifikasi email
        /// </summary>
        public async Task<bool> UpdateEmailVerificationTokenAsync(int userId, string token)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"UPDATE ms_user 
                               SET email_verification_token = @Token, 
                                   email_token_created_at = @CreatedAt 
                               WHERE user_id = @UserID";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    command.Parameters.AddWithValue("@UserID", userId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        /// <summary>
        /// Verify email using verification token
        /// </summary>
        public async Task<bool> VerifyEmailAsync(string token)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Check token validity and update user
                string query = @"UPDATE ms_user 
                               SET is_verified = 1, 
                                   email_verification_token = NULL, 
                                   email_token_created_at = NULL 
                               WHERE email_verification_token = @Token 
                               AND email_token_created_at > DATE_SUB(NOW(), INTERVAL 24 HOUR)
                               AND is_verified = 0";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
                    
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
}
}