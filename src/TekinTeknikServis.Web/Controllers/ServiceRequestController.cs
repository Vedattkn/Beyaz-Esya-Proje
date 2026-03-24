using System.Threading.Tasks;
using System.Web.Mvc;
using TekinTeknikServis.Web.Models;
using TekinTeknikServis.Web.Services;

namespace TekinTeknikServis.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        // GET /servis-talep
        [Route("servis-talep")]
        public ActionResult Index()
        {
            ViewBag.Success = Request.QueryString["success"] == "true";
            return View(new ServiceRequestForm());
        }

        // POST /servis-talep
        [HttpPost]
        [Route("servis-talep")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ServiceRequestForm form)
        {
            if (!ModelState.IsValid) return View(form);

            var supabase = new SupabaseService();
            await supabase.InsertServisTalebiAsync(form);

            return RedirectToAction("Index", new { success = true });
        }
    }
}

