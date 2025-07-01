using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface IPaymentMethodRepository
    {
        Task<List<PaymentMethod>> GetAllPaymentMethodAsync();
        Task<PaymentMethod?> GetPaymentMethodByIdAsync(int id);
        Task<int> CreatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task<bool> UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task<bool> DeletePaymentMethodAsync(int id);
    }

    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly string _connectionString;

        public PaymentMethodRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        // Read
        public async Task<List<PaymentMethod>> GetAllPaymentMethodAsync()
        {
            var paymentMethod = new List<PaymentMethod>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT payment_method_id, payment_method_name, payment_method_logo, created_at, updated_at
                    FROM ms_payment_method
                    ORDER BY payment_method_name";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        paymentMethod.Add(new PaymentMethod
                        {
                            payment_method_id = reader.GetInt32("payment_method_id"),
                            payment_method_name = reader.GetString("payment_method_name"),
                            payment_method_logo = reader.GetString("payment_method_logo"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(), 
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                        });
                    }
                }
            }
            return paymentMethod;
        }

        // Read by ID
        public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT payment_method_id, payment_method_name, payment_method_logo, created_at, updated_at
                    FROM ms_payment_method
                    WHERE payment_method_id = @payment_method_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@payment_method_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new PaymentMethod
                            {
                                payment_method_id = reader.GetInt32("payment_method_id"),
                                payment_method_name = reader.GetString("payment_method_name"),
                                payment_method_logo = reader.GetString("payment_method_logo"),
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
        public async Task<int> CreatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    INSERT INTO ms_payment_method (payment_method_name, payment_method_logo, created_at, updated_at)
                    VALUES (@payment_method_name, @payment_method_logo, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@payment_method_name", paymentMethod.payment_method_name);
                    command.Parameters.AddWithValue("@payment_method_logo", paymentMethod.payment_method_logo);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        // Update
        public async Task<bool> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    UPDATE ms_payment_method
                    SET payment_method_name = @payment_method_name, payment_method_logo = @payment_method_logo, updated_at = @updated_at
                    WHERE payment_method_id = @payment_method_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@payment_method_id", paymentMethod.payment_method_id);
                    command.Parameters.AddWithValue("@payment_method_name", paymentMethod.payment_method_name);
                    command.Parameters.AddWithValue("@payment_method_logo", paymentMethod.payment_method_logo);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        // Delete
        public async Task<bool> DeletePaymentMethodAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    DELETE FROM ms_payment_method
                    WHERE payment_method_id = @payment_method_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@payment_method_id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}