using System.Linq;
using System.Web.Mvc;
using TekinTeknikServis.Web.Models;

namespace TekinTeknikServis.Web.Controllers
{
    public class CheckoutController : Controller
    {
        // GET /odeme
        [Route("odeme")]
        public ActionResult Index()
        {
            var cart = (Session["Cart"] as System.Collections.Generic.List<CartItem>) ?? new System.Collections.Generic.List<CartItem>();
            if (!cart.Any()) return RedirectToAction("Index", "Cart");

            ViewBag.TotalTry = cart.Sum(x => x.LineTotalTry);
            return View(cart);
        }

        // POST /odeme/tamamla
        [HttpPost]
        [Route("odeme/tamamla")]
        [ValidateAntiForgeryToken]
        public ActionResult Complete(string adSoyad, string kartNo, string sonKullanma, string cvc, string email, string sartlar)
        {
            if (string.IsNullOrWhiteSpace(adSoyad) ||
                string.IsNullOrWhiteSpace(kartNo) ||
                string.IsNullOrWhiteSpace(sonKullanma) ||
                string.IsNullOrWhiteSpace(cvc) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(sartlar))
            {
                return new HttpStatusCodeResult(400, "Lütfen tüm alanları doldurun.");
            }

            // Demo: Node'da olduğu gibi ödeme entegrasyonu yok, başarılı varsayıyoruz.
            var cart = (Session["Cart"] as System.Collections.Generic.List<CartItem>) ?? new System.Collections.Generic.List<CartItem>();
            var total = cart.Sum(x => x.LineTotalTry);

            Session["Cart"] = new System.Collections.Generic.List<CartItem>(); // sepeti temizle
            ViewBag.Email = email;
            ViewBag.TotalTry = total;
            return View("Success");
        }
    }
}

