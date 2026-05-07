using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TekinTeknikServis.Core.Data;
using TekinTeknikServis.Core.Infrastructure;
using TekinTeknikServis.Core.Models;

using TekinTeknikServis.Core.Filters;

namespace TekinTeknikServis.Core.Controllers
{
    [AuthCheck]
    public class CheckoutController : Controller
    {
        private const string CartSessionKey = "Cart";
        private readonly AppDbContext _db;

        public CheckoutController(AppDbContext db)
        {
            _db = db;
        }

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
        public async Task<IActionResult> Complete(CheckoutViewModel model)
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
            var orderNo = $"TTS-{DateTime.Now:yyyyMMddHHmmss}";

            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderNo = orderNo,
                Email = model.Email,
                FullName = model.AdSoyad,
                TotalTry = total,
                CreatedAt = DateTimeOffset.UtcNow
            };

            order.Items = cart.Select(item => new OrderItemEntity
            {
                OrderId = order.Id,
                ProductId = item.Id,
                ProductName = item.Name,
                PriceText = item.PriceText,
                UnitPriceTry = item.UnitPriceTry,
                Quantity = item.Quantity,
                LineTotalTry = item.LineTotalTry
            }).ToList();

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetJson(CartSessionKey, new List<CartItem>());
            ViewBag.Email = model.Email;
            ViewBag.TotalTry = total;
            ViewBag.OrderNo = orderNo;
            return View("Success");
        }
    }
}

