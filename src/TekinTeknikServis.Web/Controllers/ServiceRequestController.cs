using System;
using System.Linq;
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
        public async Task<IActionResult> Index(bool? success = null, string? kategori = null, string? parcaId = null, string? cihazTuru = null)
        {
            ViewBag.Success = success == true;

            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            await PopulateCategorySelectionsAsync(supabase);

            var form = new ServiceRequestForm();
            var incomingCategory = NormalizeCategoryQuery(kategori, cihazTuru);
            if (!string.IsNullOrWhiteSpace(incomingCategory))
            {
                form.CihazTuru = incomingCategory;
            }
            form.SecilenParcaId = parcaId ?? string.Empty;

            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ServiceRequestForm form)
        {
            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            await PopulateCategorySelectionsAsync(supabase);

            Product? selectedPart = null;
            if (!string.IsNullOrWhiteSpace(form.SecilenParcaId))
            {
                selectedPart = await supabase.GetProductByIdAsync(form.SecilenParcaId);
                if (selectedPart == null)
                {
                    ModelState.AddModelError(nameof(form.SecilenParcaId), "Seçtiğiniz parça bulunamadı.");
                }
                else if (!string.IsNullOrWhiteSpace(form.CihazTuru) &&
                         !string.Equals(selectedPart.Category, form.CihazTuru, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(form.SecilenParcaId), "Seçilen parça, seçili kategoriye ait değil.");
                }
                else
                {
                    form.SecilenParcaAdi = selectedPart.Name;
                }
            }

            if (!ModelState.IsValid) return View(form);

            var userSession = HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (userSession != null && long.TryParse(userSession.Id, out var uid))
            {
                form.KullaniciId = uid;
            }

            if (!string.IsNullOrWhiteSpace(form.SecilenParcaAdi))
            {
                form.ArizaAciklamasi = form.ArizaAciklamasi.Trim() + Environment.NewLine + "Seçilen Parça: " + form.SecilenParcaAdi;
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
                await supabase.InsertServisTalebiAsync(form);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Talep kaydedilemedi: " + ex.Message);
                return View(form);
            }

            return RedirectToAction("Index", new { success = true });
        }

        [AuthCheck]
        public async Task<IActionResult> MyRequests(long? selectedId = null)
        {
            var userSession = HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (userSession == null || string.IsNullOrWhiteSpace(userSession.Id)) 
                return RedirectToAction("Login", "Account");

            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            var reqs = await supabase.GetUserRequestsByUidAsync(userSession.Id);

            if (selectedId.HasValue)
            {
                ViewBag.SelectedId = selectedId.Value;
            }
            
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
            try
            {
                await supabase.UpdateUserReplyAsync(id, userSession.Id, kullaniciCevabi);
                TempData["UserSuccess"] = "Cevabınız kaydedildi.";
            }
            catch (Exception ex)
            {
                TempData["UserError"] = "Cevap gönderilemedi: " + ex.Message;
            }
            
            return RedirectToAction("MyRequests", new { selectedId = id });
        }

        [AuthCheck]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseAsSolved(long id)
        {
            var userSession = HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (userSession == null || string.IsNullOrWhiteSpace(userSession.Id))
                return RedirectToAction("Login", "Account");

            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            try
            {
                await supabase.CloseRequestAsSolvedAsync(id, userSession.Id);
                TempData["UserSuccess"] = "Talep kapatıldı.";
            }
            catch (Exception ex)
            {
                TempData["UserError"] = "Talep kapatılamadı: " + ex.Message;
            }
            
            return RedirectToAction("MyRequests", new { selectedId = id });
        }

        [AuthCheck]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var userSession = HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (userSession == null || string.IsNullOrWhiteSpace(userSession.Id)) 
                return RedirectToAction("Login", "Account");

            var http = _httpClientFactory.CreateClient();
            var supabase = new SupabaseService(http, _config);
            await supabase.DeleteServisTalebiAsync(id);
            
            return RedirectToAction("MyRequests");
        }

        private async Task PopulateCategorySelectionsAsync(SupabaseService supabase)
        {
            var categories = await supabase.GetProductCategoriesAsync();
            var parts = await supabase.GetAllProductsAsync();

            ViewBag.Categories = categories;
            ViewBag.Parts = parts
                .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name))
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .ToList();
        }

        private static string NormalizeCategoryQuery(string? kategori, string? cihazTuru)
        {
            var raw = !string.IsNullOrWhiteSpace(kategori) ? kategori : cihazTuru;
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            var normalized = raw.Trim();
            if (normalized.EndsWith(" Servisi", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(0, normalized.Length - " Servisi".Length).Trim();
            }

            if (string.Equals(normalized, "Ankastra Fırın", StringComparison.OrdinalIgnoreCase))
                return "Ankastre Fırın";

            return normalized;
        }
    }
}

