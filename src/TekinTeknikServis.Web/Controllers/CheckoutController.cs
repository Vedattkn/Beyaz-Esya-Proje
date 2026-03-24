using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TekinTeknikServis.Core.Infrastructure;
using TekinTeknikServis.Core.Models;

using TekinTeknikServis.Core.Filters;

namespace TekinTeknikServis.Core.Controllers
{
    [AuthCheck]
    public class CheckoutController : Controller
    {
        private const string CartSessionKey = "Cart";

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToRoute("sepet");

            ViewBag.TotalTry = cart.Sum(x => x.LineTotalTry);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Complete(string adSoyad, string kartNo, string sonKullanma, string cvc, string email, string sartlar)
        {
            if (string.IsNullOrWhiteSpace(adSoyad) ||
                string.IsNullOrWhiteSpace(kartNo) ||
                string.IsNullOrWhiteSpace(sonKullanma) ||
                string.IsNullOrWhiteSpace(cvc) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(sartlar))
            {
                return BadRequest("Lütfen tüm alanları doldurun.");
            }

            var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var total = cart.Sum(x => x.LineTotalTry);

            HttpContext.Session.SetJson(CartSessionKey, new List<CartItem>());
            ViewBag.Email = email;
            ViewBag.TotalTry = total;
            return View("Success");
        }
    }
}

