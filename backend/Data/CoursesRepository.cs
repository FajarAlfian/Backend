using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface ICoursesRepository
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course?> GetCourseByCategoryIdAsync(int id);
        Task<int> CreateCourseAsync(Course course);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
    }

    public class CourseRepository : ICoursesRepository
    {
        private readonly string _connectionString;

        public CourseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        // Read
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            var courses = new List<Course>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT c.course_id, c.course_name, c.course_price, c.course_image, c.course_description, c.category_id, cat.category_name, c.created_at, c.updated_at
                    FROM ms_courses c
                    LEFT JOIN ms_category cat ON c.category_id = cat.category_id
                    ORDER BY c.course_id";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        courses.Add(new Course
                        {
                            course_id = reader.GetInt32("course_id"),
                            course_name = reader.GetString("course_name"),
                            course_price = reader.GetInt32("course_price"),
                            course_image = reader.GetString("course_image"),
                            course_description = reader.GetString("course_description"),
                            category_id = reader.GetInt32("category_id"),
                            category_name = reader.GetString("category_name"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(), 
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime() 
                        });
                    }
                }
            }
            return courses;
        }
        
        public async Task<Course?> GetCourseByCategoryIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT c.course_id, c.course_name, c.course_price, c.course_image, c.course_description, c.category_id, cat.category_name, c.created_at, c.updated_at
                    FROM ms_courses c
                    LEFT JOIN ms_category cat ON c.category_id = cat.category_id
                    WHERE cat.category_id = @category_id
                    ORDER BY c.course_id";

                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@category_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Course
                            {
                                course_id = reader.GetInt32("course_id"),
                                course_name = reader.GetString("course_name"),
                                course_price = reader.GetInt32("course_price"),
                                course_image = reader.GetString("course_image"),
                                course_description = reader.GetString("course_description"),
                                category_id = reader.GetInt32("category_id"),
                                category_name = reader.IsDBNull(reader.GetOrdinal("category_name")) ? string.Empty : reader.GetString("category_name"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Read by ID
        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT c.course_id, c.course_name, c.course_price, c.course_image, c.course_description, c.category_id, cat.category_name, c.created_at, c.updated_at
                    FROM ms_courses c
                    LEFT JOIN ms_category cat ON c.category_id = cat.category_id
                    WHERE c.course_id = @course_id
                    ORDER BY c.course_id";

                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Course
                            {
                                course_id = reader.GetInt32("course_id"),
                                course_name = reader.GetString("course_name"),
                                course_price = reader.GetInt32("course_price"),
                                course_image = reader.GetString("course_image"),
                                course_description = reader.GetString("course_description"),
                                category_id = reader.GetInt32("category_id"),
                                category_name = reader.IsDBNull(reader.GetOrdinal("category_name")) ? string.Empty : reader.GetString("category_name"),
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
        public async Task<int> CreateCourseAsync(Course course)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    INSERT INTO ms_courses (course_name, course_price, course_image, course_description, category_id, created_at, updated_at)
                    VALUES (@course_name, @course_price, @course_image, @course_description, @category_id, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_name", course.course_name);
                    command.Parameters.AddWithValue("@course_price", course.course_price);
                    command.Parameters.AddWithValue("@course_image", course.course_image);
                    command.Parameters.AddWithValue("@course_description", course.course_description);
                    command.Parameters.AddWithValue("@category_id", course.category_id);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow); 

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        // Update
        public async Task<bool> UpdateCourseAsync(Course course)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    UPDATE ms_courses
                    SET course_name = @course_name, course_price = @course_price, course_image = @course_image, course_description = @course_description, category_id = @category_id,
                        updated_at = @updated_at
                    WHERE course_id = @course_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_id", course.course_id);
                    command.Parameters.AddWithValue("@course_name", course.course_name);
                    command.Parameters.AddWithValue("@course_price", course.course_price); 
                    command.Parameters.AddWithValue("@course_image", course.course_image);  
                    command.Parameters.AddWithValue("@course_description", course.course_description);
                    command.Parameters.AddWithValue("@category_id", course.category_id); 
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow); 

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        // Delete
        public async Task<bool> DeleteCourseAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    DELETE FROM ms_courses
                    WHERE course_id = @course_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}