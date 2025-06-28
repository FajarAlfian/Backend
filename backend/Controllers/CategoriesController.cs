using Microsoft.AspNetCore.Mvc;
using DlanguageApi.Data;
using DlanguageApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DlanguageApi.Controllers
{
    [Authorize]
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
                return Ok(ApiResult<List<Category>>.SuccessResult(categories, "Kategori berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil data kategori");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            try
            {
                var category = await _categoriesRepository.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResult<object>.Error($"Kategori dengan ID {id} tidak ditemukan", 404));
                return Ok(ApiResult<Category>.SuccessResult(category, "Kategori berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil kategori dengan ID {category_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
        [HttpGet("category/{nama}")]
        public async Task<ActionResult<Category>> GetCategoryByName(string nama)
        {
            try
            {
                var category = await _categoriesRepository.GetCategoryByNameAsync(nama);
                if (category == null)
                    return NotFound(ApiResult<object>.Error($"Kategori dengan nama {nama} tidak ditemukan", 404));
                return Ok(ApiResult<Category>.SuccessResult(category, "Kategori berhasil diambil", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil kategori dengan nama {category_name}", nama);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));
                var newCategory = new Category
                {
                    category_name = request.category_name,
                    category_description = request.category_description,
                    category_image = request.category_image,
                    created_at = DateTime.Now
                };
                var category_id = await _categoriesRepository.CreateCategoryAsync(newCategory);
                newCategory.category_id = category_id;
                return Ok(ApiResult<object>.SuccessResult(new { category_id, category_name = newCategory.category_name, category_description = newCategory.category_description, category_image = newCategory.category_image }, "Add Category berhasil", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration.");
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequest request)
        {
            try
            {
                // if (id != category.category_id)
                //     return BadRequest(ApiResult<object>.Error("ID kategori tidak sesuai", 400));
                if (!ModelState.IsValid)
                    return BadRequest(ApiResult<object>.Error(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(), 400));

                var existingCategory = await _categoriesRepository.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                    return NotFound(ApiResult<object>.Error($"Kategori dengan ID {id} tidak ditemukan", 404));
                var updateData = new Category
                {
                    category_id = id,
                    category_name = request.category_name,
                    category_description = request.category_description,
                    category_image = request.category_name
                };
                var success = await _categoriesRepository.UpdateCategoryAsync(updateData);
                if (success)
                    return NoContent(); 

                return StatusCode(500, ApiResult<object>.Error("Gagal mengupdate kategori", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate kategori dengan ID {course_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var existingCategory = await _categoriesRepository.GetCategoryByIdAsync(id);
                if (existingCategory == null) 
                    return NotFound(ApiResult<object>.Error($"Kategori dengan ID {id} tidak ditemukan", 404));

                var success = await _categoriesRepository.DeleteCategoryAsync(id);
                if (success)
                    return NoContent();

                return StatusCode(500, ApiResult<object>.Error("Gagal menghapus kategori", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus kategori dengan ID {category_id}", id);
                return StatusCode(500, ApiResult<object>.Error("Terjadi kesalahan server", 500));
            }
        }
    }
}