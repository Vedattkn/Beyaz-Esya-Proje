using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using TekinTeknikServis.Core.Models;

namespace TekinTeknikServis.Core.Services
{
    public class SupabaseService
    {
        private readonly HttpClient _http;
        private readonly string _restUrl;
        private readonly string _apiKey;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public SupabaseService(HttpClient http, IConfiguration config)
        {
            _http = http;
            var baseUrl = (config["Supabase:Url"] ?? "").Trim().TrimEnd('/');
            _restUrl = string.IsNullOrWhiteSpace(baseUrl) ? "" : baseUrl + "/rest/v1";
            _apiKey = (config["Supabase:Key"] ?? "").Trim();
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrEmpty(_restUrl)) throw new InvalidOperationException("Supabase:Url ayarlı değil.");
            if (string.IsNullOrEmpty(_apiKey)) throw new InvalidOperationException("Supabase:Key ayarlı değil.");
        }

        private void SetHeaders()
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("apikey", _apiKey);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        private string JsonString(string? input)
        {
            if (input == null) return "null";
            return "\"" + HttpUtility.JavaScriptStringEncode(input) + "\"";
        }

        // ─── Servis Talepleri ──────────────────────────────────────────
        public async Task InsertServisTalebiAsync(ServiceRequestForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            EnsureConfigured();
            SetHeaders();

            var endpoint = _restUrl + "/servis_talepleri";

            // Yeni snake_case şemasına tam uyumlu payload
            var payload = new
            {
                kullanici_id = form.KullaniciId,
                ad_soyad = form.AdSoyad,
                telefon = form.Telefon,
                cihaz_turu = form.CihazTuru,
                ariza_aciklamasi = form.ArizaAciklamasi,
                durum = form.Durum ?? "Bekliyor",
                admin_cevabi = form.AdminCevabi ?? "",
                kullanici_cevabi = form.KullaniciCevabi ?? ""
            };

            var json = JsonSerializer.Serialize(new[] { payload });

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.TryAddWithoutValidation("Prefer", "return=minimal");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Supabase talep ekleme başarısız: " + body);
            }
        }

        public async Task<List<string>> GetDeviceTypesAsync()
        {
            EnsureConfigured();
            SetHeaders();
            var endpoint = _restUrl + "/urunler?select=name&order=name.asc";

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return new List<string> { "Hizmet Alınan Cihaz" };

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(body);
            var list = new List<string>();
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("name", out var nameProp)) 
                {
                    var name = nameProp.GetString();
                    if (!string.IsNullOrEmpty(name)) list.Add(name);
                }
            }
            return list.Distinct().ToList();
        }

        public async Task<List<ServiceRequestForm>> GetAllServiceRequestsAsync()
        {
            EnsureConfigured();
            SetHeaders();
            var endpoint = _restUrl + "/servis_talepleri?order=id.desc";

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return new List<ServiceRequestForm>();

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            // PropertyNameCaseInsensitive sayesinde ServisTalebi modelindeki PascalCase propertyler ile snake_case JSON eşleşir
            return JsonSerializer.Deserialize<List<ServiceRequestForm>>(body, _jsonOptions) ?? new List<ServiceRequestForm>();
        }

        public async Task<List<ServiceRequestForm>> GetUserRequestsByUidAsync(string uid)
        {
            EnsureConfigured();
            SetHeaders();
            var endpoint = _restUrl + "/servis_talepleri?kullanici_id=eq." + Uri.EscapeDataString(uid) + "&order=id.desc";

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return new List<ServiceRequestForm>();

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<ServiceRequestForm>>(body, _jsonOptions) ?? new List<ServiceRequestForm>();
        }

        public async Task UpdateUserReplyAsync(long id, string kullaniciCevabi)
        {
            EnsureConfigured();
            SetHeaders();
            var endpoint = _restUrl + "/servis_talepleri?id=eq." + id;

            var payload = new { kullanici_cevabi = kullaniciCevabi };
            var json = JsonSerializer.Serialize(payload);
            
            using var req = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            await _http.SendAsync(req).ConfigureAwait(false);
        }

        public async Task UpdateAdminReplyAsync(long id, string adminCevabi, string durum)
        {
            EnsureConfigured();
            SetHeaders();
            var endpoint = _restUrl + "/servis_talepleri?id=eq." + id;

            var payload = new { admin_cevabi = adminCevabi, durum = durum };
            var json = JsonSerializer.Serialize(payload);
            
            Console.WriteLine("Supabase PATCH (ADMIN): " + endpoint + " payload: " + json);

            using var req = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine("Supabase FAIL: " + body);
                throw new InvalidOperationException("Supabase güncelleme başarısız: " + body);
            }
            Console.WriteLine("Supabase OK");
        }

        // ─── Ürün Yönetimi ───────────────────────────────────────────
        public async Task InsertProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            EnsureConfigured();
            SetHeaders();

            var endpoint = _restUrl + "/urunler";

            var payload = new
            {
                id = product.Id,
                name = product.Name,
                description = product.Description,
                price_text = product.PriceText,
                image_url = product.ImageUrl,
                features = product.Features ?? new List<string>()
            };

            var json = JsonSerializer.Serialize(new[] { payload });

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.TryAddWithoutValidation("Prefer", "return=minimal");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Supabase ürün ekleme başarısız: " + body);
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            EnsureConfigured();
            SetHeaders();

            var endpoint = _restUrl + "/urunler?id=eq." + Uri.EscapeDataString(product.Id);

            var payload = new
            {
                name = product.Name,
                description = product.Description,
                price_text = product.PriceText,
                image_url = product.ImageUrl,
                features = product.Features ?? new List<string>()
            };

            var json = JsonSerializer.Serialize(payload);

            using var req = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Supabase ürün güncelleme başarısız: " + body);
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            EnsureConfigured();
            SetHeaders();
            var endpoint = _restUrl + "/urunler?order=name.asc";

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return new List<Product>();

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<Product>>(body, _jsonOptions) ?? new List<Product>();
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            EnsureConfigured();
            SetHeaders();
            var endpoint = _restUrl + "/urunler?id=eq." + Uri.EscapeDataString(id);

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return null;

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var list = JsonSerializer.Deserialize<List<Product>>(body, _jsonOptions);
            return (list != null && list.Count > 0) ? list[0] : null;
        }

        public async Task DeleteProductAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            EnsureConfigured();
            SetHeaders();

            var endpoint = _restUrl + "/urunler?id=eq." + Uri.EscapeDataString(id);

            using var req = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Supabase ürün silme başarısız: " + body);
            }
        }

        public async Task DeleteProductsAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any()) return;
            EnsureConfigured();
            SetHeaders();

            // id=in.(val1,val2,...) formatı Supabase toplu silme için kullanılır.
            var idList = string.Join(",", ids.Select(i => Uri.EscapeDataString(i)));
            var endpoint = _restUrl + "/urunler?id=in.(" + idList + ")";

            using var req = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Supabase toplu ürün silme başarısız: " + body);
            }
        }

        // ─── Kullanıcı Giriş / Kayıt ──────────────────────────────────
        public async Task<UserSession?> LoginAsync(string email, string password)
        {
            EnsureConfigured();
            SetHeaders();
            
            // Güvenli ve hızlı sorgulama: Sadece eşleşen kullanıcıyı getirir
            var endpoint = _restUrl + $"/kullanicilar?select=*&email=eq.{Uri.EscapeDataString(email)}&sifre=eq.{Uri.EscapeDataString(password)}&limit=1";

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return null;

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(body);
            
            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0) return null;

            var user = doc.RootElement[0];
            
            return new UserSession
            {
                Id = user.GetProperty("id").GetInt64().ToString(),
                AdSoyad = user.GetProperty("ad_soyad").GetString() ?? "",
                Email = user.GetProperty("email").GetString() ?? "",
                IsAdmin = user.TryGetProperty("is_admin", out var isAdminProp) && isAdminProp.GetBoolean(),
                GirisTarihi = DateTime.Now
            };
        }

        public async Task<bool> RegisterUserAsync(RegisterViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            EnsureConfigured();
            SetHeaders();

            // 1. E-posta kontrolü (Log ekledik)
            var checkUrl = _restUrl + $"/kullanicilar?select=email&email=eq.{Uri.EscapeDataString(model.Email)}&limit=1";
            Console.WriteLine("Kayıt Kontrol ediliyor: " + model.Email);
            
            using var checkReq = new HttpRequestMessage(HttpMethod.Get, checkUrl);
            var checkResp = await _http.SendAsync(checkReq).ConfigureAwait(false);
            var checkBody = await checkResp.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            Console.WriteLine("Kayıt Kontrol Yanıtı (" + (int)checkResp.StatusCode + "): " + checkBody);

            if (checkResp.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(checkBody);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    Console.WriteLine("!!! HATA: Bu e-posta zaten veritabanında mevcut.");
                    return false;
                }
            }

            // 2. Kayıt İşlemi
            var endpoint = _restUrl + "/kullanicilar";
            var payload = new 
            { 
                ad_soyad = model.AdSoyad, 
                email = model.Email, 
                sifre = model.Sifre,
                is_admin = false
            };
            var json = JsonSerializer.Serialize(new[] { payload });

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.TryAddWithoutValidation("Prefer", "return=minimal");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine("Kayıt BAŞARISIZ (Supabase): " + errorBody);
                return false;
            }

            Console.WriteLine("Kayıt BAŞARILI: " + model.Email);
            return true;
        }
    }
}
