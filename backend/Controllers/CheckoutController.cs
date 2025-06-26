using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;

[ApiController]
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
    public async Task<IActionResult> AddToCheckout(int courseId, int userId)
    {
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

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserCheckout(int userId)
    {
        var items = await _checkoutRepository.GetUserCheckoutAsync(userId);
        var total = await _checkoutRepository.GetTotalPriceAsync(userId);
        var result = new { items, total };
        return Ok(ApiResult<object>.SuccessResult(result, "Checkout retrieved successfully", 200));
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromCheckout(int userId, int courseId)
    {
        var removed = await _checkoutRepository.RemoveFromCheckoutAsync(userId, courseId);
        if (!removed)
            return NotFound(ApiResult<object>.Error("Course tidak ditemukan di checkout user", 404));
        return Ok(ApiResult<object>.SuccessResult("Course berhasil dihapus dari checkout", 200));
    }
}