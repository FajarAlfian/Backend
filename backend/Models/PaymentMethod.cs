using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class PaymentMethod
    {
        public int payment_method_id { get; set; }

        public bool is_active { get; set; } = true;

        [Required]
        [StringLength(100)]
        public string payment_method_name { get; set; } = string.Empty;
        public string payment_method_logo { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class PaymentMethodRequest
    {
        [Required]
        [StringLength(100)]
        public string payment_method_name { get; set; } = string.Empty;
        public string payment_method_logo { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
    }
    

}