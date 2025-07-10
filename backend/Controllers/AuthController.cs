using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using DlanguageApi.Configuration;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace DlanguageApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;

        public AuthController(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
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

                var verificationToken = Guid.NewGuid().ToString();

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var newUser = new User
                {
                    username = request.Username,
                    email = request.Email,
                    password = hashedPassword,
                    role = "member",
                    updated_at = DateTime.Now,
                    is_verified = false,
                    email_verification_token = verificationToken,
                    email_token_created_at = DateTime.Now,
                    token = null
                };

                var userId = await _userRepository.CreateUserAsync(newUser);
                newUser.user_id = userId;

                bool emailSent = await _emailService.SendVerificationEmailAsync(request.Email, request.Username, verificationToken);
                if (!emailSent)
                {
                    _logger.LogError("Failed to send verification email to {Email}", request.Email);
                    return StatusCode(500, ApiResult<object>.Error("Gagal mengirim email verifikasi", 500));
                }

                return Ok(ApiResult<object>.SuccessResult(new { userId = newUser.user_id, username = newUser.username, email = newUser.email, role = newUser.role } , "Registrasi berhasil, Silahkan cek email", 201));
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
                 if (!user.is_verified)
                 {
                 return Unauthorized(ApiResult<object>.Error("Email belum diverifikasi. Silakan cek inbox Anda.", 401));
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

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Token tidak valid"
                    });
                }
                bool verified = await _userRepository.VerifyEmailAsync(token);
                if (!verified)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Token verifikasi tidak valid atau sudah expired"
                    });
                }
                _logger.LogInformation($"Email veriication succesful for token {token.Substring(0, 8)}...");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Email berhasil diverifikasi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during email verification for token: {token?.Substring(0, 8)}...");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Terjadi kesalahan server"
                });
            }
        }

            /// <summary>
            /// Check verification status
            /// GET /api/auth/verification-status?email={email}
            /// </summary>
            [HttpGet("verification-status")]

            public async Task<ActionResult<VerificationStatusResponse>> GetVerificationStatus([FromQuery] string email)
            {
                try
                {
                    if (string.IsNullOrEmpty(email))
                    {
                        return BadRequest(new VerificationStatusResponse
                        {
                            Success = false,
                            Message = "Email tidak valid"
                        });
                    }

                    var user = await _userRepository.GetUserByEmailAsync(email);
                    if (user == null)
                    {
                        return NotFound(new VerificationStatusResponse
                        {
                            Success = false,
                            Message = "Email tidak ditemukan"
                        });
                    }

                    return Ok(new VerificationStatusResponse
                    {
                        Success = true,
                        Message = "Status verifikasi berhasil diambil",
                        IsVerified = user.is_verified,
                        Email = user.email
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting verification status for email: {email}");
                    return StatusCode(500, new VerificationStatusResponse
                    {
                        Success = false,
                        Message = "Terjadi kesalahan server"
                    });
                }
            }
              [HttpPost("resend-verification")]
             public async Task<ActionResult<LoginResponse>> ResendVerification([FromBody] ResendVerificationRequest request)
             {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email tidak valid"
                    });
                }

                // Get user by email
                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email tidak ditemukan"
                    });
                }

                // Check if already verified
                if (user.is_verified)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email sudah terverifikasi"
                    });
                }

                // Generate new verification token
                string newToken = Guid.NewGuid().ToString();
                bool updated = await _userRepository.UpdateEmailVerificationTokenAsync(user.user_id, newToken);

                if (!updated)
                {
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Gagal generate token verifikasi baru"
                    });
                }

                // Send verification email
                bool emailSent = await _emailService.SendVerificationEmailAsync(
                    user.email,
                    user.username,
                    newToken
                );

                if (!emailSent)
                {
                    _logger.LogWarning($"Failed to resend verification email to {user.email}");
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Gagal mengirim email verifikasi"
                    });
                }

                _logger.LogInformation($"Verification email resent to {user.email}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Email verifikasi telah dikirim ulang. Silakan cek email Anda."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during resend verification for email: {request.Email}");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Terjadi kesalahan server"
                });
            }
        }
    

            // Endpoint untuk request reset password
            [HttpPost("forgot-password")]
            [AllowAnonymous]
            public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                    return Ok(ApiResult<object>.SuccessResult(null, "Jika email terdaftar, link reset sudah dikirim", 200));

                var resetToken = Guid.NewGuid().ToString();

                await _userRepository.UpdatePasswordResetTokenAsync(user.user_id, resetToken);

                await _emailService.SendPasswordResetEmailAsync(user.email, user.username, resetToken);

                return Ok(ApiResult<object>.SuccessResult(null, "Jika email terdaftar, link reset sudah dikirim", 200));
            }
        
            // Endpoint untuk reset password
            [HttpPost("reset-password")]
            [AllowAnonymous]
            public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));


                var user = await _userRepository.GetUserByResetTokenAsync(request.Token);
                if (user == null) return BadRequest(ApiResult<object>.Error("Token tidak valid atau sudah expired", 400));

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.password = hashedPassword;
                await _userRepository.UpdateUserAsync(user);

                return Ok(ApiResult<object>.SuccessResult(null, "Password berhasil direset", 200));
            }
        }
    }
