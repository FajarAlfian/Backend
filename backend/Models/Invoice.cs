using System.ComponentModel.DataAnnotations;

namespace DlanguageApi.Models
{
    public class Invoice
    {   
        public int invoice_id { get; set; }
        public string invoice_number { get; set; } = string.Empty;
        
        public string invoice_date { get; set; } = string.Empty;

        public int user_id { get; set; }

        public double total_price { get; set; }
        public int payment_method_id { get; set; }
        public string payment_method_name { get; set; } = string.Empty;
        public string category_name { get; set; } = string.Empty;
        public int total_courses { get; set; } = 0;

        public bool isPaid { get; set; } = false;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;

    }

    public class InvoiceCreateRequest
    {
        [Required]
        public int user_id { get; set; }

        [Required]
        public int payment_method_id { get; set; }
        [Required]
        public string payment_method_name { get; set; } = string.Empty;
        
    }
    public class InvoiceUpdateRequest
    {
        [Required]
        public int invoice_id { get; set; }
        [Required]
        public int user_id { get; set; }

        [Required]
        public int payment_method_id { get; set; }

        public string payment_method_name { get; set; } = string.Empty;
    

        [Required]
        public bool isPaid { get; set; } = false;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }
}

