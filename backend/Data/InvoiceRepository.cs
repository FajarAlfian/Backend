using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface IInvoiceRepository
    {
        Task<List<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<int> CreateInvoiceAsync(Invoice invoice);
        Task<bool> UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(int id);
    }

    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        // Read
        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            var invoices = new List<Invoice>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT 
                        inv.invoice_id,
                        inv.user_id,
                        inv.total_price,
                        inv.payment_method_id,
                        pm.payment_method_name,
                        inv.is_paid,
                        inv.created_at,
                        inv.updated_at,
                        COUNT(ind.invoice_detail_id) AS total_courses
                    FROM tr_invoice inv
                    LEFT JOIN ms_payment_method pm ON inv.payment_method_id = pm.payment_method_id
                    LEFT JOIN tr_invoice_detail ind ON inv.invoice_id = ind.invoice_id
                    GROUP BY inv.invoice_id
                    ORDER BY inv.created_at DESC;
                ";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        invoices.Add(new Invoice
                        {
                            invoice_id = reader.GetInt32("invoice_id"),
                            user_id = reader.GetInt32("user_id"),
                            total_price = reader.GetDouble("total_price"),
                            payment_method_id = reader.GetInt32("payment_method_id"),
                            payment_method_name = reader.IsDBNull(reader.GetOrdinal("payment_method_name")) ? null : reader.GetString("payment_method_name"),
                            is_paid = reader.GetBoolean("is_paid"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                        });
                    }
                }
            }
            return invoices;
        }

        // Read by ID
        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT 
                        inv.invoice_id,
                        inv.user_id,
                        inv.total_price,
                        inv.payment_method_id,
                        pm.payment_method_name,
                        inv.is_paid,
                        inv.created_at,
                        inv.updated_at,
                        COUNT(ind.invoice_detail_id) AS total_courses
                    FROM tr_invoice inv
                    LEFT JOIN ms_payment_method pm ON inv.payment_method_id = pm.payment_method_id
                    LEFT JOIN tr_invoice_detail ind ON inv.invoice_id = ind.invoice_id
                    WHERE inv.invoice_id = @invoice_id
                    GROUP BY inv.invoice_id
                    ORDER BY inv.created_at DESC;
                ";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@invoice_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Invoice
                            {
                                invoice_id = reader.GetInt32("invoice_id"),
                                user_id = reader.GetInt32("user_id"),
                                total_price = reader.GetDouble("total_price"),
                                payment_method_id = reader.GetInt32("payment_method_id"),
                                payment_method_name = reader.IsDBNull(reader.GetOrdinal("payment_method_name")) ? null : reader.GetString("payment_method_name"),
                                is_paid = reader.GetBoolean("is_paid"),
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
        public async Task<int> CreateInvoiceAsync(Invoice invoice)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    INSERT INTO tr_invoice (user_id, total_price, payment_method_id, payment_method_name, is_paid, created_at, updated_at)
                    VALUES (@user_id, @total_price, @payment_method_id, @payment_method_name, @is_paid, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();
                ";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@user_id", invoice.user_id);
                    command.Parameters.AddWithValue("@total_price", invoice.total_price);
                    command.Parameters.AddWithValue("@payment_method_id", invoice.payment_method_id);
                    command.Parameters.AddWithValue("@payment_method_name", invoice.payment_method_name);
                    command.Parameters.AddWithValue("@is_paid", invoice.is_paid);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        // Update
        public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    UPDATE tr_invoice
                    SET user_id = @user_id,
                        total_price = @total_price,
                        payment_method_id = @payment_method_id,
                        payment_method_name = @payment_method_name,
                        is_paid = @is_paid,
                        updated_at = @updated_at
                    WHERE invoice_id = @invoice_id;
                ";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@invoice_id", invoice.invoice_id);
                    command.Parameters.AddWithValue("@user_id", invoice.user_id);
                    command.Parameters.AddWithValue("@total_price", invoice.total_price);
                    command.Parameters.AddWithValue("@payment_method_id", invoice.payment_method_id);
                    command.Parameters.AddWithValue("@payment_method_name", invoice.payment_method_name);
                    command.Parameters.AddWithValue("@is_paid", invoice.is_paid);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        // Delete
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    DELETE FROM tr_invoice
                    WHERE invoice_id = @invoice_id;
                ";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@invoice_id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}

