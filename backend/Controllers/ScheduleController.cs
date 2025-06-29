using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DlanguageApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IScheduleRepository scheduleRepository, ILogger<ScheduleController> logger)
        {
            _scheduleRepository = scheduleRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Schedule>>> GetSchedule()
        {
            try
            {
                var schedules = await _scheduleRepository.GetAllScheduleAsync();
                return Ok(ApiResult<List<Schedule>>.SuccessResult(schedules, "Schedule  berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil data schedule ");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Schedule>> GetSchedule(int id)
        {
            try
            {
                var schedules = await _scheduleRepository.GetScheduleByIdAsync(id);
                if (schedules == null)
                    return NotFound(ApiResult<object>.Error($"Schedule  dengan ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<Schedule>.SuccessResult(schedules, "Schedule  berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil schedule  dengan ID {schedule_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpPost]
        public async Task<ActionResult<Schedule>> CreateSchedule([FromBody] ScheduleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));
                var newSchedule = new Schedule
                {
                    schedule_date = request.schedule_date,
                    created_at = DateTime.Now
                };
                var schedule_id = await _scheduleRepository.CreateScheduleAsync(newSchedule);
                newSchedule.schedule_id = schedule_id;
                return Ok(ApiResult<object>.SuccessResult(new { schedule_id, schedule_date = newSchedule.schedule_date }, "Add schedule  berhasil", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during add schedule .");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ScheduleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var existingSchedule = await _scheduleRepository.GetScheduleByIdAsync(id);
                if (existingSchedule == null)
                    return NotFound(ApiResult<object>.Error($"schedule  dengan ID {id} tidak ditemukan", 404));
                var updateData = new Schedule
                {
                    schedule_id = id,
                    schedule_date = request.schedule_date
                };
                var success = await _scheduleRepository.UpdateScheduleAsync(updateData);
                if (success)
                    return NoContent(); 

                return StatusCode(500, ApiResult<object>.Error("Gagal mengupdate schedule", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate schedule dengan ID {schedule_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var existingSchedule = await _scheduleRepository.GetScheduleByIdAsync(id);
                if (existingSchedule == null) 
                    return NotFound(ApiResult<object>.Error($"Schedule dengan ID {id} tidak ditemukan", 404));

                var success = await _scheduleRepository.DeleteScheduleAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal menghapus schedule ", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus schedule  dengan ID {schedule_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}