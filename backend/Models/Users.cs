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
 
        public DateTime created_at { get; set; } = DateTime.UtcNow; 
        public DateTime updated_at { get; set; } = DateTime.UtcNow; 
    }


    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}