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
        private readonly ILogger<InvoiceController> _logger;
        public InvoiceController(IInvoiceRepository invoiceRepository, ILogger<InvoiceController> logger)
        {
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<List<Invoice>>> GetInvoices()
        {   
            try
            {
                var invoices = await _invoiceRepository.GetAllInvoicesAsync();
                return Ok(ApiResult<List<Invoice>>.SuccessResult(invoices, "Daftar invoice berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil daftar invoice");
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
        public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] InvoiceCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));  
                var invoice = new Invoice
                {
                    user_id = request.user_id,
                    total_price = request.total_price,
                    payment_method_id = request.payment_method_id,
                    payment_method_name = request.payment_method_name,
                    is_paid = false,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                var invoice_id = await _invoiceRepository.CreateInvoiceAsync(invoice);
                invoice.invoice_id = invoice_id;
                return Ok(ApiResult<object>.SuccessResult(invoice, "Invoice berhasil dibuat", 201));
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
                    total_price = request.total_price,
                    payment_method_id = request.payment_method_id,
                    payment_method_name = request.payment_method_name,
                    is_paid = request.is_paid,
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
