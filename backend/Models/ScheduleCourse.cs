using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class ScheduleCourse
    {
        public int schedule_course_id { get; set; }

        [Required]
        public int course_id { get; set; } 

        [Required]
        public int schedule_id { get; set; }
        public string schedule_date { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class ScheduleCourseRequest
    {
        [Required]
        public int course_id { get; set; } 

        [Required]
        public int schedule_id { get; set; }
    }
    

}