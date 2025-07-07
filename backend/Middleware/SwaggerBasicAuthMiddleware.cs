using Microsoft.AspNetCore.Http;
using System.Text;

namespace backend.Middleware
{
    /// <summary>
    /// Middleware untuk mengamankan Swagger UI di production dengan Basic Authentication
    /// </summary>
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Hanya check authentication untuk path swagger/api-docs
            if (context.Request.Path.StartsWithSegments("/api-docs") || 
                context.Request.Path.StartsWithSegments("/swagger"))
            {
                // Skip authentication di development
                if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                {
                    await _next(context);
                    return;
                }

                // Get credentials dari configuration
                var username = _configuration["SwaggerAuth:Username"] ?? "admin";
                var password = _configuration["SwaggerAuth:Password"] ?? "swagger123";

                // Check Authorization header
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    // Extract credentials
                    var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                    var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                    var usernamePasswordArray = decodedUsernamePassword.Split(':', 2);
                    var receivedUsername = usernamePasswordArray[0];
                    var receivedPassword = usernamePasswordArray[1];

                    // Validate credentials
                    if (receivedUsername == username && receivedPassword == password)
                    {
                        await _next(context);
                        return;
                    }
                }

                // Return 401 Unauthorized dengan WWW-Authenticate header
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger Documentation\"";
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized access to Swagger documentation");
                return;
            }

            await _next(context);
        }
    }
}
