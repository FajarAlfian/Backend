using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DlanguageApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly ILogger<PaymentMethodController> _logger;

        public PaymentMethodController(IPaymentMethodRepository paymentMethodRepository, ILogger<PaymentMethodController> logger)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PaymentMethod>>> GetPaymentMethod()
        {
            try
            {
                var paymentMethods = await _paymentMethodRepository.GetAllPaymentMethodAsync();
                return Ok(ApiResult<List<PaymentMethod>>.SuccessResult(paymentMethods, "Payment method berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil data payment method");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethod>> GetPaymentMethod(int id)
        {
            try
            {
                var paymentMethods = await _paymentMethodRepository.GetPaymentMethodByIdAsync(id);
                if (paymentMethods == null)
                    return NotFound(ApiResult<object>.Error($"Payment method dengan ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<PaymentMethod>.SuccessResult(paymentMethods, "Payment method berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil payment method dengan ID {payment_method_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<PaymentMethod>> CreatePaymentMethod([FromBody] PaymentMethodRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));
                var newPaymentMethod = new PaymentMethod
                {
                    payment_method_name = request.payment_method_name,
                    created_at = DateTime.Now
                };
                var payment_method_id = await _paymentMethodRepository.CreatePaymentMethodAsync(newPaymentMethod);
                newPaymentMethod.payment_method_id = payment_method_id;
                return Ok(ApiResult<object>.SuccessResult(new { payment_method_id, payment_method_name = newPaymentMethod.payment_method_name }, "Add payment method berhasil", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during add payment method.");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] PaymentMethodRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var existingPaymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(id);
                if (existingPaymentMethod == null)
                    return NotFound(ApiResult<object>.Error($"Payment method dengan ID {id} tidak ditemukan", 404));
                var updateData = new PaymentMethod
                {
                    payment_method_id = id,
                    payment_method_name = request.payment_method_name,
                };
                var success = await _paymentMethodRepository.UpdatePaymentMethodAsync(updateData);
                if (success)
                    return NoContent(); 

                return StatusCode(500, ApiResult<object>.Error("Gagal mengupdate payment method ", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate payment method  dengan ID {payment_method_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            try
            {
                var existingPaymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(id);
                if (existingPaymentMethod == null) 
                    return NotFound(ApiResult<object>.Error($"Payment method dengan ID {id} tidak ditemukan", 404));

                var success = await _paymentMethodRepository.DeletePaymentMethodAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal menghapus payment method", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus payment method dengan ID {payment_method_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}