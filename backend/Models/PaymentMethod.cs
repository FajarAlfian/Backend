using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class PaymentMethod
    {
        public int payment_method_id { get; set; }
        [Required]
        [StringLength(100)]
        public string payment_method_name { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class PaymentMethodRequest
    {
        [Required]
        [StringLength(100)]
        public string payment_method_name { get; set; } = string.Empty;
    }
    

}