// Import namespace untuk ProductRepository
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DlanguageApi.Data;

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
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[]{}
        }
    });
});

// =====================================
// DEPENDENCY INJECTION REGISTRATION
// =====================================
// AddScoped = register service dengan Scoped lifetime
// Scoped = 1 instance per HTTP request
// IProductRepository akan di-resolve ke ProductRepository
// Setiap kali controller butuh IProductRepository, DI container akan provide ProductRepository instance
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICheckoutRepository, CheckoutRepository>();
builder.Services.AddScoped<ICoursesRepository, CourseRepository>();
builder.Services.AddScoped<ICategoriesRepository, CategoryRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IScheduleCourseRepository, ScheduleCourseRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
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
// CORS CONFIGURATION
// =====================================
// CORS = Cross-Origin Resource Sharing
// Diperlukan jika frontend dan backend di domain/port yang berbeda
builder.Services.AddCors(options =>
{
    // AddDefaultPolicy = kebijakan CORS default
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()      // Boleh dari origin mana saja (untuk development)
              .AllowAnyMethod()      // Boleh HTTP method apa saja (GET, POST, PUT, DELETE)
              .AllowAnyHeader();     // Boleh header apa saja
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

// Environment check - hanya aktif di Development environment
if (app.Environment.IsDevelopment())
{
    // UseSwagger() = middleware untuk expose Swagger JSON endpoint
    // Biasanya di: /swagger/v1/swagger.json
    app.UseSwagger();
    
    // UseSwaggerUI() = middleware untuk Swagger UI web interface
    // Biasanya di: /swagger/index.html
    app.UseSwaggerUI();
}

// UseHttpsRedirection() = middleware untuk redirect HTTP ke HTTPS
// Security best practice - semua request akan di-redirect ke HTTPS
app.UseHttpsRedirection();

// UseCors() = middleware untuk enable CORS policy yang sudah dikonfigurasi
// Harus dipanggil sebelum UseAuthorization dan MapControllers
app.UseCors();
app.UseAuthentication(); // Must be before UseAuthorization

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