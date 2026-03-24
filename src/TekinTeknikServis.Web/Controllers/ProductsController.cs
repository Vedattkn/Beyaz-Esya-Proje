using System.Web.Mvc;
using TekinTeknikServis.Web.Services;

namespace TekinTeknikServis.Web.Controllers
{
    public class ProductsController : Controller
    {
        // GET /urun/{id}
        [Route("urun/{id}")]
        public ActionResult Detail(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return HttpNotFound();
            ProductCatalog.Products.TryGetValue(id, out var product);
            if (product == null) return HttpNotFound();
            return View(product);
        }
    }
}

