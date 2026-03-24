using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TekinTeknikServis.Web.Models;
using TekinTeknikServis.Web.Services;

namespace TekinTeknikServis.Web.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";

        // GET /sepet
        [Route("sepet")]
        public ActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalTry = cart.Sum(x => x.LineTotalTry);
            return View(cart);
        }

        // POST /sepet/ekle/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("sepet/ekle/{id}")]
        public ActionResult Add(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return HttpNotFound();
            if (!ProductCatalog.Products.TryGetValue(id, out var product)) return HttpNotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.Quantity += 1;
            }
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
            return RedirectToAction("Index");
        }

        // POST /sepet/sil/{index}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("sepet/sil/{index:int}")]
        public ActionResult RemoveAt(int index)
        {
            var cart = GetCart();
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // POST /sepet/guncelle/{index}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("sepet/guncelle/{index:int}")]
        public ActionResult UpdateQuantity(int index, int quantity)
        {
            var cart = GetCart();
            if (index >= 0 && index < cart.Count)
            {
                if (quantity > 0) cart[index].Quantity = quantity;
                else cart.RemoveAt(index);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            var cart = Session[CartSessionKey] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session[CartSessionKey] = cart;
            }
            return cart;
        }

        private void SaveCart(List<CartItem> cart)
        {
            Session[CartSessionKey] = cart ?? new List<CartItem>();
        }
    }
}

