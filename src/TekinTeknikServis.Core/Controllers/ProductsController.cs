using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TekinTeknikServis.Core.Services;

namespace TekinTeknikServis.Core.Controllers
{
    public class ProductsController : Controller
    {
        private readonly SupabaseService _supabase;
        
        public ProductsController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var product = await _supabase.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}

