using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class Checkout
    {
        public int cart_product_id { get; set; }
        public int course_id { get; set; }
        public string course_name { get; set; } = string.Empty;
        public int course_price { get; set; }
        public int user_id { get; set; }
        public int schedule_course_id { get; set; }
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class GetCheckout
    {
        public int cart_product_id { get; set; }
        public int course_id { get; set; }
        public string course_image { get; set; } = string.Empty;
        public string course_name { get; set; } = string.Empty;
        public string category_name { get; set; } = string.Empty;
        public int course_price { get; set; }
        public int user_id { get; set; }
        public string schedule_date { get; set; }
        public DateTime created_at { get; set; } = DateTime.UtcNow; 
        public DateTime updated_at { get; set; } = DateTime.UtcNow; 
    }
}