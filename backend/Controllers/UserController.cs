using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;

namespace DlanguageApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers([FromQuery] string? search = null)
        {
            try
            {
            var users = string.IsNullOrWhiteSpace(search)
            ? await _userRepository.GetAllUsersAsync()
            : await _userRepository.SearchUsersAsync(search);
                return Ok(ApiResult<List<User>>.SuccessResult(users, "Daftar user berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil user");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(ApiResult<object>.Error($"User dengan ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<User>.SuccessResult(user, "User berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil user dengan ID {user_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var user_id = await _userRepository.CreateUserAsync(user);
                user.user_id = user_id;
                return CreatedAtAction(nameof(GetUser), new { id = user_id }, ApiResult<User>.SuccessResult(user, "User berhasil dibuat", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat user baru");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            try
            {
                if (id != user.user_id)
                    return BadRequest(ApiResult<object>.Error("ID user tidak sesuai", 400));

                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                    return NotFound(ApiResult<object>.Error($"User dengan ID {id} tidak ditemukan", 404));

                var success = await _userRepository.UpdateUserAsync(user);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal mengupdate user", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate user dengan ID {user_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{id}")]
        [Consumes("application/json-patch+json")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] JsonPatchDocument<User> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest(ApiResult<object>.Error("Payload patch tidak boleh kosong", 400));

            var existing = await _userRepository.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResult<object>.Error($"User ID {id} tidak ditemukan", 404));

            patchDoc.ApplyTo(existing, ModelState);
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<object>.Error(
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

            existing.updated_at = DateTime.UtcNow;
            var ok = await _userRepository.UpdateUserAsync(existing);
            if (!ok)
                return StatusCode(500, ApiResult<object>.Error("Gagal menyimpan perubahan", 500));

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null) 
                    return NotFound(ApiResult<object>.Error($"User dengan ID {id} tidak ditemukan", 404));

                var success = await _userRepository.DeleteUserAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal menghapus user", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus user dengan ID {user_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}