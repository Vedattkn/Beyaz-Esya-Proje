using Microsoft.AspNetCore.Mvc;
using TekinTeknikServis.Core.Models;
using TekinTeknikServis.Core.Services;
using TekinTeknikServis.Core.Infrastructure;

namespace TekinTeknikServis.Core.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupabaseService _supabase;
        private readonly JwtTokenService _jwtTokenService;
        private const string SessionKey = "CurrentUser";
        private const string SessionJwtKey = "AuthToken";

        public AccountController(SupabaseService supabase, JwtTokenService jwtTokenService)
        {
            _supabase = supabase;
            _jwtTokenService = jwtTokenService;
        }

        // ─── Kayıt Sayfası (GET) ──────────────────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            var user = HttpContext.Session.GetJson<UserSession>(SessionKey);
            if (user != null) return RedirectToRoute("home");

            return View(new RegisterViewModel());
        }

        // ─── Kayıt İşlemi (POST) ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var result = await _supabase.RegisterUserAsync(model);
                if (!result)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
                return RedirectToRoute("giris");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kayıt sırasında bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        // ─── Giriş Sayfası (GET) ──────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            var user = HttpContext.Session.GetJson<UserSession>(SessionKey);
            if (user != null) return RedirectToRoute("home");

            return View(new LoginViewModel());
        }

        // ─── Giriş İşlemi (POST) ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var session = await _supabase.LoginAsync(model.Email, model.Sifre);
                if (session == null)
                {
                    ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                    return View(model);
                }

                // Session'a kullanıcı bilgilerini kaydet
                HttpContext.Session.SetJson(SessionKey, session);

                // Kullanıcı için JWT üret ve session/cookie üzerinde sakla
                var token = _jwtTokenService.GenerateToken(session);
                HttpContext.Session.SetString(SessionJwtKey, token);

                if (model.BeniHatirla)
                {
                    Response.Cookies.Append(SessionJwtKey, token, new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = HttpContext.Request.IsHttps,
                        Expires = DateTimeOffset.UtcNow.AddDays(14)
                    });
                }
                else
                {
                    Response.Cookies.Delete(SessionJwtKey);
                }

                TempData["SuccessMessage"] = "Hoş geldiniz, " + session.AdSoyad + "!";
                return RedirectToRoute("home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Giriş sırasında bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        // ─── Çıkış İşlemi ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(SessionJwtKey);
            HttpContext.Session.Remove(SessionKey);
            HttpContext.Session.Clear();
            Response.Cookies.Delete(SessionJwtKey);
            TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToRoute("home");
        }

        // ─── Profilim Sayfası ──────────────────────────────────────────
        [HttpGet]
        public IActionResult Profile()
        {
            var user = HttpContext.Session.GetJson<UserSession>(SessionKey);
            if (user == null) return RedirectToRoute("giris");

            return View(user);
        }
    }
}
