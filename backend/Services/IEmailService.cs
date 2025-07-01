

namespace backend.Services
{
    /// <summary>
    /// Interface untuk email service yang menghandle pengiriman email
    /// Digunakan untuk verification, reset password, notifications, dll
    /// </summary>
    public interface IEmailService
    {

        Task<bool> SendPasswordResetEmailAsync(string to, string userName, string resetToken);
    }
}
