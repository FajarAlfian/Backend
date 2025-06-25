using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class User
    {
        public int user_id { get; set; }

        [Required]
        [StringLength(100)]
        public string username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string password { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.Now; 
        public DateTime updated_at { get; set; } = DateTime.Now; 
    }
}