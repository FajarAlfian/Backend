using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutRepository _checkoutRepository;
    private readonly ICoursesRepository _coursesRepository;

    public CheckoutController(ICheckoutRepository checkoutRepository, ICoursesRepository coursesRepository)
    {
        _checkoutRepository = checkoutRepository;
        _coursesRepository = coursesRepository;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCheckout([FromQuery] int scheduleCourseId)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<object>.Error("User ID tidak valid atau tidak ditemukan di token.", 401));
            }

            // Ambil data schedule_course
            var scheduleCourseRepo = HttpContext.RequestServices.GetService(typeof(IScheduleCourseRepository)) as IScheduleCourseRepository;
            var scheduleCourse = await scheduleCourseRepo.GetScheduleCourseByIdAsync(scheduleCourseId);
            if (scheduleCourse == null)
                return NotFound(ApiResult<object>.Error("Schedule course tidak ditemukan", 404));

            // Ambil data course
            var course = await _coursesRepository.GetCourseByIdAsync(scheduleCourse.course_id);
            if (course == null)
                return NotFound(ApiResult<object>.Error("Course tidak ditemukan", 404));

            // Cek apakah sudah ada di checkout (berdasarkan schedule_course_id)
            if (await _checkoutRepository.IsScheduleCourseInCheckoutAsync(userId, scheduleCourseId))
                return BadRequest(ApiResult<object>.Error("Schedule course sudah ada di checkout", 400));

            var checkout = new Checkout
            {
                course_id = course.course_id,
                course_name = course.course_name,
                course_price = course.course_price,
                user_id = userId,
                schedule_course_id = scheduleCourseId,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            await _checkoutRepository.AddToCheckoutAsync(checkout);
            return Ok(ApiResult<object>.SuccessResult(checkout, "Schedule course added to checkout", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<object>.Error(
                $"Terjadi kesalahan server: {ex.Message}", 500));
        }
    }
    
    [HttpGet("user")]
    public async Task<IActionResult> GetUserCheckout()
    { 
        try
        {
            // Ambil userId dari token
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<object>.Error("User ID tidak valid atau tidak ditemukan di token.", 401));
            }

            var items = await _checkoutRepository.GetUserCheckoutAsync(userId);
            var total = await _checkoutRepository.GetTotalPriceAsync(userId);
            var result = new { items, total };
            return Ok(ApiResult<object>.SuccessResult(result, "Checkout retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<object>.Error(
                $"Terjadi kesalahan server: {ex.Message}", 500));
        }
    }
 [HttpDelete("remove/{cartProductId}")]
    public async Task<IActionResult> RemoveFromCheckout(int cartProductId)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<object>.Error(
                    "User ID tidak valid atau tidak ditemukan di token.", 401));
            }

            var removed = await _checkoutRepository
                .RemoveFromCheckoutAsync(userId, cartProductId);
            if (!removed)
                return NotFound(ApiResult<object>.Error(
                    "Item tidak ditemukan di checkout user", 404));

            var items = await _checkoutRepository.GetUserCheckoutAsync(userId);
            var total = items.Sum(x => x.course_price);

            var result = new
            {
                items,
                total
            };

            return Ok(ApiResult<object>.SuccessResult(
                result,
                "Item berhasil dihapus dari checkout",
                200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<object>.Error(
                $"Terjadi kesalahan server: {ex.Message}", 500));
        }
    }
}