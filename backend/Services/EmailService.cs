using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using DlanguageApi.Configuration;

namespace backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly AppSettings _appSettings;

        public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger,
        IOptions<AppSettings> appSettings)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

 public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null)
        {
            try
            {
                // Validasi input parameters
                if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(subject))
                {
                    _logger.LogWarning("Email sending failed: Missing required parameters (to: {To}, subject: {Subject})", to, subject);
                    return false;
                }

                // Create MimeMessage object
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                // Create body builder untuk HTML dan text
                var bodyBuilder = new BodyBuilder();
                
                if (!string.IsNullOrWhiteSpace(htmlBody))
                {
                    bodyBuilder.HtmlBody = htmlBody;
                }
                
                if (!string.IsNullOrWhiteSpace(textBody))
                {
                    bodyBuilder.TextBody = textBody;
                }
                else if (!string.IsNullOrWhiteSpace(htmlBody))
                {
                    // Generate plain text dari HTML sebagai fallback
                    bodyBuilder.TextBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, "<.*?>", "");
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Send email menggunakan SMTP client
                using var client = new SmtpClient();
                
                // Connect ke SMTP server dengan SSL/TLS
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, 
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                // Authenticate dengan username dan password
                if (!string.IsNullOrWhiteSpace(_emailSettings.Username) && !string.IsNullOrWhiteSpace(_emailSettings.Password))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }
                
                // Send message
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {To} with subject '{Subject}'", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'. Error: {Error}", to, subject, ex.Message);
                return false;
            }
        }
        public async Task<bool> SendPasswordResetEmailAsync(string to, string userName, string resetToken)
        {
            try
            {
                var subject = "Reset Password Anda - " + _appSettings.AppName;
                var baseUrl = _appSettings.FrontendBaseUrl;
                var resetLink = $"{baseUrl}/reset-password?token={resetToken}";

                var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); color: white; padding: 30px; text-align: center; border-radius: 10px; }}
                        .btn {{ display: inline-block; padding: 15px 30px; background: #ff6b6b; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
                        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔑 Reset Password</h1>
                        </div>
                        
                        <h2>Halo {userName},</h2>
                        <p>Kami menerima permintaan untuk reset password akun Anda di {_appSettings.AppName}.</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' class='btn'>Reset Password Sekarang</a>
                        </div>
                        
                        <div class='warning'>
                            <h4>⚠️ Informasi Penting:</h4>
                            <ul>
                                <li>Link ini akan kedaluwarsa dalam <strong>1 jam</strong></li>
                                <li>Jika Anda tidak meminta reset password, abaikan email ini</li>
                                <li>Password Anda tidak akan berubah sampai Anda klik link di atas</li>
                            </ul>
                        </div>
                        
                        <p>Link alternatif: {resetLink}</p>
                        <p>Reset Token: {resetToken}</p>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(to, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", to);
                return false;
            }
        }
    }
}
    

