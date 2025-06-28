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
    public DateTime created_at { get; set; } = DateTime.UtcNow; 
    public DateTime updated_at { get; set; } = DateTime.UtcNow; 
}
}