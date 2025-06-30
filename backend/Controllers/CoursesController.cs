using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DlanguageApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesRepository _coursesRepository;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICoursesRepository coursesRepository, ILogger<CoursesController> logger)
        {
            _coursesRepository = coursesRepository;
            _logger = logger;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<Course>>> GetCourses()
        {
            try
            {
                var courses = await _coursesRepository.GetAllCoursesAsync();
                return Ok(ApiResult<List<Course>>.SuccessResult(courses, "Daftar kursus berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil daftar kursus");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            try
            {
                var course = await _coursesRepository.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound(ApiResult<object>.Error($"Kursus dengan ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<Course>.SuccessResult(course, "Kursus berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil kursus dengan ID {course_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [AllowAnonymous]
        [HttpGet("category/{id}")]
        public async Task<ActionResult<Course>> GetCourseByCategoryId(int id)
        {
            try
            {
                var course = await _coursesRepository.GetCourseByCategoryIdAsync(id);
                if (course == null)
                    return NotFound(ApiResult<object>.Error($"Kursus dengan category ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<Course>.SuccessResult(course, "Kursus berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil kursus dengan category ID {course_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse([FromBody] Course course)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var course_id = await _coursesRepository.CreateCourseAsync(course);
                course.course_id = course_id;
                return CreatedAtAction(nameof(GetCourse), new { id = course_id }, ApiResult<Course>.SuccessResult(course, "Kursus berhasil dibuat", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat kursus baru");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course course)
        {
            try
            {
                if (id != course.course_id)
                    return BadRequest(ApiResult<object>.Error("ID kursus tidak sesuai", 400));

                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var existingCourse = await _coursesRepository.GetCourseByIdAsync(id);
                if (existingCourse == null)
                    return NotFound(ApiResult<object>.Error($"Kursus dengan ID {id} tidak ditemukan", 404));

                var success = await _coursesRepository.UpdateCourseAsync(course);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal mengupdate kursus", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate kursus dengan ID {course_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var existingCourse = await _coursesRepository.GetCourseByIdAsync(id);
                if (existingCourse == null) 
                    return NotFound(ApiResult<object>.Error($"Kursus dengan ID {id} tidak ditemukan", 404));

                var success = await _coursesRepository.DeleteCourseAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal menghapus kursus", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus kursus dengan ID {course_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}