using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface IScheduleCourseRepository
    {
        Task<List<ScheduleCourse>> GetAllScheduleCourseAsync();
        Task<ScheduleCourse?> GetScheduleCourseByIdAsync(int id);
        Task<List<ScheduleCourse>> GetScheduleCourseByCourseIdAsync(int id);
        Task<int> CreateScheduleCourseAsync(ScheduleCourse ScheduleCourse);
        Task<bool> UpdateScheduleCourseAsync(ScheduleCourse ScheduleCourse);
        Task<bool> DeleteScheduleCourseAsync(int id);
    }

    public class ScheduleCourseRepository : IScheduleCourseRepository
    {
        private readonly string _connectionString;

        public ScheduleCourseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        // Read
        public async Task<List<ScheduleCourse>> GetAllScheduleCourseAsync()
        {
            var scheduleCourse = new List<ScheduleCourse>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT s.schedule_course_id, s.course_id, s.schedule_id, sch.schedule_date, s.created_at, s.updated_at
                    FROM tr_schedule_course s
                    LEFT JOIN ms_schedule sch ON s.schedule_course_id = sch.schedule_id";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        scheduleCourse.Add(new ScheduleCourse
                        {
                            schedule_course_id = reader.GetInt32("schedule_course_id"),
                            course_id = reader.GetInt32("course_id"),
                            schedule_id = reader.GetInt32("schedule_id"),
                            schedule_date = reader.IsDBNull(reader.GetOrdinal("schedule_date")) ? null : reader.GetString("schedule_date"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(), 
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                        });
                    }
                }
            }
            return scheduleCourse;
        }

        // Read by ID
        public async Task<ScheduleCourse?> GetScheduleCourseByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT s.schedule_course_id, s.course_id, s.schedule_id, sch.schedule_date, s.created_at, s.updated_at
                    FROM tr_schedule_course s
                    LEFT JOIN ms_schedule sch ON s.schedule_course_id = sch.schedule_id
                    WHERE schedule_course_id = @schedule_course_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@schedule_course_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ScheduleCourse
                            {
                                schedule_course_id = reader.GetInt32("schedule_course_id"),
                                course_id = reader.GetInt32("course_id"),
                                schedule_id = reader.GetInt32("schedule_id"),
                                schedule_date = reader.IsDBNull(reader.GetOrdinal("schedule_date")) ? null : reader.GetString("schedule_date"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(),
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Read schedule by course_id
        public async Task<List<ScheduleCourse>> GetScheduleCourseByCourseIdAsync(int id)
        {
            var scheduleCourse = new List<ScheduleCourse>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT s.schedule_course_id, s.course_id, s.schedule_id, sch.schedule_date, s.created_at, s.updated_at
                    FROM tr_schedule_course s
                    LEFT JOIN ms_schedule sch ON s.schedule_course_id = sch.schedule_id
                    WHERE course_id = @course_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            scheduleCourse.Add(new ScheduleCourse
                            {
                                schedule_course_id = reader.GetInt32("schedule_course_id"),
                                course_id = reader.GetInt32("course_id"),
                                schedule_id = reader.GetInt32("schedule_id"),
                                schedule_date = reader.IsDBNull(reader.GetOrdinal("schedule_date")) ? null : reader.GetString("schedule_date"),
                                created_at = reader.GetDateTime("created_at").ToUniversalTime(), 
                                updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                            });
                        }
                    }
                }
            }
            return scheduleCourse;
        }
        
        // Create
        public async Task<int> CreateScheduleCourseAsync(ScheduleCourse scheduleCourse)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    INSERT INTO tr_schedule_course (course_id, schedule_id, created_at, updated_at)
                    VALUES (@course_id, @schedule_id, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@course_id", scheduleCourse.course_id);
                    command.Parameters.AddWithValue("@schedule_id", scheduleCourse.schedule_id);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        // Update
        public async Task<bool> UpdateScheduleCourseAsync(ScheduleCourse scheduleCourse)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    UPDATE tr_schedule_course
                    SET course_id = @course_id, schedule_id = @schedule_id, updated_at = @updated_at
                    WHERE schedule_course_id = @schedule_course_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@schedule_course_id", scheduleCourse.schedule_course_id);
                    command.Parameters.AddWithValue("@course_id", scheduleCourse.course_id);
                    command.Parameters.AddWithValue("@schedule_id", scheduleCourse.schedule_id);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        // Delete
        public async Task<bool> DeleteScheduleCourseAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    DELETE FROM tr_schedule_course
                    WHERE schedule_course_id = @schedule_course_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@schedule_course_id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}