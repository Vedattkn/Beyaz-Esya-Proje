using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TekinTeknikServis.Core.Models;
using TekinTeknikServis.Core.Services;

using TekinTeknikServis.Core.Filters;
using TekinTeknikServis.Core.Infrastructure;

namespace TekinTeknikServis.Core.Controllers
{
    [AuthCheck]
    public class ServiceRequestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public ServiceRequestController(IHttpClientFactory httpClientFactory, IConfiguration config, EmailService emailService)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool? success = null)
        {
            ViewBag.Success = success == true;
            
            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            ViewBag.DeviceTypes = await supabase.GetDeviceTypesAsync();

            return View(new ServiceRequestForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ServiceRequestForm form)
        {
            if (!ModelState.IsValid) return View(form);

            var userSession = HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (userSession != null && long.TryParse(userSession.Id, out var uid))
            {
                form.KullaniciId = uid;
            }

            if (!_emailService.IsConfigured)
            {
                ModelState.AddModelError("", "E-posta ayarları yapılmamış. appsettings.json içinde Email:To, SmtpHost, SmtpUser, SmtpPassword alanlarını doldurun.");
                return View(form);
            }

            try
            {
                await _emailService.SendServisTalebiAsync(form);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "E-posta gönderilirken hata oluştu: " + ex.Message);
                return View(form);
            }

            try
            {
                var http = _httpClientFactory.CreateClient();
                var supabase = new SupabaseService(http, _config);
                await supabase.InsertServisTalebiAsync(form);
            }
            catch
            {
                // Supabase opsiyonel - e-posta gönderildiyse devam et
            }

            return RedirectToAction("Index", new { success = true });
        }

        [AuthCheck]
        public async Task<IActionResult> MyRequests()
        {
            var userSession = HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (userSession == null || string.IsNullOrWhiteSpace(userSession.Id)) 
                return RedirectToAction("Login", "Account");

            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            var reqs = await supabase.GetUserRequestsByUidAsync(userSession.Id);
            
            return View(reqs);
        }
        [AuthCheck]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReply(long id, string kullaniciCevabi)
        {
            var userSession = HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (userSession == null || string.IsNullOrWhiteSpace(userSession.Id)) 
                return RedirectToAction("Login", "Account");

            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            await supabase.UpdateUserReplyAsync(id, kullaniciCevabi);
            
            return RedirectToAction("MyRequests");
        }
    }
}

