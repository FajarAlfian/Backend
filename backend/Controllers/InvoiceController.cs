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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Invoice>>> GetInvoicesByUserId(int userId)
        {
            try
            {
                var invoices = await _invoiceRepository.GetInvoiceByUserIdAsync(userId);
                if (invoices == null)
                return NotFound(ApiResult<object>.Error($"Tidak ada invoice ditemukan untuk user dengan ID {userId}", 404));
                return Ok(ApiResult<List<Invoice>>.SuccessResult(invoices, "Invoice berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil invoice untuk user dengan ID {user_id}", userId);
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
        public async Task<ActionResult<Invoice>> CreateInvoice(
            [FromQuery] int user_id,
            [FromQuery] int payment_method_id,
            [FromQuery] string payment_method_name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

            try
            {
                var cartItems = await _checkoutRepository.GetUserCheckoutAsync(user_id);
                if (cartItems.Count == 0)
                    return BadRequest(ApiResult<object>.Error("Cart kosong, tidak bisa membuat invoice.", 400));

                var totalPrice = cartItems.Sum(x => x.course_price);

                int lastNumber = await _invoiceRepository.GetLastInvoiceNumberAsync();
                string newInvoiceNumber = $"DLA{(lastNumber + 1).ToString("D5")}";

                var invoice = new Invoice
                {
                    invoice_number = newInvoiceNumber,
                    user_id = user_id,
                    total_price = totalPrice,
                    payment_method_id = payment_method_id,
                    payment_method_name = payment_method_name,
                    isPaid = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };
                var invoice_id = await _invoiceRepository.CreateInvoiceAsync(invoice);
                invoice.invoice_id = invoice_id;

                foreach (var item in cartItems)
                {
                    await _invoiceRepository.CreateInvoiceDetailAsync(
                        invoice_id,          
                        item.cart_product_id,   
                        item.course_id,          
                        item.course_price        
                    );
                }

                //await _checkoutRepository.ClearUserCartAsync(user_id);

                return CreatedAtAction(nameof(GetInvoice), new { id = invoice_id }, ApiResult<Invoice>.SuccessResult(invoice, "Invoice berhasil dibuat", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat invoice baru");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
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
