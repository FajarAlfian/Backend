using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net; 

namespace DlanguageApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));
                }

                var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return Conflict(ApiResult<object>.Error("Email sudah terdaftar", 409));
                }

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var newUser = new User
                {
                    username = request.Username,
                    email = request.Email,
                    password = hashedPassword,
                    role = "member",
                    updated_at = DateTime.Now
                };

                var userId = await _userRepository.CreateUserAsync(newUser);
                newUser.user_id = userId;

                return Ok(ApiResult<object>.SuccessResult(new { userId = newUser.user_id, username = newUser.username, email = newUser.email, role = newUser.role }, "Registrasi berhasil", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration.");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.password))
                {
                    return Unauthorized(ApiResult<object>.Error("Email atau password salah", 401));
                }

                // Generate JWT Token
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.username),
                    new Claim(JwtRegisteredClaimNames.Email, user.email),
                    new Claim("userId", user.user_id.ToString()),
                    new Claim(ClaimTypes.Role, user.role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])),
                    signingCredentials: credentials);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                var authResponse = new AuthResponse
                {
                    UserId = user.user_id,
                    Username = user.username,
                    Email = user.email,
                    Role = user.role,
                    Token = tokenString
                };

                return Ok(ApiResult<AuthResponse>.SuccessResult(authResponse, "Login berhasil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login.");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}