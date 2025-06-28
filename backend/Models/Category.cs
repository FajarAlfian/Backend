using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class Category
    {
        public int category_id { get; set; }

        [Required]
        [StringLength(100)]
        public string category_name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string category_description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string category_image { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class CategoryRequest
    {
        [Required]
        [StringLength(100)]
        public string category_name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string category_description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string category_image { get; set; } = string.Empty;
    }
    

}