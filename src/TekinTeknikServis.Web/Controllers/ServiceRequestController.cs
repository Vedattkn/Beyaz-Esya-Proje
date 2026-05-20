using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TekinTeknikServis.Core.Data;
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
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<ServiceRequestController> _logger;
        private readonly AppDbContext _db;

        public ServiceRequestController(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            EmailService emailService,
            IWhatsAppService whatsAppService,
            ILogger<ServiceRequestController> logger,
            AppDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _logger = logger;
            _db = db;
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
                if (string.IsNullOrWhiteSpace(form.CustomerEmail) && !string.IsNullOrWhiteSpace(userSession.Email))
                {
                    form.CustomerEmail = userSession.Email.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(form.SecilenParcaAdi))
            {
                form.ArizaAciklamasi = form.ArizaAciklamasi.Trim() + Environment.NewLine + "Seçilen Parça: " + form.SecilenParcaAdi;
            }

            var adresBlok = "Adres: " + form.Adres.Trim();
            form.ArizaAciklamasi = form.ArizaAciklamasi.Trim() + Environment.NewLine + adresBlok;
            form.Durum = ServiceRequestStatusHelper.Pending;

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

            var whatsappResult = await _whatsAppService.SendServiceRequestConfirmationAsync(form);
            if (!whatsappResult.IsSuccess)
            {
                _logger.LogWarning("WhatsApp Cloud API send failed: {Error}", whatsappResult.ErrorMessage);
            }

            return RedirectToAction("MyRequests");
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

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Approval(long id, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return NotFound();

            var request = await _db.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (request == null || !string.Equals(request.ApprovalToken ?? string.Empty, token, StringComparison.Ordinal))
            {
                ViewBag.Error = "Onay linki bulunamadı veya geçersiz.";
                return View(new ServiceRequestApprovalViewModel { Id = id, Token = token, CanRespond = false });
            }

            var canRespond = string.Equals(request.Durum ?? string.Empty, ServiceRequestStatusHelper.WaitingCustomerApproval, StringComparison.OrdinalIgnoreCase);
            var model = new ServiceRequestApprovalViewModel
            {
                Id = request.Id,
                Token = token,
                CustomerName = request.AdSoyad,
                DeviceType = request.CihazTuru,
                FaultyPart = request.FaultyPart,
                ReplacementPart = request.ReplacementPart,
                RepairDetails = request.RepairDetails,
                LaborPriceTry = request.LaborPriceTry,
                PartPriceTry = request.PartPriceTry,
                TotalPriceTry = request.TotalPriceTry,
                AdminNotes = request.AdminNotes,
                Status = request.Durum ?? ServiceRequestStatusHelper.Pending,
                ApprovalStatus = request.ApprovalStatus,
                ApprovalDate = request.ApprovalDate,
                CanRespond = canRespond
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> ApprovalDecision(long id, string token, string decision)
        {
            if (string.IsNullOrWhiteSpace(token)) return NotFound();

            var request = await _db.ServiceRequests
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (request == null || !string.Equals(request.ApprovalToken ?? string.Empty, token, StringComparison.Ordinal))
            {
                TempData["ApprovalMessage"] = "Onay linki bulunamadı veya geçersiz.";
                return RedirectToAction("Approval", new { id, token });
            }

            if (!string.Equals(request.Durum ?? string.Empty, ServiceRequestStatusHelper.WaitingCustomerApproval, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ApprovalMessage"] = "Bu talep için onay zaten işlendi.";
                return RedirectToAction("Approval", new { id, token });
            }

            var normalizedDecision = (decision ?? string.Empty).Trim().ToLowerInvariant();
            if (normalizedDecision != "approve" && normalizedDecision != "reject")
            {
                TempData["ApprovalMessage"] = "Geçersiz işlem.";
                return RedirectToAction("Approval", new { id, token });
            }

            var approved = normalizedDecision == "approve";
            request.Durum = approved ? ServiceRequestStatusHelper.Approved : ServiceRequestStatusHelper.Rejected;
            request.ApprovalStatus = approved ? ServiceRequestStatusHelper.Approved : ServiceRequestStatusHelper.Rejected;
            request.ApprovalDate = DateTime.UtcNow;

            await _db.SaveChangesAsync().ConfigureAwait(false);

            TempData["ApprovalMessage"] = approved
                ? "Onayınız alınmıştır. Teşekkürler."
                : "Talep reddedildi. Geri bildiriminiz kaydedildi.";

            return RedirectToAction("Approval", new { id, token });
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

