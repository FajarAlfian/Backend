using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;

namespace DlanguageApi.Controllers
{
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

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil user");
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound($"User dengan ID {id} tidak ditemukan");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil user dengan ID {user_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user_id = await _userRepository.CreateUserAsync(user);
                user.user_id = user_id;
                return CreatedAtAction(nameof(GetUser), new { id = user_id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat user baru");
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            try
            {
                if (id != user.user_id)
                    return BadRequest("ID user tidak sesuai");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                    return NotFound($"User dengan ID {id} tidak ditemukan");

                var success = await _userRepository.UpdateUserAsync(user);
                if (success)
                    return NoContent();

                return StatusCode(500, "Gagal mengupdate user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate user dengan ID {user_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                    return NotFound($"User dengan ID {id} tidak ditemukan");

                var success = await _userRepository.DeleteUserAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, "Gagal menghapus user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus user dengan ID {user_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }
    }
}