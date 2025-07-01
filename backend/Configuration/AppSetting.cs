namespace DlanguageApi.Configuration
{
    /// <summary>
    /// Konfigurasi umum aplikasi untuk mengatur perilaku dasar sistem
    /// Digunakan untuk pagination, timeout, caching, dll
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Nama aplikasi - digunakan untuk logging, dokumentasi, dan header response
        /// </summary>
        public string ApplicationName { get; set; } = string.Empty;
        
        /// <summary>
        /// Versi aplikasi - untuk tracking deployment dan debugging
        /// Format: "1.0.0" atau "v2.1.3"
        /// </summary>
        public string Version { get; set; } = string.Empty;
        
        /// <summary>
        /// Maksimal jumlah item yang dapat diminta per halaman
        /// Mencegah overload server dengan request data yang terlalu besar
        /// Contoh: 100 item maximum per page
        /// </summary>
        public int MaxPageSize { get; set; }
        
        /// <summary>
        /// Jumlah default item per halaman jika tidak dispesifikasi
        /// Contoh: 10 item per page sebagai default
        /// </summary>
        public int DefaultPageSize { get; set; }
        
        /// <summary>
        /// Timeout untuk API request dalam milisecond
        /// Contoh: 30000 = 30 detik timeout
        /// </summary>
        public int ApiTimeout { get; set; }
        
        /// <summary>
        /// Flag untuk mengaktifkan/nonaktifkan caching di aplikasi
        /// true = caching aktif, false = caching tidak aktif
        /// </summary>
        public bool EnableCaching { get; set; }
        
        /// <summary>
        /// Durasi cache dalam menit sebelum data expired dan harus refresh
        /// Contoh: 15 = cache selama 15 menit
        /// </summary>
        public int CacheExpirationMinutes { get; set; }

        /// <summary>
        /// Base URL aplikasi frontend untuk generating links
        /// </summary>
        public string FrontendBaseUrl { get; set; } = "http://localhost:3000";

        /// <summary>
        /// Base URL aplikasi backend API
        /// </summary>
        public string BackendBaseUrl { get; set; } = "http://localhost:5009";

        /// <summary>
        /// Nama aplikasi untuk branding di email
        /// </summary>
        public string AppName { get; set; } = "Dlanguage";
    }

    /// <summary>
    /// Konfigurasi keamanan aplikasi untuk JWT, password policy, dan login protection
    /// Mengatur aturan authentication dan authorization
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Secret key untuk menandatangani JWT token
        /// HARUS panjang minimal 256 bit (32 karakter) dan dijaga kerahasiaannya
        /// Contoh: "YourJwtSecretKeyHere_MakeItLongAndSecure_AtLeast256Bits"
        /// </summary>
        public string JwtSecret { get; set; } = string.Empty;
        
        /// <summary>
        /// Durasi token JWT valid dalam jam sebelum harus login ulang
        /// Contoh: 24 = token berlaku 24 jam
        /// </summary>
        public int JwtExpirationHours { get; set; }
        
        /// <summary>
        /// Minimal panjang password yang diizinkan
        /// Contoh: 8 = password minimal 8 karakter
        /// </summary>
        public int PasswordMinLength { get; set; }
        
        /// <summary>
        /// Wajibkan password mengandung huruf besar (A-Z)
        /// true = harus ada huruf besar, false = tidak wajib
        /// </summary>
        public bool RequireUppercase { get; set; }
        
        /// <summary>
        /// Wajibkan password mengandung huruf kecil (a-z)
        /// true = harus ada huruf kecil, false = tidak wajib
        /// </summary>
        public bool RequireLowercase { get; set; }
        
        /// <summary>
        /// Wajibkan password mengandung angka (0-9)
        /// true = harus ada angka, false = tidak wajib
        /// </summary>
        public bool RequireDigit { get; set; }
        
        /// <summary>
        /// Wajibkan password mengandung karakter khusus (!@#$%^&*)
        /// true = harus ada karakter khusus, false = tidak wajib
        /// </summary>
        public bool RequireSpecialChar { get; set; }
        
        /// <summary>
        /// Maksimal percobaan login gagal sebelum account dikunci
        /// Contoh: 5 = setelah 5x salah password, account dikunci
        /// </summary>
        public int MaxFailedAttempts { get; set; }
        
        /// <summary>
        /// Durasi account dikunci dalam menit setelah mencapai maksimal percobaan gagal
        /// Contoh: 15 = account dikunci selama 15 menit
        /// </summary>
        public int LockoutTimeMinutes { get; set; }
    }

    /// <summary>
    /// Konfigurasi untuk upload file termasuk validasi ukuran, tipe, dan lokasi penyimpanan
    /// Mengatur batasan keamanan untuk file upload
    /// </summary>
    public class FileUploadSettings
    {
        /// <summary>
        /// Maksimal ukuran file yang dapat diupload dalam MB (MegaByte)
        /// Contoh: 10 = maksimal 10 MB per file
        /// Mencegah upload file yang terlalu besar yang bisa membebani server
        /// </summary>
        public int MaxFileSizeMB { get; set; }
        
        /// <summary>
        /// Daftar ekstensi file yang diperbolehkan untuk diupload
        /// Contoh: [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx"]
        /// Mencegah upload file berbahaya seperti .exe, .bat, .php
        /// </summary>
        public List<string> AllowedFileTypes { get; set; } = new List<string>();
        
        /// <summary>
        /// Folder di server tempat file disimpan setelah diupload
        /// Contoh: "wwwroot/uploads" atau "C:/uploads"
        /// Path bisa relatif atau absolut
        /// </summary>
        public string UploadPath { get; set; } = string.Empty;
    }
}
