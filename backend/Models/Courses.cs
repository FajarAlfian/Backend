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
        public int language_id { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now; 
        public DateTime updated_at { get; set; } = DateTime.Now; 
    }
}