// Import namespace yang diperlukan
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DlanguageApi.Data;
using backend.Services;

// =====================================
// BUILDER PATTERN - Konfigurasi Services
// =====================================
// WebApplicationBuilder = builder pattern untuk konfigurasi aplikasi web
var builder = WebApplication.CreateBuilder(args);

// AddControllers() = mendaftarkan services untuk MVC Controllers
// Tanpa ini, controller tidak akan berfungsi
builder.Services.AddControllers();

// AddEndpointsApiExplorer() = untuk metadata endpoints API
// Diperlukan untuk Swagger documentation
builder.Services.AddEndpointsApiExplorer();

// AddSwaggerGen() = mendaftarkan Swagger generator
// Swagger = tools untuk generate dokumentasi API otomatis
builder.Services.AddSwaggerGen(options =>
{
    // Konfigurasi informasi API
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product API",
        Version = "v1",
        Description = "API untuk manajemen produk dengan fitur checkout dan payment",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@yourcompany.com"
        }
    });

    // Konfigurasi JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Masukkan JWT token dalam format: Bearer {your-token}"
    });

    // Require authentication untuk semua endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });

    // Include XML comments jika ada
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// =====================================
// AUTHENTICATION CONFIGURATION
// =====================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key not found in configuration")))
    };
});

// =====================================
// CONFIGURATION SETTINGS REGISTRATION
// =====================================
// Configure strongly typed settings objects - only configure what exists
// Note: Configuration classes need to be properly imported
// For now, we'll use basic configuration access instead of strongly typed

// Basic configuration access - no need for strongly typed classes
// var appSettings = builder.Configuration.GetSection("AppSettings");
// var emailSettings = builder.Configuration.GetSection("EmailSettings");

// TODO: Implement proper configuration classes if needed
// builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
// builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// =====================================
// DEPENDENCY INJECTION REGISTRATION
// =====================================
// AddScoped = register service dengan Scoped lifetime
// Scoped = 1 instance per HTTP request
// Only register services that actually exist and are needed

// Register UserRepository untuk authentication
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register EmailService for email functionality
builder.Services.AddScoped<IEmailService, EmailService>();

// TODO: Add other repositories as needed
// Example: builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
// Note: Make sure interface and class names match exactly

// =====================================
// CORS CONFIGURATION
// =====================================
// CORS = Cross-Origin Resource Sharing
// Diperlukan jika frontend dan backend di domain/port yang berbeda
builder.Services.AddCors(options =>
{
    // Development Policy - Allow specific localhost origins
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",    // React development server
                "http://localhost:3001",    // Alternative React port
                "http://localhost:8080",    // Vue.js development server
                "http://localhost:4200",    // Angular development server
                "http://127.0.0.1:3000",    // Alternative localhost format
                "http://127.0.0.1:3001"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();          // Allow credentials for authentication
    });

    // Production Policy - Allow specific production domains
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(
                "https://yourdomain.com",
                "https://www.yourdomain.com",
                "http://localhost:3000",     // Still allow localhost for testing
                "https://api.yourdomain.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    // Default Policy - More permissive for backward compatibility
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(
                    "https://yourdomain.com",
                    "https://www.yourdomain.com",
                    "http://localhost:3000"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// =====================================
// BUILD APPLICATION
// =====================================
// Build() = membuat WebApplication instance dari konfigurasi yang sudah didefinisikan
var app = builder.Build();

// =====================================
// HTTP REQUEST PIPELINE CONFIGURATION
// =====================================
// Pipeline = urutan middleware yang akan memproses setiap HTTP request
// Urutan middleware PENTING! Request akan melewati middleware dari atas ke bawah

// =====================================
// SWAGGER CONFIGURATION
// =====================================
// Enable Swagger in both Development and Production environments
// For production, you might want to add additional security or disable it entirely

// Always enable Swagger JSON endpoint
app.UseSwagger();

// Configure Swagger UI based on environment
if (app.Environment.IsDevelopment())
{
    // Development: Full Swagger UI with default settings
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        options.RoutePrefix = "swagger"; // Swagger UI akan tersedia di /swagger
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        options.DefaultModelsExpandDepth(-1); // Hide schemas section
    });
}
else
{
    // Production: Swagger UI with custom configuration
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        options.RoutePrefix = "api-docs"; // Swagger UI akan tersedia di /api-docs
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        options.DefaultModelsExpandDepth(-1);
        
        // Optional: Add basic authentication for production swagger
        // options.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
        
        // Custom CSS for production
        options.InjectStylesheet("/swagger-ui/custom.css");
        
        // Custom title for production
        options.DocumentTitle = "Product API Documentation - Production";
    });
}

// =====================================
// GLOBAL EXCEPTION HANDLING MIDDLEWARE
// =====================================
// Add global exception handling middleware (commented out until implemented)
// This should be one of the first middleware in the pipeline
// app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// UseHttpsRedirection() = middleware untuk redirect HTTP ke HTTPS
// Security best practice - semua request akan di-redirect ke HTTPS
app.UseHttpsRedirection();

// Enable static files for Swagger assets (CSS, JS, etc.)
app.UseStaticFiles();

// Add Swagger Basic Authentication middleware (only in production)
if (!app.Environment.IsDevelopment())
{
    // Uncomment after implementing SwaggerBasicAuthMiddleware
    // app.UseMiddleware<SwaggerBasicAuthMiddleware>();
}

// UseCors() = middleware untuk enable CORS policy yang sudah dikonfigurasi
// Harus dipanggil sebelum UseAuthorization dan MapControllers
// Use environment-specific CORS policy
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("ProductionPolicy");
}

// UseAuthentication() = middleware untuk authentication
app.UseAuthentication();

// UseAuthorization() = middleware untuk authorization/authentication
// Meskipun belum implement auth, disimpan untuk future implementation
app.UseAuthorization();

// MapControllers() = mapping route ke controller actions
// Tanpa ini, routing ke controller tidak akan berfungsi
app.MapControllers();

// =====================================
// START APPLICATION
// =====================================
// Run() = start web server dan listen untuk incoming requests
// Method ini blocking - aplikasi akan terus berjalan sampai di-stop
app.Run();

// =====================================
// PENJELASAN FLOW APLIKASI:
// =====================================
// 1. Builder pattern untuk konfigurasi services dan dependencies
// 2. Dependency Injection container akan create instance sesuai kebutuhan
// 3. HTTP pipeline akan process setiap request melalui middleware
// 4. Router akan direct request ke controller yang sesuai
// 5. Controller akan call repository untuk data access
// 6. Repository akan connect ke database dan return data
// 7. Response akan dikirim kembali ke client
