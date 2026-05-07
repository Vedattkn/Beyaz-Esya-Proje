using Microsoft.AspNetCore.Mvc;
using TekinTeknikServis.Core.Services;

namespace TekinTeknikServis.Core.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly SupabaseService _supabase;

        public ProductsApiController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _supabase.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("Ürün id boş olamaz.");

            var product = await _supabase.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (product == null) return BadRequest("Ürün verisi zorunludur.");
            if (string.IsNullOrWhiteSpace(product.Id)) return BadRequest("Ürün id zorunludur.");
            if (string.IsNullOrWhiteSpace(product.Name)) return BadRequest("Ürün adı zorunludur.");
            if (string.IsNullOrWhiteSpace(product.Category)) return BadRequest("Kategori zorunludur.");

            var existing = await _supabase.GetProductByIdAsync(product.Id);
            if (existing != null) return Conflict("Aynı id ile ürün zaten mevcut.");

            await _supabase.InsertProductAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Product product)
        {
            if (product == null) return BadRequest("Ürün verisi zorunludur.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("Ürün id zorunludur.");
            if (!string.Equals(id, product.Id, StringComparison.OrdinalIgnoreCase))
                return BadRequest("URL id ile payload id aynı olmalıdır.");

            await _supabase.UpdateProductAsync(product);
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("Ürün id boş olamaz.");

            await _supabase.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
