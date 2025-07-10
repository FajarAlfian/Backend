using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface ICategoriesRepository
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category?> GetCategoryByNameAsync(string nama);
        Task<int> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public class CategoryRepository : ICategoriesRepository
    {
        private readonly string _connectionString;

        public CategoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        // Read
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = new List<Category>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT category_id, category_name, category_description, category_image, category_banner, is_active, created_at, updated_at
                    FROM ms_category
                    ORDER BY category_id";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(new Category
                        {
                            category_id = reader.GetInt32("category_id"),
                            category_name = reader.GetString("category_name"),
                            category_description = reader.GetString("category_description"),
                            category_image = reader.GetString("category_image"),
                            category_banner = reader.GetString("category_banner"),
                            is_active = reader.GetBoolean("is_active"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(), 
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                        });
                    }
                }
            }
            return categories;
        }
        
        public async Task<List<Category>> GetActiveCategoriesAsync()
              {
            var categories = new List<Category>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT category_id, category_name, category_description, category_image, category_banner, is_active, created_at, updated_at
                    FROM ms_category
                    WHERE is_active = 1
                    ORDER BY category_name";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(new Category
                        {
                            category_id = reader.GetInt32("category_id"),
                            category_name = reader.GetString("category_name"),
                            category_description = reader.GetString("category_description"),
                            category_image = reader.GetString("category_image"),
                            category_banner = reader.GetString("category_banner"),
                            is_active = reader.GetBoolean("is_active"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(), 
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                        });
                    }
                }
            }
            return categories;
        }
        
        

        // Read by ID
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT category_id, category_name, category_description, category_image, category_banner, is_active, created_at, updated_at
                    FROM ms_category
                    WHERE category_id = @category_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@category_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Category
                            {
                                category_id = reader.GetInt32("category_id"),
                                category_name = reader.GetString("category_name"),
                                category_description = reader.GetString("category_description"),
                                category_image = reader.GetString("category_image"),
                                category_banner = reader.GetString("category_banner"),
                                is_active = reader.GetBoolean("is_active"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                            };
                        }
                    }
                }
            }
            return null;
        }
        
        // Read by name
        public async Task<Category?> GetCategoryByNameAsync(string nama)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT category_id, category_name, category_description, category_image, category_banner, created_at, updated_at
                    FROM ms_category
                    WHERE category_name = @category_name";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@category_name", nama);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Category
                            {
                                category_id = reader.GetInt32("category_id"),
                                category_name = reader.GetString("category_name"),
                                category_description = reader.GetString("category_description"),
                                category_image = reader.GetString("category_image"),
                                category_banner = reader.GetString("category_banner"),
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
        public async Task<int> CreateCategoryAsync(Category category)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    INSERT INTO ms_category (category_name, category_description, category_image, category_banner, created_at, updated_at)
                    VALUES (@category_name, @category_description, @category_image, @category_banner, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@category_name", category.category_name);
                    command.Parameters.AddWithValue("@category_description", category.category_description);
                    command.Parameters.AddWithValue("@category_image", category.category_image);
                    command.Parameters.AddWithValue("@category_banner", category.category_banner);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        // Update
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    UPDATE ms_category
                    SET category_name = @category_name, category_description = @category_description, category_image = @category_image, category_banner = @category_banner, is_active = @is_active, updated_at = @updated_at
                    WHERE category_id = @category_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@category_id", category.category_id);
                    command.Parameters.AddWithValue("@category_name", category.category_name);
                    command.Parameters.AddWithValue("@category_description", category.category_description); 
                    command.Parameters.AddWithValue("@category_image", category.category_image);
                    command.Parameters.AddWithValue("@category_banner", category.category_banner);
                    command.Parameters.AddWithValue("@is_active", category.is_active);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow); 

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        // Delete
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    DELETE FROM ms_category
                    WHERE category_id = @category_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@category_id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}