using System.Web.Mvc;

namespace TekinTeknikServis.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET /
        public ActionResult Index() => View();

        // GET /hizmetler
        public ActionResult Hizmetler() => View();

        // GET /urunler
        public ActionResult Urunler() => View();

        // GET /iletisim
        public ActionResult Iletisim() => View();
    }
}

