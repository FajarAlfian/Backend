using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface ICoursesRepository
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(int id);
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
                    SELECT course_id, course_name, course_price, language_id, created_at, updated_at
                    FROM ms_courses
                    ORDER BY course_name";
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
                            language_id = reader.GetInt32("language_id"),
                            created_at = reader.GetDateTime("created_at"),
                            updated_at = reader.GetDateTime("updated_at")
                        });
                    }
                }
            }
            return courses;
        }

        // Read by ID
        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT course_id, course_name, course_price, language_id, created_at, updated_at
                    FROM ms_courses
                    WHERE course_id = @course_id";
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
                                language_id = reader.GetInt32("language_id"),
                                created_at = reader.GetDateTime("created_at"),
                                updated_at = reader.GetDateTime("updated_at")
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
                    INSERT INTO ms_courses (course_name, course_price, language_id, created_at, updated_at)
                    VALUES (@course_name, @course_price, @language_id, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_name", course.course_name);
                    command.Parameters.AddWithValue("@course_price", course.course_price);
                    command.Parameters.AddWithValue("@language_id", course.language_id);
                    command.Parameters.AddWithValue("@created_at", DateTime.Now);
                    command.Parameters.AddWithValue("@updated_at", DateTime.Now);

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
                    SET course_name = @course_name, course_price = @course_price, language_id = @language_id,
                        updated_at = @updated_at
                    WHERE course_id = @course_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_id", course.course_id);
                    command.Parameters.AddWithValue("@course_name", course.course_name);
                    command.Parameters.AddWithValue("@course_price", course.course_price);
                    command.Parameters.AddWithValue("@language_id", course.language_id);
                    command.Parameters.AddWithValue("@updated_at", DateTime.Now);

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