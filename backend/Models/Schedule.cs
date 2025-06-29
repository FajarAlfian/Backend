using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class Schedule
    {
        public int schedule_id { get; set; }

        [Required]
        public string schedule_date { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class ScheduleRequest
    {
        [Required]
        public string schedule_date { get; set; } = string.Empty;
    }
    

}