using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;

namespace DlanguageApi.Controllers
{
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

        [HttpGet]
        public async Task<ActionResult<List<Course>>> GetCourses()
        {
            try
            {
                var courses = await _coursesRepository.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil daftar kursus");
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            try
            {
                var course = await _coursesRepository.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound($"Kursus dengan ID {id} tidak ditemukan");
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil kursus dengan ID {course_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse([FromBody] Course course)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var course_id = await _coursesRepository.CreateCourseAsync(course);
                course.course_id = course_id;
                return CreatedAtAction(nameof(GetCourse), new { id = course_id }, course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat kursus baru");
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course course)
        {
            try
            {
                if (id != course.course_id)
                    return BadRequest("ID kursus tidak sesuai");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingCourse = await _coursesRepository.GetCourseByIdAsync(id);
                if (existingCourse == null)
                    return NotFound($"Kursus dengan ID {id} tidak ditemukan");

                var success = await _coursesRepository.UpdateCourseAsync(course);
                if (success)
                    return NoContent();

                return StatusCode(500, "Gagal mengupdate kursus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate kursus dengan ID {course_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var existingCourse = await _coursesRepository.GetCourseByIdAsync(id);
                if (existingCourse == null)
                    return NotFound($"Kursus dengan ID {id} tidak ditemukan");

                var success = await _coursesRepository.DeleteCourseAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, "Gagal menghapus kursus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus kursus dengan ID {course_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }
    }
}