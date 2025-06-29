using System.Data;
using DlanguageApi.Models;
using MySql.Data.MySqlClient;

namespace DlanguageApi.Data
{
    public interface ICheckoutRepository
    {
        Task AddToCheckoutAsync(Checkout checkout);
        Task<List<Checkout>> GetUserCheckoutAsync(int userId);
        Task<decimal> GetTotalPriceAsync(int userId);
        Task<bool> IsCourseInCheckoutAsync(int userId, int courseId);
        Task<bool> RemoveFromCheckoutAsync(int userId, int courseId);
        Task ClearUserCartAsync(int userId);
    }

    public class CheckoutRepository : ICheckoutRepository
    {
        private readonly string _connectionString;

        public CheckoutRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        public async Task AddToCheckoutAsync(Checkout checkout)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    INSERT INTO tr_cart_product (user_id, course_id, schedule_course_id, course_price, created_at, updated_at)
                    VALUES (@user_id, @course_id, @schedule_course_id, @course_price, @created_at, @updated_at)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", checkout.user_id);
                    command.Parameters.AddWithValue("@course_id", checkout.course_id);
                    command.Parameters.AddWithValue("@schedule_course_id", checkout.schedule_course_id);
                    command.Parameters.AddWithValue("@course_price", checkout.course_price);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Checkout>> GetUserCheckoutAsync(int userId)
        {
            var checkouts = new List<Checkout>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT cp.cart_product_id, cp.course_id, c.course_name, cp.course_price, cp.user_id, cp.created_at, cp.updated_at
                    FROM tr_cart_product cp
                    JOIN ms_courses c ON cp.course_id = c.course_id
                    WHERE cp.user_id = @user_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checkouts.Add(new Checkout
                            {
                                cart_product_id = reader.GetInt32("cart_product_id"),
                                course_id = reader.GetInt32("course_id"),
                                course_name = reader.GetString("course_name"),
                                course_price = reader.GetInt32("course_price"),
                                user_id = reader.GetInt32("user_id"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                            });
                        }
                    }
                }
            }
            return checkouts;
        }

        public async Task<decimal> GetTotalPriceAsync(int userId)
        {
            decimal total = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT SUM(course_price) FROM tr_cart_product WHERE user_id = @user_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    var result = await command.ExecuteScalarAsync();
                    if (result != DBNull.Value)
                        total = Convert.ToDecimal(result);
                }
            }
            return total;
        }

        public async Task<bool> IsCourseInCheckoutAsync(int userId, int courseId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COUNT(*) FROM tr_cart_product WHERE user_id = @user_id AND course_id = @course_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@course_id", courseId);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<bool> RemoveFromCheckoutAsync(int userId, int courseId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM tr_cart_product WHERE user_id = @user_id AND course_id = @course_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@course_id", courseId);
                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task ClearUserCartAsync(int userId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM tr_cart_product WHERE user_id = @user_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}