using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<int> CreateInvoiceAsync(Invoice invoice);
        Task<List<Invoice>> GetInvoiceByUserIdAsync(int userId);
        Task<bool> UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(int id);
        Task<double> GetTotalPriceAsync(int userId);
        Task CreateInvoiceDetailAsync(int invoiceId, int cartProductId, int courseId, double subTotalPrice);
        Task<int> GetLastInvoiceNumberAsync();
        Task<string> GetPaymentMethodNameByIdAsync(int payment_method_id);
    }

    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        public async Task<List<Invoice>> GetInvoiceByUserIdAsync(int userId)
        {
            var invoices = new List<Invoice>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
            SELECT
                inv.invoice_id,
                inv.invoice_number,
                inv.user_id,
                inv.total_price,
                inv.payment_method_id,
                pm.payment_method_name,
                inv.isPaid,
                COUNT(ind.invoice_detail_id) AS total_courses,
                inv.created_at,
                inv.updated_at
            FROM tr_invoice inv
            LEFT JOIN ms_payment_method pm ON inv.payment_method_id = pm.payment_method_id
            LEFT JOIN tr_invoice_detail ind ON inv.invoice_id = ind.invoice_id
            WHERE inv.user_id = @user_id
            GROUP BY inv.invoice_id
            ORDER BY inv.created_at DESC";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            invoices.Add(new Invoice
                            {
                                invoice_id = reader.GetInt32("invoice_id"),
                                invoice_number = reader.GetString("invoice_number"),
                                user_id = reader.GetInt32("user_id"),
                                total_price = reader.GetDouble("total_price"),
                                payment_method_id = reader.GetInt32("payment_method_id"),
                                payment_method_name = reader.GetString("payment_method_name"),
                                isPaid = reader.GetBoolean("isPaid"),
                                total_courses = reader.GetInt32("total_courses"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                            });
                        }
                    }
                }
            }
            return invoices;
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT
                inv.invoice_id,
                inv.user_id,
                inv.payment_method_id,
                pm.payment_method_name,
                inv.isPaid,
                inv.invoice_number       AS invoice_number,
                inv.created_at           AS invoice_date,
                inv.total_price          AS total_price,
                COUNT(ind.invoice_detail_id) AS total_courses,
                ROW_NUMBER() OVER (
                    PARTITION BY inv.invoice_id
                    ORDER BY ind.invoice_detail_id
                )                         AS detail_no,
                c.course_name            AS course_name,
                cat.category_name        AS LANGUAGE,
                sch.schedule_date        AS SCHEDULE,
                ind.sub_total_price      AS price
                FROM tr_invoice           inv
                JOIN tr_invoice_detail    ind ON ind.invoice_id   = inv.invoice_id
                JOIN ms_courses           c   ON c.course_id      = ind.course_id
                JOIN ms_category          cat ON cat.category_id  = c.category_id
                LEFT JOIN tr_schedule_course sc  ON sc.course_id     = c.course_id
                LEFT JOIN ms_schedule        sch ON sch.schedule_id  = sc.schedule_id
                LEFT JOIN ms_payment_method pm ON pm.payment_method_id = inv.payment_method_id
                WHERE inv.invoice_id = @invoiceId
                ORDER BY ind.invoice_detail_id;";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@invoiceId", id);

            using var reader = await cmd.ExecuteReaderAsync();
            Invoice? invoice = null;

            while (await reader.ReadAsync())
            {
                if (invoice is null)
                {
                    invoice = new Invoice
                    {
                        invoice_id = reader.GetInt32("invoice_id"),
                        user_id = reader.GetInt32("user_id"),
                        invoice_number = reader.GetString("invoice_number"),
                        invoice_date = reader.GetDateTime("invoice_date"),
                        total_price = reader.GetDouble("total_price"),
                        payment_method_id = reader.GetInt32("payment_method_id"),
                        payment_method_name = reader.GetString("payment_method_name"),
                        total_courses = reader.GetInt32("total_courses"),
                        isPaid = reader.GetBoolean("isPaid")
                    };
                }

                invoice.detail.Add(new InvoiceDetail
                {
                    detail_no   = reader.GetInt32("detail_no"),
                    course_name = reader.GetString("course_name"),
                    language   = reader.GetString("LANGUAGE"),
                    schedule   = reader.GetString("SCHEDULE"),
                    price      = reader.GetDouble("price")
                });
            }

            return invoice;
        }



            public async Task<int> CreateInvoiceAsync(Invoice invoice)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string queryString = @"
                        INSERT INTO tr_invoice
                            (invoice_number, user_id, total_price, payment_method_id, isPaid, created_at, updated_at)
                        VALUES
                            (@invoice_number,
                            @user_id,
                            (SELECT COALESCE(SUM(course_price),0)
                                FROM tr_cart_product
                            WHERE user_id = @user_id),
                            @payment_method_id,
                            @isPaid,
                            @created_at,
                            @updated_at);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@invoice_number", invoice.invoice_number);
                        command.Parameters.AddWithValue("@user_id",         invoice.user_id);
                        command.Parameters.AddWithValue("@payment_method_id", invoice.payment_method_id);
                        command.Parameters.AddWithValue("@isPaid",          invoice.isPaid);
                        command.Parameters.AddWithValue("@created_at",      DateTime.UtcNow);
                        command.Parameters.AddWithValue("@updated_at",      DateTime.UtcNow);

                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }

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
                        isPaid = @isPaid,
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
                    command.Parameters.AddWithValue("@isPaid", invoice.isPaid);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

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

        public async Task<double> GetTotalPriceAsync(int userId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COALESCE(SUM(course_price),0) FROM tr_cart_product WHERE user_id = @user_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToDouble(result);
                }
            }
        }

        public async Task CreateInvoiceDetailAsync(int invoiceId, int cartProductId, int courseId, double subTotalPrice)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    INSERT INTO tr_invoice_detail (invoice_id, cart_product_id, course_id, sub_total_price, created_at, updated_at)
                    VALUES (@invoice_id, @cart_product_id, @course_id, @sub_total_price, @created_at, @updated_at)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoice_id", invoiceId);
                    command.Parameters.AddWithValue("@cart_product_id", cartProductId);
                    command.Parameters.AddWithValue("@course_id", courseId);
                    command.Parameters.AddWithValue("@sub_total_price", subTotalPrice);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> GetLastInvoiceNumberAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT invoice_number FROM tr_invoice ORDER BY invoice_id DESC LIMIT 1";
                using (var command = new MySqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        string lastInvoiceNumber = result.ToString();

                        if (lastInvoiceNumber.Length > 3 && int.TryParse(lastInvoiceNumber.Substring(3), out int lastNumber))
                        {
                            return lastNumber;
                        }
                    }
                    return 0;
                }
            }
        }

        public async Task<string> GetPaymentMethodNameByIdAsync(int payment_method_id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT payment_method_name FROM ms_payment_method WHERE payment_method_id = @id LIMIT 1";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", payment_method_id);
                    var result = await command.ExecuteScalarAsync();
                    return result?.ToString() ?? "";
                }
            }
        }
    }
}

