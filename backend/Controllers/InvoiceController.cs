using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DlanguageApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICheckoutRepository _checkoutRepository;
        private readonly ILogger<InvoiceController> _logger;
        public InvoiceController(IInvoiceRepository invoiceRepository, ICheckoutRepository checkoutRepository, ILogger<InvoiceController> logger)
        {
            _invoiceRepository = invoiceRepository;
            _checkoutRepository = checkoutRepository;
            _logger = logger;
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<Invoice>>> GetInvoicesByUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(ApiResult<object>.Error("User ID tidak valid atau tidak ditemukan di token.", 401));
                }
                var invoices = await _invoiceRepository.GetInvoiceByUserIdAsync(userId);
                if (invoices == null || invoices.Count == 0)
                    return NotFound(ApiResult<object>.Error($"Tidak ada invoice ditemukan untuk user dengan ID {userId}", 404));
                return Ok(ApiResult<List<Invoice>>.SuccessResult(invoices, "Invoice berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil invoice untuk user dengan ID {user_id}", "me");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
                if (invoice == null)
                    return NotFound(ApiResult<object>.Error($"Invoice dengan ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<Invoice>.SuccessResult(invoice, "Invoice berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil invoice dengan ID {invoice_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }   
        }   
        [HttpPost]
        public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] InvoiceCreateRequest req)
        {
            if (req == null || req.cart_product_ids == null || !req.cart_product_ids.Any())
                return BadRequest(ApiResult<object>.Error("Pilih minimal satu item untuk dibayar.", 400));

            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResult<object>.Error("User ID tidak valid atau tidak ditemukan di token.", 401));

            var cartItems = await _checkoutRepository.GetUserCheckoutAsync(userId);
            var selected = cartItems
                .Where(ci => req.cart_product_ids.Contains(ci.cart_product_id))
                .ToList();

            if (!selected.Any())
                return BadRequest(ApiResult<object>.Error("Tidak ada item yang cocok dengan pilihan Anda.", 400));

            var totalPrice = selected.Sum(ci => ci.course_price);

            var lastNum = await _invoiceRepository.GetLastInvoiceNumberAsync();
            var newInvoiceNumber = $"DLA{(lastNum + 1):D5}";

            var invoice = new Invoice {
                invoice_number    = newInvoiceNumber,
                user_id           = userId,
                total_price       = totalPrice,
                payment_method_id = req.payment_method_id,
                isPaid            = true,
                created_at        = DateTime.UtcNow,
                updated_at        = DateTime.UtcNow
            };
            var invoiceId = await _invoiceRepository.CreateInvoiceAsync(invoice);
            invoice.invoice_id = invoiceId;

            foreach (var item in selected)
            {
                await _invoiceRepository.CreateInvoiceDetailAsync(
                    invoiceId,
                    item.cart_product_id,
                    item.course_id,
                    item.course_price,
                    item.schedule_course_id);
                await _checkoutRepository.RemoveFromCheckoutAsync(userId, item.cart_product_id);
            }

            return CreatedAtAction(
                nameof(GetInvoice),
                new { id = invoiceId },
                ApiResult<Invoice>.SuccessResult(invoice, "Invoice berhasil dibuat", 201));
        }


                    
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));
                var existingInvoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
                if (existingInvoice == null)
                    return NotFound(ApiResult<object>.Error($"Invoice dengan ID {id} tidak ditemukan", 404));
                var updatedInvoice = new Invoice
                { 
                    invoice_id = id,
                    user_id = request.user_id,
                    total_price = await _invoiceRepository.GetTotalPriceAsync(request.user_id),
                    payment_method_id = request.payment_method_id,
                    payment_method_name = request.payment_method_name,
                    isPaid = request.isPaid,
                    updated_at = DateTime.Now
                };
                var success = await _invoiceRepository.UpdateInvoiceAsync(updatedInvoice);
                if (success)
                    return NoContent();
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan saat memperbarui invoice", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat memperbarui invoice dengan ID {invoice_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            try
            {
                var existingInvoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
                if (existingInvoice == null)   
                    return NotFound(ApiResult<object>.Error($"Invoice dengan ID {id} tidak ditemukan", 404));
                var success = await _invoiceRepository.DeleteInvoiceAsync(id);
                if (success)
                    return NoContent();
                return StatusCode(500, ApiResult<object>.Error("Gagal menghapus invoice", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus invoice dengan ID {invoice_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}
