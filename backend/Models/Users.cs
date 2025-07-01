using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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

        public string? password_reset_token { get; set; }
        public DateTime? PasswordResetTokenCreatedAt { get; set; }
        public string role { get; set; } = "member";

        public bool is_verified { get; set; } = false;
        
        public string? email_verification_token { get; set; }
        
        public DateTime? email_token_created_at { get; set; }
        public string? token { get; set; }


        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }


    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "member";
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
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public UserLoginInfo? User { get; set; }
        public string? Token { get; set; }
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

    // DTO untuk Forgot Password Request
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    // DTO untuk Reset Password Request
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Password dan konfirmasi password tidak sama")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
        public class VerificationStatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string Email { get; set; } = string.Empty;
    }
        public class ResendVerificationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

}