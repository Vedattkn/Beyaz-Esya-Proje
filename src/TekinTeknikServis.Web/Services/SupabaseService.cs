using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TekinTeknikServis.Web.Models;

namespace TekinTeknikServis.Web.Services
{
    // Supabase (PostgREST) insert örneği.
    // MVC5/.NET Framework tarafında NuGet kurulumuna girmeden HTTP ile ilerliyoruz.
    public class SupabaseService
    {
        private readonly HttpClient _http;
        private readonly string _restUrl;
        private readonly string _apiKey;

        public SupabaseService()
        {
            _http = new HttpClient();

            // Web.config appSettings:
            // Supabase:Url = https://xxx.supabase.co
            // Supabase:Key = <anon/service key>
            _restUrl = (ConfigurationManager.AppSettings["Supabase:Url"] ?? "").Trim().TrimEnd('/') + "/rest/v1";
            _apiKey = (ConfigurationManager.AppSettings["Supabase:Key"] ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                _http.DefaultRequestHeaders.Add("apikey", _apiKey);
            }
        }

        public async Task InsertServisTalebiAsync(ServiceRequestForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (string.IsNullOrWhiteSpace(_restUrl)) throw new InvalidOperationException("Supabase:Url ayarlı değil.");
            if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("Supabase:Key ayarlı değil.");

            var endpoint = _restUrl + "/servis_talepleri";

            // Node'daki alan adları: AdSoyad, Telefon, CihazTuru, ArizaAciklamasi
            // Burada JSON property'leri aynı tutuluyor.
            var json =
                "[{" +
                "\"AdSoyad\":" + JsonString(form.AdSoyad) + "," +
                "\"Telefon\":" + JsonString(form.Telefon) + "," +
                "\"CihazTuru\":" + JsonString(form.CihazTuru) + "," +
                "\"ArizaAciklamasi\":" + JsonString(form.ArizaAciklamasi) +
                "}]";

            using (var req = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                req.Headers.TryAddWithoutValidation("Prefer", "return=minimal");
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await _http.SendAsync(req).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new InvalidOperationException("Supabase insert başarısız: " + (int)resp.StatusCode + " " + resp.ReasonPhrase + "\n" + body);
                }
            }
        }

        private static string JsonString(string value)
        {
            if (value == null) return "null";
            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") + "\"";
        }
    }
}

