using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;

namespace DlanguageApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoriesRepository categoriesRepository, ILogger<CategoriesController> logger)
        {
            _categoriesRepository = categoriesRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetCategories()
        {
            try
            {
                var categories = await _categoriesRepository.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil data kategori");
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            try
            {
                var category = await _categoriesRepository.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound($"Kategori dengan ID {id} tidak ditemukan");
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil kategori dengan ID {category_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category_id = await _categoriesRepository.CreateCategoryAsync(category);
                category.category_id = category_id;
                return CreatedAtAction(nameof(GetCategory), new { id = category_id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat kategori baru");
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                if (id != category.category_id)
                    return BadRequest("ID kategori tidak sesuai");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingCategory = await _categoriesRepository.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                    return NotFound($"Kategori dengan ID {id} tidak ditemukan");

                var success = await _categoriesRepository.UpdateCategoryAsync(category);
                if (success)
                    return NoContent();

                return StatusCode(500, "Gagal mengupdate kategori");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate kategori dengan ID {course_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var existingCategory = await _categoriesRepository.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                    return NotFound($"Kategori dengan ID {id} tidak ditemukan");

                var success = await _categoriesRepository.DeleteCategoryAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, "Gagal menghapus kategori");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus kategori dengan ID {category_id}", id);
                return StatusCode(500, "Terjadi kesalahan server");
            }
        }
    }
}