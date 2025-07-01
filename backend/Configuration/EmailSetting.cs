namespace DlanguageApi.Configuration
{
    /// <summary>
    /// Configuration class untuk email settings
    /// Digunakan untuk SMTP configuration, from email, dll
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// SMTP server hostname (contoh: smtp.gmail.com, smtp.outlook.com)
        /// </summary>
        public string SmtpServer { get; set; } = string.Empty;

        /// <summary>
        /// SMTP server port (biasanya 587 untuk STARTTLS, 465 untuk SSL)
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// Enable SSL/TLS encryption untuk SMTP connection
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// Email address yang akan muncul sebagai sender
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// Nama yang akan muncul sebagai sender name
        /// </summary>
        public string FromName { get; set; } = string.Empty;

        /// <summary>
        /// Username untuk SMTP authentication (biasanya sama dengan FromEmail)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password untuk SMTP authentication
        /// Untuk Gmail: gunakan App Password, bukan password biasa
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
