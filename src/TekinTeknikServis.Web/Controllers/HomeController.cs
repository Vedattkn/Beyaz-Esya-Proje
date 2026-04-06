using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TekinTeknikServis.Core.Services;

namespace TekinTeknikServis.Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly SupabaseService _supabase;
        
        public HomeController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public IActionResult Index() => View();
        public IActionResult Hizmetler() => View();
        
        public async Task<IActionResult> Urunler(string search = "")
        {
            var products = await _supabase.GetAllProductsAsync();
            
            // Arama filtrelemesi
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                products = products
                    .Where(p =>
                        (p.Name ?? string.Empty).ToLower().Contains(searchLower) ||
                        (p.Category ?? string.Empty).ToLower().Contains(searchLower) ||
                        (p.Description ?? string.Empty).ToLower().Contains(searchLower))
                    .ToList();
            }
            
            ViewBag.SearchTerm = search;
            return View(products);
        }

        public IActionResult Iletisim() => View();

        [Route("/Home/Error")]
        public IActionResult Error() => View();
    }
}
