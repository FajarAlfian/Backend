using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class Course
    {
        public int course_id { get; set; }

        [Required]
        [StringLength(100)]
        public string course_name { get; set; } = string.Empty;

        [Required]
        public int course_price { get; set; }
        [Required]
        public string course_image { get; set; } = string.Empty;
        [Required]
        public string course_description { get; set; } = string.Empty;

        [Required]
        public int category_id { get; set; }

        public string category_name { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class CourseDetail
    {
        [Required]
        public string course_name { get; set; } = string.Empty;
        [Required]
        public string course_image { get; set; } = string.Empty;
        [Required]
        public string category_name { get; set; } = string.Empty;
        [Required]
        public string schedule_date { get; set; } = string.Empty;
    }
    
}