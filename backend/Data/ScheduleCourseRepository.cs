using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface IScheduleCourseRepository
    {
        Task<List<ScheduleCourse>> GetAllScheduleCourseAsync();
        Task<ScheduleCourse?> GetScheduleCourseByIdAsync(int id);
        Task<List<ScheduleCourse>> GetScheduleCourseByCourseIdAsync(int courseId);
        Task<int> CreateScheduleCourseAsync(ScheduleCourse scheduleCourse);
        Task<bool> UpdateScheduleCourseAsync(ScheduleCourse scheduleCourse);
        Task<bool> SetScheduleCourseActiveAsync(int id, bool isActive);
    }

    public class ScheduleCourseRepository : IScheduleCourseRepository
    {
        private readonly string _connectionString;

        public ScheduleCourseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        public async Task<List<ScheduleCourse>> GetAllScheduleCourseAsync()
        {
            var list = new List<ScheduleCourse>();
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
                SELECT sc.schedule_course_id,
                       sc.course_id,
                       sc.schedule_id,
                       sch.schedule_date,
                       sc.created_at,
                       sc.updated_at,
                       sc.is_active
                  FROM tr_schedule_course sc
            INNER JOIN ms_schedule sch ON sc.schedule_id = sch.schedule_id
                 WHERE sc.is_active = 1";

            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ScheduleCourse
                {
                    schedule_course_id = reader.GetInt32("schedule_course_id"),
                    course_id          = reader.GetInt32("course_id"),
                    schedule_id        = reader.GetInt32("schedule_id"),
                    schedule_date      = reader.IsDBNull(reader.GetOrdinal("schedule_date"))
                                            ? null
                                            : reader.GetString("schedule_date"),
                    created_at         = reader.GetDateTime("created_at").ToUniversalTime(),
                    updated_at         = reader.GetDateTime("updated_at").ToUniversalTime(),
                    is_active          = reader.GetBoolean("is_active")
                });
            }

            return list;
        }

        public async Task<ScheduleCourse?> GetScheduleCourseByIdAsync(int id)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
                SELECT sc.schedule_course_id,
                       sc.course_id,
                       sc.schedule_id,
                       sch.schedule_date,
                       sc.created_at,
                       sc.updated_at,
                       sc.is_active
                  FROM tr_schedule_course sc
            LEFT JOIN ms_schedule sch ON sc.schedule_id = sch.schedule_id
                 WHERE sc.schedule_course_id = @id
                   AND sc.is_active = 1";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ScheduleCourse
                {
                    schedule_course_id = reader.GetInt32("schedule_course_id"),
                    course_id          = reader.GetInt32("course_id"),
                    schedule_id        = reader.GetInt32("schedule_id"),
                    schedule_date      = reader.IsDBNull(reader.GetOrdinal("schedule_date"))
                                            ? null
                                            : reader.GetString("schedule_date"),
                    created_at         = reader.GetDateTime("created_at").ToUniversalTime(),
                    updated_at         = reader.GetDateTime("updated_at").ToUniversalTime(),
                    is_active          = reader.GetBoolean("is_active")
                };
            }

            return null;
        }

        public async Task<List<ScheduleCourse>> GetScheduleCourseByCourseIdAsync(int courseId)
        {
            var list = new List<ScheduleCourse>();
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
                SELECT sc.schedule_course_id,
                       sc.course_id,
                       sc.schedule_id,
                       sch.schedule_date,
                       sc.created_at,
                       sc.updated_at,
                       sc.is_active
                  FROM tr_schedule_course sc
            LEFT JOIN ms_schedule sch ON sc.schedule_id = sch.schedule_id
                 WHERE sc.course_id = @courseId
                   AND sc.is_active = 1";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@courseId", courseId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ScheduleCourse
                {
                    schedule_course_id = reader.GetInt32("schedule_course_id"),
                    course_id          = reader.GetInt32("course_id"),
                    schedule_id        = reader.GetInt32("schedule_id"),
                    schedule_date      = reader.IsDBNull(reader.GetOrdinal("schedule_date"))
                                            ? null
                                            : reader.GetString("schedule_date"),
                    created_at         = reader.GetDateTime("created_at").ToUniversalTime(),
                    updated_at         = reader.GetDateTime("updated_at").ToUniversalTime(),
                    is_active          = reader.GetBoolean("is_active")
                });
            }

            return list;
        }

        public async Task<int> CreateScheduleCourseAsync(ScheduleCourse scheduleCourse)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
                INSERT INTO tr_schedule_course
                    (course_id, schedule_id, created_at, updated_at, is_active)
                VALUES
                    (@course_id, @schedule_id, @created_at, @updated_at, 1);
                SELECT LAST_INSERT_ID();";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@course_id", scheduleCourse.course_id);
            cmd.Parameters.AddWithValue("@schedule_id", scheduleCourse.schedule_id);
            cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // UPDATE course & schedule
        public async Task<bool> UpdateScheduleCourseAsync(ScheduleCourse scheduleCourse)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
                UPDATE tr_schedule_course
                   SET course_id   = @course_id,
                       schedule_id = @schedule_id,
                       updated_at  = @updated_at
                 WHERE schedule_course_id = @schedule_course_id";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@schedule_course_id", scheduleCourse.schedule_course_id);
            cmd.Parameters.AddWithValue("@course_id", scheduleCourse.course_id);
            cmd.Parameters.AddWithValue("@schedule_id", scheduleCourse.schedule_id);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> SetScheduleCourseActiveAsync(int id, bool isActive)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
                UPDATE tr_schedule_course
                   SET is_active  = @isActive,
                       updated_at = @updated_at
                 WHERE schedule_course_id = @id";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@isActive",  isActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@id",          id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
