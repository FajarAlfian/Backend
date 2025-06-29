using System.Data;
using MySql.Data.MySqlClient;
using DlanguageApi.Models;

namespace DlanguageApi.Data
{
    public interface IScheduleRepository
    {
        Task<List<Schedule>> GetAllScheduleAsync();
        Task<Schedule?> GetScheduleByIdAsync(int id);
        Task<int> CreateScheduleAsync(Schedule Schedule);
        Task<bool> UpdateScheduleAsync(Schedule Schedule);
        Task<bool> DeleteScheduleAsync(int id);
    }

    public class ScheduleRepository : IScheduleRepository
    {
        private readonly string _connectionString;

        public ScheduleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        // Read
        public async Task<List<Schedule>> GetAllScheduleAsync()
        {
            var schedule = new List<Schedule>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT schedule_id, schedule_date, created_at, updated_at
                    FROM ms_schedule";
                using (var command = new MySqlCommand(queryString, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        schedule.Add(new Schedule
                        {
                            schedule_id = reader.GetInt32("schedule_id"),
                            schedule_date = reader.GetString("schedule_date"),
                            created_at = reader.GetDateTime("created_at").ToUniversalTime(), 
                            updated_at = reader.GetDateTime("updated_at").ToUniversalTime()
                        });
                    }
                }
            }
            return schedule;
        }

        // Read by ID
        public async Task<Schedule?> GetScheduleByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    SELECT schedule_id, schedule_date, created_at, updated_at
                    FROM ms_schedule
                    WHERE schedule_id = @schedule_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@schedule_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Schedule
                            {
                                schedule_id = reader.GetInt32("schedule_id"),
                                schedule_date = reader.GetString("schedule_date"),
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
        public async Task<int> CreateScheduleAsync(Schedule schedule)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    INSERT INTO ms_schedule (schedule_date, created_at, updated_at)
                    VALUES (@schedule_date, @created_at, @updated_at);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@schedule_date", schedule.schedule_date);
                    command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        // Update
        public async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    UPDATE ms_schedule
                    SET schedule_date = @schedule_date, updated_at = @updated_at
                    WHERE schedule_id = @schedule_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@schedule_id", schedule.schedule_id);
                    command.Parameters.AddWithValue("@schedule_date", schedule.schedule_date);
                    command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        
        // Delete
        public async Task<bool> DeleteScheduleAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string queryString = @"
                    DELETE FROM ms_schedule
                    WHERE schedule_id = @schedule_id";
                using (var command = new MySqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@schedule_id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}