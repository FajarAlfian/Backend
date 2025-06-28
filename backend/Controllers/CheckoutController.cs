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
    public async Task<IActionResult> AddToCheckout([FromQuery] int courseId)
    { 
        try
        {
            // Ambil userId dari token
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<object>.Error("User ID tidak valid atau tidak ditemukan di token.", 401));
            }

            var course = await _coursesRepository.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound(ApiResult<object>.Error("Course not found", 404));

                
            if (await _checkoutRepository.IsCourseInCheckoutAsync(userId, courseId))
                return BadRequest(ApiResult<object>.Error("Course sudah ada di checkout", 400));

            var checkout = new Checkout
            {
                course_id = course.course_id,
                course_name = course.course_name,
                course_price = course.course_price,
                user_id = userId
            };

            await _checkoutRepository.AddToCheckoutAsync(checkout);
            return Ok(ApiResult<object>.SuccessResult(checkout, "Course added to checkout", 200));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server saat menambahkan course ke checkout", 500));
        }
    }

    [HttpGet("user/{userId}")]
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
        catch (Exception)
        {
            return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server saat mengambil checkout", 500));
        }
    }

    [HttpDelete("remove/{courseId}")]
    public async Task<IActionResult> RemoveFromCheckout(int courseId)
    {
        try
        {
            // Ambil userId dari token
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<object>.Error("User ID tidak valid atau tidak ditemukan di token.", 401));
            }

            var removed = await _checkoutRepository.RemoveFromCheckoutAsync(userId, courseId);
            if (!removed)
                return NotFound(ApiResult<object>.Error("Course tidak ditemukan di checkout user", 404));
            return Ok(ApiResult<object>.SuccessResult("Course berhasil dihapus dari checkout", 200));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server saat menghapus course dari checkout", 500));
        }
    }
}