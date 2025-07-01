

namespace backend.Services
{
    /// <summary>
    /// Interface untuk email service yang menghandle pengiriman email
    /// Digunakan untuk verification, reset password, notifications, dll
    /// </summary>
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string to, string userName);
        Task<bool> SendPasswordResetEmailAsync(string to, string userName, string resetToken);
        Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null);
    }
}
