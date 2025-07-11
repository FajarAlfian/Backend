using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DlanguageApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleCourseController : ControllerBase
    {
        private readonly IScheduleCourseRepository _scheduleCourseRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ILogger<ScheduleCourseController> _logger;

        public ScheduleCourseController(
            IScheduleCourseRepository scheduleCourseRepository,
            IScheduleRepository scheduleRepository,
            ILogger<ScheduleCourseController> logger)
        {
            _scheduleCourseRepository = scheduleCourseRepository;
            _scheduleRepository = scheduleRepository;
            _logger = logger;
        }

        [Authorize(Roles = "admin")]
        [HttpPost("with-date")]
        public async Task<ActionResult> CreateScheduleCourseWithDate(
            [FromBody] ScheduleCourseWithDateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<object>.Error(
                    ModelState.Values.SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToList(), 400));

            var newSchedule = new Schedule
            {
                schedule_date = request.schedule_date,
                created_at = DateTime.Now
            };
            var scheduleId = await _scheduleRepository.CreateScheduleAsync(newSchedule);

            var newScheduleCourse = new ScheduleCourse
            {
                course_id = request.course_id,
                schedule_id = scheduleId,
                created_at = DateTime.Now
            };
            var scheduleCourseId = await _scheduleCourseRepository
                                        .CreateScheduleCourseAsync(newScheduleCourse);

            var result = new
            {
                schedule_course_id = scheduleCourseId,
                course_id = request.course_id,
                schedule_id = scheduleId,
                schedule_date = request.schedule_date
            };
            return Ok(ApiResult<object>
                .SuccessResult(result, "ScheduleCourse dengan date berhasil dibuat", 201));
        }
        [AllowAnonymous]
        [HttpGet]

        public async Task<ActionResult<List<ScheduleCourse>>> GetScheduleCourse()
        {
            try
            {
                var scheduleCourses = await _scheduleCourseRepository.GetAllScheduleCourseAsync();
                return Ok(ApiResult<List<ScheduleCourse>>.SuccessResult(scheduleCourses, "Schedule course berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil data schedule course");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]

        public async Task<ActionResult<ScheduleCourse>> GetScheduleCourse(int id)
        {
            try
            {
                var scheduleCourses = await _scheduleCourseRepository.GetScheduleCourseByIdAsync(id);
                if (scheduleCourses == null)
                    return NotFound(ApiResult<object>.Error($"Schedule course dengan ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<ScheduleCourse>.SuccessResult(scheduleCourses, "Schedule course berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil schedule course dengan ID {schedule_course_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        [AllowAnonymous]
        [HttpGet("course/{id}")]
        public async Task<ActionResult<ScheduleCourse>> GetScheduleByCourseID(int id)
        {
            try
            {
                var scheduleCourses = await _scheduleCourseRepository.GetScheduleCourseByCourseIdAsync(id);
                if (scheduleCourses == null)
                    return NotFound(ApiResult<object>.Error($"Schedule course dengan course ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<List<ScheduleCourse>>.SuccessResult(scheduleCourses, "Schedule course berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil data schedule course");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }

        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<ScheduleCourse>> CreateScheduleCourse([FromBody] ScheduleCourseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));
                var newScheduleCourse = new ScheduleCourse
                {
                    course_id = request.course_id,
                    schedule_id = request.schedule_id,
                    created_at = DateTime.Now
                };
                var schedule_course_id = await _scheduleCourseRepository.CreateScheduleCourseAsync(newScheduleCourse);
                newScheduleCourse.schedule_course_id = schedule_course_id;
                return Ok(ApiResult<object>.SuccessResult(new { schedule_course_id, course_id = newScheduleCourse.course_id, schedule_id = newScheduleCourse.schedule_id }, "Add schedule course berhasil", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during add schedule course.");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScheduleCourse(int id, [FromBody] ScheduleCourseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var existingScheduleCourse = await _scheduleCourseRepository.GetScheduleCourseByIdAsync(id);
                if (existingScheduleCourse == null)
                    return NotFound(ApiResult<object>.Error($"schedule course dengan ID {id} tidak ditemukan", 404));
                var updateData = new ScheduleCourse
                {
                    schedule_course_id = id,
                    course_id = request.course_id,
                    schedule_id = request.schedule_id
                };
                var success = await _scheduleCourseRepository.UpdateScheduleCourseAsync(updateData);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal mengupdate schedule course ", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate schedule course  dengan ID {schedule_course_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }


        [Authorize(Roles = "admin")]
        [HttpPatch("{id}/active")]
        public async Task<IActionResult> ToggleActive(int id, [FromBody] ActiveDto dto)
        {
            try
            {
                var existing = await _scheduleCourseRepository.GetScheduleCourseByIdAsync(id);
                if (existing == null)
                    return NotFound(ApiResult<object>.Error($"ScheduleCourse dengan ID {id} tidak ditemukan", 404));

                var success = await _scheduleCourseRepository.SetScheduleCourseActiveAsync(id, dto.IsActive);
                if (!success)
                    return StatusCode(500, ApiResult<object>.Error("Gagal mengubah status aktif", 500));

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat toggle active pada ScheduleCourse ID {id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}