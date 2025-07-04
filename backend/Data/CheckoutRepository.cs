using System.Data;
using DlanguageApi.Models;
using MySql.Data.MySqlClient;

namespace DlanguageApi.Data
{
    public interface ICheckoutRepository
    {
        Task AddToCheckoutAsync(Checkout checkout);
        Task<List<GetCheckout>> GetUserCheckoutAsync(int userId);
        Task<decimal> GetTotalPriceAsync(int userId);
        Task<bool> IsScheduleCourseInCheckoutAsync(int userId, int scheduleCourseId); 
        Task<bool> RemoveFromCheckoutAsync(int userId, int scheduleCourseId); 
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
                    VALUES (@user_id, @course_id, @schedule_course_id, @course_price, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", checkout.user_id);
                    command.Parameters.AddWithValue("@course_id", checkout.course_id);
                    command.Parameters.AddWithValue("@schedule_course_id", checkout.schedule_course_id);
                    command.Parameters.AddWithValue("@course_price", checkout.course_price);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

                    var result = await command.ExecuteScalarAsync();
                    checkout.cart_product_id = Convert.ToInt32(result); 
                }
            }
        }

        public async Task<List<GetCheckout>> GetUserCheckoutAsync(int userId)
        {
            var checkouts = new List<GetCheckout>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT cp.cart_product_id, cp.course_id, cp.schedule_course_id,
                    c.course_image, c.course_name, 
                    cat.category_name, sch.schedule_date, c.course_price,
                    cp.user_id, cp.created_at, cp.updated_at
                    FROM tr_cart_product cp
                    JOIN ms_courses c ON cp.course_id = c.course_id
                    JOIN ms_category cat ON c.category_id = cat.category_id
                    JOIN tr_schedule_course tsc ON  cp.schedule_course_id = tsc.schedule_course_id
                    JOIN ms_schedule sch ON tsc.schedule_id = sch.schedule_id
                    WHERE cp.user_id = @user_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checkouts.Add(new GetCheckout
                            {
                                cart_product_id = reader.GetInt32("cart_product_id"),
                                course_id = reader.GetInt32("course_id"),
                                schedule_course_id = reader.GetInt32("schedule_course_id"),
                                course_image = reader.GetString("course_image"),
                                course_name = reader.GetString("course_name"),
                                category_name = reader.GetString("category_name"),
                                course_price = reader.GetInt32("course_price"),
                                user_id = reader.GetInt32("user_id"),
                                schedule_date = reader.GetString("schedule_date"),
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

        public async Task<bool> IsScheduleCourseInCheckoutAsync(int userId, int scheduleCourseId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COUNT(*) FROM tr_cart_product WHERE user_id = @user_id AND schedule_course_id = @schedule_course_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@schedule_course_id", scheduleCourseId);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<bool> RemoveFromCheckoutAsync(int userId, int cartProductId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();


            const string sqlNullChild = @"
        UPDATE tr_invoice_detail
        SET cart_product_id = NULL
        WHERE cart_product_id = @cartId;
        ";
            using (var cmd0 = new MySqlCommand(sqlNullChild, connection))
            {
                cmd0.Parameters.AddWithValue("@cartId", cartProductId);
                await cmd0.ExecuteNonQueryAsync();
            }


            const string sqlDeleteCart = @"
        DELETE FROM tr_cart_product
        WHERE user_id = @userId
        AND cart_product_id = @cartId;
        ";
            int affected;
            using (var cmd1 = new MySqlCommand(sqlDeleteCart, connection))
            {
                cmd1.Parameters.AddWithValue("@userId",  userId);
                cmd1.Parameters.AddWithValue("@cartId",  cartProductId);
                affected = await cmd1.ExecuteNonQueryAsync();
            }

            return affected > 0;
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