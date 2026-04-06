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

            var model = new CheckoutViewModel
            {
                CartItems = cart
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Complete(CheckoutViewModel model)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["CheckoutError"] = "Sepetiniz boş olduğu için ödeme alınamadı.";
                return RedirectToRoute("sepet");
            }

            model.CartItems = cart;

            // Temel normalize işlemleri; validasyon bu formatlar üzerinde çalışır.
            model.KartNo = new string((model.KartNo ?? string.Empty).Where(char.IsDigit).ToArray());
            model.Cvc = new string((model.Cvc ?? string.Empty).Where(char.IsDigit).ToArray());
            model.SonKullanma = (model.SonKullanma ?? string.Empty).Trim();

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var total = model.TotalTry;

            HttpContext.Session.SetJson(CartSessionKey, new List<CartItem>());
            ViewBag.Email = model.Email;
            ViewBag.TotalTry = total;
            ViewBag.OrderNo = $"TTS-{DateTime.Now:yyyyMMddHHmmss}";
            return View("Success");
        }
    }
}

