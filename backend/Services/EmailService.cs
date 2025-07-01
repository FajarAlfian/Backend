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

          public async Task<bool> SendVerificationEmailAsync(string to, string userName)
        {
            try
            {
                var subject = "Verifikasi Email Anda - ProductAPI";
                
                // Base URL untuk verification link (bisa dari frontend atau backend)
                var baseUrl = _appSettings.FrontendBaseUrl;
                
                // HTML template yang professional dan menarik
                var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Email Verification</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f4; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; margin: -20px -20px 20px -20px; }}
                        .header h1 {{ margin: 0; font-size: 28px; }}
                        .content {{ padding: 20px 0; }}
                        .verification-box {{ background-color: #f8f9fa; border: 2px solid #e9ecef; border-radius: 8px; padding: 20px; margin: 20px 0; text-align: center; }}
                        .btn {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 10px 0; }}
                        .btn:hover {{ background: linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%); }}
                        .token-info {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 10px; border-radius: 5px; margin: 10px 0; font-family: monospace; word-break: break-all; }}
                        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e9ecef; font-size: 12px; color: #6c757d; }}
                        .security-note {{ background-color: #d1ecf1; border: 1px solid #bee5eb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔐 Verifikasi Email</h1>
                            <p>{_appSettings.AppName} - E-commerce Platform</p>
                        </div>
                        
                        <div class='content'>
                            <h2>Halo {userName}! 👋</h2>
                            <p>Terima kasih telah mendaftar di <strong>{_appSettings.AppName}</strong>. Untuk mengaktifkan akun Anda dan mulai berbelanja, silakan verifikasi email Anda dengan mengklik tombol di bawah ini:</p>
                            
                            <div class='verification-box'>
                                <h3>✅ Verifikasi Email Anda</h3>
                                <p>Klik tombol ini untuk mengonfirmasi email Anda:</p>
                                <a href='' class='btn'>Verifikasi Email Sekarang</a>
                            </div>
                            
                            <div class='security-note'>
                                <h4>🔒 Informasi Keamanan:</h4>
                                <ul>
                                    <li>Link verifikasi ini akan <strong>kedaluwarsa dalam 24 jam</strong></li>
                                    <li>Jika Anda tidak mendaftar di {_appSettings.AppName}, abaikan email ini</li>
                                    <li>Jangan bagikan link ini kepada orang lain</li>
                                </ul>
                            </div>
                            
                            <p><strong>Alternatif:</strong> Jika tombol di atas tidak berfungsi, copy dan paste link berikut di browser Anda:</p>
                            <div class='token-info'>
                               
                            </div>
                            
                            <p><strong>Verification Token:</strong></p>
                            <div class='token-info'>
                              
                            </div>
                        </div>
                        
                        <div class='footer'>
                            <p>Email ini dikirim secara otomatis oleh sistem {_appSettings.AppName}.</p>
                            <p>Jika Anda memiliki pertanyaan, hubungi kami di support@{_appSettings.AppName.ToLower()}.com</p>
                            <p>&copy; 2025 {_appSettings.AppName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

                // Plain text fallback
                var textBody = $@"
                Halo {userName}!
                
                Terima kasih telah mendaftar di {_appSettings.AppName}.
                
                Untuk mengaktifkan akun Anda, silakan verifikasi email dengan mengunjungi link berikut:
                
                
                Atau gunakan verification token ini: 
                
                Link ini akan kedaluwarsa dalam 24 jam.
                
                Jika Anda tidak mendaftar di {_appSettings.AppName}, abaikan email ini.
                
                Terima kasih,
                Tim {_appSettings.AppName}
                ";

                var result = await SendEmailAsync(to, subject, htmlBody, textBody);
                
                if (result)
                {
                    _logger.LogInformation("Verification email sent successfully to {Email} for user {UserName}", to, userName);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email} for user {UserName}", to, userName);
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
    

