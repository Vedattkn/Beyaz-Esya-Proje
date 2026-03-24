using Microsoft.AspNetCore.Mvc;
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
        
        public async Task<IActionResult> Urunler()
        {
            var products = await _supabase.GetAllProductsAsync();
            return View(products);
        }

        public IActionResult Iletisim() => View();

        [Route("/Home/Error")]
        public IActionResult Error() => View();
    }
}
