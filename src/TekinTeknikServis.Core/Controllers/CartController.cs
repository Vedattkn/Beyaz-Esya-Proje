using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TekinTeknikServis.Core.Infrastructure;
using TekinTeknikServis.Core.Models;
using TekinTeknikServis.Core.Services;

using TekinTeknikServis.Core.Filters;

namespace TekinTeknikServis.Core.Controllers
{
    [AuthCheck]
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";
        private readonly SupabaseService _supabase;
        
        public CartController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalTry = cart.Sum(x => x.LineTotalTry);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var product = await _supabase.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (existing != null) existing.Quantity += 1;
            else
            {
                cart.Add(new CartItem
                {
                    Id = product.Id,
                    Name = product.Name,
                    PriceText = product.PriceText,
                    Quantity = 1
                });
            }

            SaveCart(cart);
            return RedirectToRoute("sepet");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveAt(int index)
        {
            var cart = GetCart();
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int index, int quantity)
        {
            var cart = GetCart();
            if (index >= 0 && index < cart.Count)
            {
                if (quantity > 0) cart[index].Quantity = quantity;
                else cart.RemoveAt(index);
                SaveCart(cart);
            }
            return RedirectToRoute("sepet");
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetJson(CartSessionKey, cart ?? new List<CartItem>());
        }
    }
}

