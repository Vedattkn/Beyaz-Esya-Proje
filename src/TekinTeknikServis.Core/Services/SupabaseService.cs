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
using Npgsql;
using TekinTeknikServis.Core.Models;

namespace TekinTeknikServis.Core.Services
{
    public class SupabaseService
    {
        private readonly HttpClient _http;
        private readonly string _restUrl;
        private readonly string _apiKey;
        private readonly string _dbConnectionString;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public SupabaseService(HttpClient http, IConfiguration config)
        {
            _http = http;
            var baseUrl = (config["Supabase:Url"] ?? "").Trim().TrimEnd('/');
            _restUrl = string.IsNullOrWhiteSpace(baseUrl) ? "" : baseUrl + "/rest/v1";
            _apiKey = (Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
                ?? config["Supabase:ServiceRoleKey"]
                ?? config["Supabase:Key"]
                ?? "").Trim();
            _dbConnectionString = (config.GetConnectionString("SupabaseDb") ?? config.GetConnectionString("DefaultConnection") ?? "").Trim();
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

        private async Task<List<string>> TryGetCategoriesFromCategoryTableAsync()
        {
            try
            {
                EnsureConfigured();
                SetHeaders();
                var endpoint = _restUrl + "/kategoriler?select=name&order=name.asc";

                using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
                var resp = await _http.SendAsync(req).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode) return new List<string>();

                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var doc = JsonDocument.Parse(body);
                var list = new List<string>();
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("name", out var nameProp))
                    {
                        var name = nameProp.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            list.Add(name.Trim());
                        }
                    }
                }

                return list;
            }
            catch
            {
                return new List<string>();
            }
        }

        private async Task<List<string>> TryGetCategoriesFromCategoryTableWithDbAsync()
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            var categories = new List<string>();
            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            const string sql = "select name from public.kategoriler order by name asc";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var name = reader.IsDBNull(0) ? null : reader.GetString(0);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    categories.Add(name.Trim());
                }
            }

            return categories
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        private static List<string> ParseFeaturesValue(object? value)
        {
            if (value == null || value == DBNull.Value) return new List<string>();

            try
            {
                if (value is string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return new List<string>();
                    var parsed = JsonSerializer.Deserialize<List<string>>(s);
                    return parsed ?? new List<string>();
                }

                var asString = Convert.ToString(value);
                if (string.IsNullOrWhiteSpace(asString)) return new List<string>();
                var parsedFromString = JsonSerializer.Deserialize<List<string>>(asString);
                return parsedFromString ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private async Task<string> ResolveUrunlerCategoryColumnAsync(NpgsqlConnection conn)
        {
            const string sql = @"select column_name
from information_schema.columns
where table_schema = 'public'
  and table_name = 'urunler'
  and column_name in ('category','kategori')
order by case when column_name = 'category' then 0 else 1 end
limit 1";

            await using var cmd = new NpgsqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            var value = result?.ToString()?.Trim().ToLowerInvariant();

            if (value == "category" || value == "kategori")
            {
                return value;
            }

            // Self-heal: eski şemalarda kategori kolonu yoksa otomatik oluştur.
            const string alterSql = "alter table public.urunler add column if not exists category text";
            await using (var alterCmd = new NpgsqlCommand(alterSql, conn))
            {
                await alterCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            await using var recheckCmd = new NpgsqlCommand(sql, conn);
            var recheck = await recheckCmd.ExecuteScalarAsync().ConfigureAwait(false);
            var recheckValue = recheck?.ToString()?.Trim().ToLowerInvariant();
            if (recheckValue == "category" || recheckValue == "kategori")
            {
                return recheckValue;
            }

            throw new InvalidOperationException("public.urunler tablosunda 'category' veya 'kategori' kolonu bulunamadı.");
        }

        private async Task<List<Product>> TryGetProductsFromDbAsync()
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            var list = new List<Product>();
            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

            var sql = $@"select id, name, description, price_text, image_url, {categoryColumn} as category, features
from public.urunler
order by name asc";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var product = new Product
                {
                    Id = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    PriceText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    ImageUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Category = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Features = ParseFeaturesValue(reader.IsDBNull(6) ? null : reader.GetValue(6))
                };

                list.Add(product);
            }

            return list;
        }

        private async Task<Product?> TryGetProductByIdFromDbAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString) || string.IsNullOrWhiteSpace(id)) return null;

            try
            {
                await using var conn = new NpgsqlConnection(_dbConnectionString);
                await conn.OpenAsync().ConfigureAwait(false);

                var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

                var sql = $@"select id, name, description, price_text, image_url, {categoryColumn} as category, features
from public.urunler
where id = @id
limit 1";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", id);

                await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                if (!await reader.ReadAsync().ConfigureAwait(false)) return null;

                return new Product
                {
                    Id = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    PriceText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    ImageUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Category = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Features = ParseFeaturesValue(reader.IsDBNull(6) ? null : reader.GetValue(6))
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task EnsureCategoryExistsAsync(string? category)
        {
            var normalizedCategory = (category ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedCategory)) return;

            if (string.IsNullOrWhiteSpace(_dbConnectionString)) return;
            await CreateCategoryWithDbConnectionAsync(normalizedCategory).ConfigureAwait(false);
        }

        public async Task<bool> CreateCategoryIfNotExistsAsync(string? category)
        {
            var normalizedCategory = (category ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedCategory))
            {
                throw new ArgumentException("Kategori adı boş olamaz.", nameof(category));
            }

            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            return await CreateCategoryWithDbConnectionAsync(normalizedCategory).ConfigureAwait(false);
        }

        private async Task<bool> CreateCategoryWithDbConnectionAsync(string normalizedCategory)
        {
            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            return await CreateCategoryWithDbConnectionAsync(conn, normalizedCategory, null).ConfigureAwait(false);
        }

        private static async Task<bool> CreateCategoryWithDbConnectionAsync(NpgsqlConnection conn, string normalizedCategory, NpgsqlTransaction? tx)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));

            const string sql = @"
insert into public.kategoriler(name)
values (@name)
on conflict (name) do nothing
returning id;";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("name", normalizedCategory);

            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            return result != null && result != DBNull.Value;
        }

        private static List<ServiceRequestMessage> ParseStoredConversation(ServiceRequestForm request)
        {
            if (!string.IsNullOrWhiteSpace(request.AdminCevabi))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<ServiceRequestMessage>>(request.AdminCevabi);
                    if (parsed != null && parsed.Count > 0)
                    {
                        return parsed
                            .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                            .ToList();
                    }
                }
                catch
                {
                    // Legacy düz metin olabilir, aşağıda ele alınır.
                }
            }

            var legacy = new List<ServiceRequestMessage>();
            if (!string.IsNullOrWhiteSpace(request.AdminCevabi))
            {
                legacy.Add(new ServiceRequestMessage
                {
                    Role = "admin",
                    Text = request.AdminCevabi,
                    SentAt = request.KayitTarihi ?? DateTime.UtcNow
                });
            }

            if (!string.IsNullOrWhiteSpace(request.KullaniciCevabi))
            {
                legacy.Add(new ServiceRequestMessage
                {
                    Role = "user",
                    Text = request.KullaniciCevabi,
                    SentAt = DateTime.UtcNow
                });
            }

            return legacy;
        }

        private static List<ServiceRequestMessage> BuildConversationForDisplay(ServiceRequestForm request)
        {
            var messages = new List<ServiceRequestMessage>();

            if (!string.IsNullOrWhiteSpace(request.ArizaAciklamasi))
            {
                messages.Add(new ServiceRequestMessage
                {
                    Role = "user",
                    Text = request.ArizaAciklamasi,
                    SentAt = request.KayitTarihi ?? DateTime.UtcNow
                });
            }

            messages.AddRange(ParseStoredConversation(request));
            return messages
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .ToList();
        }

            private static bool IsClosedStatus(string? status)
            {
                if (string.IsNullOrWhiteSpace(status)) return false;

                var normalized = status.Trim();
                return string.Equals(normalized, "Kapatildi", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Cozuldu", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, "Kapali", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Kapalı", StringComparison.OrdinalIgnoreCase);
            }

        private async Task<ServiceRequestForm?> GetServiceRequestByIdAsync(long id, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            var parsedUserId = !string.IsNullOrWhiteSpace(userId) && long.TryParse(userId, out var uid)
                ? uid
                : (long?)null;
            return await TryGetServiceRequestByIdWithDbAsync(id, parsedUserId).ConfigureAwait(false);
        }

        private async Task<ServiceRequestForm?> TryGetServiceRequestByIdWithDbAsync(long id, long? userId = null)
        {
            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var sql = @"select id, kullanici_id, ad_soyad, telefon, cihaz_turu, ariza_aciklamasi, durum, admin_cevabi, kullanici_cevabi, kayit_tarihi
from public.servis_talepleri
where id = @id";

            if (userId.HasValue)
            {
                sql += " and kullanici_id = @kullanici_id";
            }

            sql += " limit 1";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            if (userId.HasValue)
            {
                cmd.Parameters.AddWithValue("kullanici_id", userId.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            if (!await reader.ReadAsync().ConfigureAwait(false)) return null;

            var request = new ServiceRequestForm
            {
                Id = reader.IsDBNull(0) ? null : Convert.ToInt64(reader.GetValue(0)),
                KullaniciId = reader.IsDBNull(1) ? null : Convert.ToInt64(reader.GetValue(1)),
                AdSoyad = reader.IsDBNull(2) ? string.Empty : reader.GetValue(2).ToString() ?? string.Empty,
                Telefon = reader.IsDBNull(3) ? string.Empty : reader.GetValue(3).ToString() ?? string.Empty,
                CihazTuru = reader.IsDBNull(4) ? string.Empty : reader.GetValue(4).ToString() ?? string.Empty,
                ArizaAciklamasi = reader.IsDBNull(5) ? string.Empty : reader.GetValue(5).ToString() ?? string.Empty,
                Durum = reader.IsDBNull(6) ? "Bekliyor" : reader.GetValue(6).ToString(),
                AdminCevabi = reader.IsDBNull(7) ? string.Empty : reader.GetValue(7).ToString(),
                KullaniciCevabi = reader.IsDBNull(8) ? string.Empty : reader.GetValue(8).ToString(),
                KayitTarihi = reader.IsDBNull(9) ? null : Convert.ToDateTime(reader.GetValue(9))
            };

            request.SohbetMesajlari = BuildConversationForDisplay(request);
            return request;
        }

        private async Task UpdateConversationAsync(long id, string role, string? message, string? durum, string? userId)
        {
            var request = await GetServiceRequestByIdAsync(id, userId).ConfigureAwait(false);
            if (request == null)
            {
                throw new InvalidOperationException("Talep bulunamadı veya güncelleme yetkiniz yok.");
            }

            if (IsClosedStatus(request.Durum))
            {
                throw new InvalidOperationException("Bu talep kapatıldı. Artık mesaj gönderilemez veya güncellenemez.");
            }

            var storedMessages = ParseStoredConversation(request);
            var normalizedMessage = (message ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedMessage))
            {
                storedMessages.Add(new ServiceRequestMessage
                {
                    Role = role,
                    Text = normalizedMessage,
                    SentAt = DateTime.UtcNow
                });
            }

            var parsedUserId = !string.IsNullOrWhiteSpace(userId) && long.TryParse(userId, out var uid)
                ? uid
                : (long?)null;

            await UpdateConversationWithDbAsync(
                id,
                parsedUserId,
                JsonSerializer.Serialize(storedMessages),
                role == "user" ? normalizedMessage : request.KullaniciCevabi,
                string.IsNullOrWhiteSpace(durum) ? (request.Durum ?? "Bekliyor") : durum
            ).ConfigureAwait(false);
        }

        private async Task UpdateConversationWithDbAsync(long id, long? userId, string adminCevabiJson, string? kullaniciCevabi, string? durum)
        {
            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var sql = @"update public.servis_talepleri
set admin_cevabi = @admin_cevabi,
    kullanici_cevabi = @kullanici_cevabi,
    durum = @durum
where id = @id";

            if (userId.HasValue)
            {
                sql += " and kullanici_id = @kullanici_id";
            }

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("admin_cevabi", adminCevabiJson ?? string.Empty);
            cmd.Parameters.AddWithValue("kullanici_cevabi", (object?)kullaniciCevabi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("durum", durum ?? "Bekliyor");
            cmd.Parameters.AddWithValue("id", id);
            if (userId.HasValue)
            {
                cmd.Parameters.AddWithValue("kullanici_id", userId.Value);
            }

            var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (rows <= 0)
            {
                throw new InvalidOperationException("Talep bulunamadı veya güncelleme yetkiniz yok.");
            }
        }

        private static bool IsBcryptHash(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$");
        }

        private static int? TryGetOrdinal(NpgsqlDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        private async Task<UserSession?> TryLoginWithDbAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString)) return null;

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            const string sql = "select * from public.kullanicilar where lower(email) = lower(@email) limit 1";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", email);

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            if (!await reader.ReadAsync().ConfigureAwait(false)) return null;

            var sifreOrd = TryGetOrdinal(reader, "sifre");
            var idOrd = TryGetOrdinal(reader, "id");
            var adOrd = TryGetOrdinal(reader, "ad_soyad");
            var emailOrd = TryGetOrdinal(reader, "email");
            var telOrd = TryGetOrdinal(reader, "telefon");
            var isAdminOrd = TryGetOrdinal(reader, "is_admin");
            var rolOrd = TryGetOrdinal(reader, "rol");

            var storedPassword = sifreOrd.HasValue && !reader.IsDBNull(sifreOrd.Value)
                ? reader.GetString(sifreOrd.Value)
                : string.Empty;

            var isValidPassword = IsBcryptHash(storedPassword)
                ? BCrypt.Net.BCrypt.Verify(password, storedPassword)
                : string.Equals(storedPassword, password, StringComparison.Ordinal);

            if (!isValidPassword) return null;

            var roleValue = rolOrd.HasValue && !reader.IsDBNull(rolOrd.Value)
                ? (reader.GetValue(rolOrd.Value)?.ToString() ?? string.Empty)
                : string.Empty;

            var isAdminFromBool = isAdminOrd.HasValue && !reader.IsDBNull(isAdminOrd.Value) && Convert.ToBoolean(reader.GetValue(isAdminOrd.Value));
            var isAdminFromRole = string.Equals(roleValue, "admin", StringComparison.OrdinalIgnoreCase);

            return new UserSession
            {
                Id = idOrd.HasValue && !reader.IsDBNull(idOrd.Value) ? reader.GetValue(idOrd.Value).ToString() ?? string.Empty : string.Empty,
                AdSoyad = adOrd.HasValue && !reader.IsDBNull(adOrd.Value) ? reader.GetString(adOrd.Value) : string.Empty,
                Email = emailOrd.HasValue && !reader.IsDBNull(emailOrd.Value) ? reader.GetString(emailOrd.Value) : email,
                Telefon = telOrd.HasValue && !reader.IsDBNull(telOrd.Value) ? reader.GetString(telOrd.Value) : string.Empty,
                IsAdmin = isAdminFromBool || isAdminFromRole,
                GirisTarihi = DateTime.Now
            };
        }

        private async Task TryMigratePasswordHashAsync(string userId, string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(plainPassword)) return;

            try
            {
                if (string.IsNullOrWhiteSpace(_dbConnectionString)) return;
                if (!long.TryParse(userId, out var parsedUserId)) return;

                await using var conn = new NpgsqlConnection(_dbConnectionString);
                await conn.OpenAsync().ConfigureAwait(false);

                const string sql = "update public.kullanicilar set sifre = @sifre where id = @id";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("sifre", BCrypt.Net.BCrypt.HashPassword(plainPassword));
                cmd.Parameters.AddWithValue("id", parsedUserId);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch
            {
                // Geçiş denemesi başarısız olursa login akışı etkilenmesin.
            }
        }

        // ─── Servis Talepleri ──────────────────────────────────────────
        public async Task InsertServisTalebiAsync(ServiceRequestForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await InsertServiceRequestWithDbAsync(form).ConfigureAwait(false);
        }

        private async Task InsertServiceRequestWithDbAsync(ServiceRequestForm form)
        {
            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            const string sql = @"insert into public.servis_talepleri
(kullanici_id, ad_soyad, telefon, cihaz_turu, ariza_aciklamasi, durum, admin_cevabi, kullanici_cevabi)
values
(@kullanici_id, @ad_soyad, @telefon, @cihaz_turu, @ariza_aciklamasi, @durum, @admin_cevabi, @kullanici_cevabi)";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("kullanici_id", (object?)form.KullaniciId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("ad_soyad", form.AdSoyad ?? string.Empty);
            cmd.Parameters.AddWithValue("telefon", form.Telefon ?? string.Empty);
            cmd.Parameters.AddWithValue("cihaz_turu", form.CihazTuru ?? string.Empty);
            cmd.Parameters.AddWithValue("ariza_aciklamasi", form.ArizaAciklamasi ?? string.Empty);
            cmd.Parameters.AddWithValue("durum", form.Durum ?? "Bekliyor");
            cmd.Parameters.AddWithValue("admin_cevabi", form.AdminCevabi ?? string.Empty);
            cmd.Parameters.AddWithValue("kullanici_cevabi", form.KullaniciCevabi ?? string.Empty);

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<List<string>> GetDeviceTypesAsync()
        {
            return await GetProductCategoriesAsync().ConfigureAwait(false);
        }

        public async Task<List<string>> GetProductCategoriesAsync()
        {
            var categoriesFromDbTable = await TryGetCategoriesFromCategoryTableWithDbAsync().ConfigureAwait(false);
            if (categoriesFromDbTable.Any())
            {
                return categoriesFromDbTable;
            }

            var categoriesFromDbProducts = (await TryGetProductsFromDbAsync().ConfigureAwait(false))
                .Where(x => !string.IsNullOrWhiteSpace(x.Category))
                .Select(x => x.Category.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            return categoriesFromDbProducts;
        }

        public async Task<List<ServiceRequestForm>> GetAllServiceRequestsAsync()
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            return await TryGetServiceRequestsFromDbAsync().ConfigureAwait(false);
        }

        public async Task<List<ServiceRequestForm>> GetUserRequestsByUidAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            if (!long.TryParse(uid, out var userId)) return new List<ServiceRequestForm>();
            return await TryGetServiceRequestsFromDbAsync(userId).ConfigureAwait(false);
        }

        private async Task<List<ServiceRequestForm>> TryGetServiceRequestsFromDbAsync(long? userId = null)
        {
            var list = new List<ServiceRequestForm>();

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var sql = @"select id, kullanici_id, ad_soyad, telefon, cihaz_turu, ariza_aciklamasi, durum, admin_cevabi, kullanici_cevabi, kayit_tarihi
from public.servis_talepleri";

            if (userId.HasValue)
            {
                sql += " where kullanici_id = @kullanici_id";
            }

            sql += " order by id desc";

            await using var cmd = new NpgsqlCommand(sql, conn);
            if (userId.HasValue)
            {
                cmd.Parameters.AddWithValue("kullanici_id", userId.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            var idOrd = TryGetOrdinal(reader, "id");
            var kullaniciIdOrd = TryGetOrdinal(reader, "kullanici_id");
            var adSoyadOrd = TryGetOrdinal(reader, "ad_soyad");
            var telefonOrd = TryGetOrdinal(reader, "telefon");
            var cihazTuruOrd = TryGetOrdinal(reader, "cihaz_turu");
            var arizaOrd = TryGetOrdinal(reader, "ariza_aciklamasi");
            var durumOrd = TryGetOrdinal(reader, "durum");
            var adminOrd = TryGetOrdinal(reader, "admin_cevabi");
            var kullaniciCevabiOrd = TryGetOrdinal(reader, "kullanici_cevabi");
            var kayitTarihiOrd = TryGetOrdinal(reader, "kayit_tarihi");

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var item = new ServiceRequestForm
                {
                    Id = idOrd.HasValue && !reader.IsDBNull(idOrd.Value) ? Convert.ToInt64(reader.GetValue(idOrd.Value)) : null,
                    KullaniciId = kullaniciIdOrd.HasValue && !reader.IsDBNull(kullaniciIdOrd.Value) ? Convert.ToInt64(reader.GetValue(kullaniciIdOrd.Value)) : null,
                    AdSoyad = adSoyadOrd.HasValue && !reader.IsDBNull(adSoyadOrd.Value) ? reader.GetValue(adSoyadOrd.Value).ToString() ?? string.Empty : string.Empty,
                    Telefon = telefonOrd.HasValue && !reader.IsDBNull(telefonOrd.Value) ? reader.GetValue(telefonOrd.Value).ToString() ?? string.Empty : string.Empty,
                    CihazTuru = cihazTuruOrd.HasValue && !reader.IsDBNull(cihazTuruOrd.Value) ? reader.GetValue(cihazTuruOrd.Value).ToString() ?? string.Empty : string.Empty,
                    ArizaAciklamasi = arizaOrd.HasValue && !reader.IsDBNull(arizaOrd.Value) ? reader.GetValue(arizaOrd.Value).ToString() ?? string.Empty : string.Empty,
                    Durum = durumOrd.HasValue && !reader.IsDBNull(durumOrd.Value) ? reader.GetValue(durumOrd.Value).ToString() : "Bekliyor",
                    AdminCevabi = adminOrd.HasValue && !reader.IsDBNull(adminOrd.Value) ? reader.GetValue(adminOrd.Value).ToString() : string.Empty,
                    KullaniciCevabi = kullaniciCevabiOrd.HasValue && !reader.IsDBNull(kullaniciCevabiOrd.Value) ? reader.GetValue(kullaniciCevabiOrd.Value).ToString() : string.Empty,
                    KayitTarihi = kayitTarihiOrd.HasValue && !reader.IsDBNull(kayitTarihiOrd.Value) ? Convert.ToDateTime(reader.GetValue(kayitTarihiOrd.Value)) : null
                };

                item.SohbetMesajlari = BuildConversationForDisplay(item);
                list.Add(item);
            }

            return list;
        }

        public async Task UpdateUserReplyAsync(long id, string userId, string kullaniciCevabi)
        {
            if (string.IsNullOrWhiteSpace(kullaniciCevabi)) return;

            var request = await GetServiceRequestByIdAsync(id, userId).ConfigureAwait(false);
            if (request == null)
            {
                throw new InvalidOperationException("Talep bulunamadı veya güncelleme yetkiniz yok.");
            }

            var remaining = GetUserReplyCooldownRemaining(request, TimeSpan.FromHours(1));
            if (remaining > TimeSpan.Zero)
            {
                var remainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
                throw new InvalidOperationException($"Yeni cevap göndermek için {remainingMinutes} dakika bekleyin.");
            }

            await UpdateConversationAsync(id, "user", kullaniciCevabi, "Musteri Yanitladi", userId).ConfigureAwait(false);
        }

        private static TimeSpan GetUserReplyCooldownRemaining(ServiceRequestForm request, TimeSpan cooldown)
        {
            var lastUserReplyAtUtc = ParseStoredConversation(request)
                .Where(x => string.Equals(x.Role, "user", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.Text))
                .Select(x => NormalizeToUtc(x.SentAt))
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();

            if (lastUserReplyAtUtc == DateTime.MinValue)
            {
                return TimeSpan.Zero;
            }

            var elapsed = DateTime.UtcNow - lastUserReplyAtUtc;
            var remaining = cooldown - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc) return value;
            if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public async Task CloseRequestAsSolvedAsync(long id, string userId)
        {
            await UpdateConversationAsync(id, "system", null, "Cozuldu", userId).ConfigureAwait(false);
        }

        public async Task UpdateAdminReplyAsync(long id, string adminCevabi, string durum)
        {
            await UpdateConversationAsync(id, "admin", adminCevabi, durum, null).ConfigureAwait(false);
        }

        public async Task AdvanceServiceRequestStatusAsync(long id)
        {
            var request = await GetServiceRequestByIdAsync(id).ConfigureAwait(false);
            if (request == null)
            {
                throw new InvalidOperationException("Talep bulunamadı.");
            }

            if (IsClosedStatus(request.Durum))
            {
                throw new InvalidOperationException("Talep zaten kapatılmış.");
            }

            var current = (request.Durum ?? string.Empty).Trim();
            var next = string.Equals(current, "Inceleniyor", StringComparison.OrdinalIgnoreCase)
                ? "Kapatildi"
                : "Inceleniyor";

            await UpdateConversationAsync(id, "system", null, next, null).ConfigureAwait(false);
        }

        public async Task DeleteServisTalebiAsync(long id)
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            const string sql = "delete from public.servis_talepleri where id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (rows <= 0)
            {
                throw new InvalidOperationException("Silinecek talep bulunamadı.");
            }
        }

        // ─── Ürün Yönetimi ───────────────────────────────────────────
        public async Task InsertProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            await EnsureCategoryExistsAsync(product.Category).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

            var sql = $@"insert into public.urunler(id,name,description,price_text,image_url,{categoryColumn},features)
values (@id,@name,@description,@price_text,@image_url,@category,cast(@features as jsonb))";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", product.Id ?? string.Empty);
            cmd.Parameters.AddWithValue("name", product.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("description", product.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("price_text", product.PriceText ?? string.Empty);
            cmd.Parameters.AddWithValue("image_url", product.ImageUrl ?? string.Empty);
            cmd.Parameters.AddWithValue("category", product.Category ?? string.Empty);
            cmd.Parameters.AddWithValue("features", JsonSerializer.Serialize(product.Features ?? new List<string>()));

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            await EnsureCategoryExistsAsync(product.Category).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

            var sql = $@"update public.urunler
set name=@name,
    description=@description,
    price_text=@price_text,
    image_url=@image_url,
    {categoryColumn}=@category,
    features=cast(@features as jsonb)
where id=@id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", product.Id ?? string.Empty);
            cmd.Parameters.AddWithValue("name", product.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("description", product.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("price_text", product.PriceText ?? string.Empty);
            cmd.Parameters.AddWithValue("image_url", product.ImageUrl ?? string.Empty);
            cmd.Parameters.AddWithValue("category", product.Category ?? string.Empty);
            cmd.Parameters.AddWithValue("features", JsonSerializer.Serialize(product.Features ?? new List<string>()));

            var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (rows <= 0)
            {
                throw new InvalidOperationException("Güncellenecek ürün bulunamadı.");
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await TryGetProductsFromDbAsync().ConfigureAwait(false);
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            return await TryGetProductByIdFromDbAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteProductAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            const string sql = "delete from public.urunler where id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task DeleteProductsAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any()) return;
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            var cleanIds = ids.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (cleanIds.Length == 0) return;

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            const string sql = "delete from public.urunler where id = any(@ids)";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ids", cleanIds);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task AssignCategoryToProductsAsync(IEnumerable<string> ids, string category)
        {
            if (ids == null) return;
            var normalizedCategory = (category ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedCategory)) return;

            await EnsureCategoryExistsAsync(normalizedCategory).ConfigureAwait(false);

            var cleanIds = ids
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!cleanIds.Any()) return;

            foreach (var id in cleanIds)
            {
                await UpdateProductCategoryAsync(id, normalizedCategory).ConfigureAwait(false);
            }
        }

        public async Task UpdateProductCategoryAsync(string productId, string category)
        {
            if (string.IsNullOrWhiteSpace(productId)) return;
            var normalizedCategory = (category ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedCategory)) return;

            await EnsureCategoryExistsAsync(normalizedCategory).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

            var sql = $"update public.urunler set {categoryColumn} = @category where id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("category", normalizedCategory);
            cmd.Parameters.AddWithValue("id", productId);

            var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (rows <= 0)
            {
                throw new InvalidOperationException("Ürün bulunamadı veya kategori güncellenemedi.");
            }
        }

        public async Task ClearProductCategoryAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Ürün bilgisi eksik.", nameof(productId));
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

            var sql = $"update public.urunler set {categoryColumn} = null where id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", productId);

            var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (rows <= 0)
            {
                throw new InvalidOperationException("Ürün bulunamadı veya kategoriden çıkarılamadı.");
            }
        }

        public async Task RenameCategoryAsync(string oldCategoryName, string newCategoryName)
        {
            var oldName = (oldCategoryName ?? string.Empty).Trim();
            var newName = (newCategoryName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(oldName))
                throw new ArgumentException("Eski kategori adı boş olamaz.", nameof(oldCategoryName));
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Yeni kategori adı boş olamaz.", nameof(newCategoryName));
            if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
                return;
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);

            var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

            const string categoryExistsSql = "select 1 from public.kategoriler where lower(name)=lower(@name) limit 1";
            var hasCategoryRow = false;
            await using (var categoryExistsCmd = new NpgsqlCommand(categoryExistsSql, conn, tx))
            {
                categoryExistsCmd.Parameters.AddWithValue("name", oldName);
                hasCategoryRow = await categoryExistsCmd.ExecuteScalarAsync().ConfigureAwait(false) != null;
            }

            var hasProductsSql = $"select 1 from public.urunler where lower({categoryColumn}) = lower(@old_name) limit 1";
            var hasProductsWithCategory = false;
            await using (var hasProductsCmd = new NpgsqlCommand(hasProductsSql, conn, tx))
            {
                hasProductsCmd.Parameters.AddWithValue("old_name", oldName);
                hasProductsWithCategory = await hasProductsCmd.ExecuteScalarAsync().ConfigureAwait(false) != null;
            }

            if (!hasCategoryRow && !hasProductsWithCategory)
            {
                throw new InvalidOperationException("Düzenlenecek kategori bulunamadı.");
            }

            await CreateCategoryWithDbConnectionAsync(conn, newName, tx).ConfigureAwait(false);

            if (hasProductsWithCategory)
            {
                var updateProductsSql = $"update public.urunler set {categoryColumn} = @new_name where lower({categoryColumn}) = lower(@old_name)";
                await using var updateProductsCmd = new NpgsqlCommand(updateProductsSql, conn, tx);
                updateProductsCmd.Parameters.AddWithValue("new_name", newName);
                updateProductsCmd.Parameters.AddWithValue("old_name", oldName);
                await updateProductsCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            if (hasCategoryRow)
            {
                const string deleteOldCategorySql = "delete from public.kategoriler where lower(name)=lower(@old_name)";
                await using var deleteOldCmd = new NpgsqlCommand(deleteOldCategorySql, conn, tx);
                deleteOldCmd.Parameters.AddWithValue("old_name", oldName);
                await deleteOldCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            await tx.CommitAsync().ConfigureAwait(false);
        }

        public async Task DeleteCategoryAsync(string categoryName)
        {
            var category = (categoryName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Silinecek kategori adı boş olamaz.", nameof(categoryName));
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);

            var categoryColumn = await ResolveUrunlerCategoryColumnAsync(conn).ConfigureAwait(false);

            const string existsSql = "select 1 from public.kategoriler where lower(name)=lower(@name) limit 1";
            await using (var existsCmd = new NpgsqlCommand(existsSql, conn, tx))
            {
                existsCmd.Parameters.AddWithValue("name", category);
                var exists = await existsCmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (exists == null)
                {
                    throw new InvalidOperationException("Silinecek kategori bulunamadı.");
                }
            }

            var hasProductsSql = $"select 1 from public.urunler where lower({categoryColumn}) = lower(@category) limit 1";
            var hasProducts = false;
            await using (var hasProductsCmd = new NpgsqlCommand(hasProductsSql, conn, tx))
            {
                hasProductsCmd.Parameters.AddWithValue("category", category);
                hasProducts = await hasProductsCmd.ExecuteScalarAsync().ConfigureAwait(false) != null;
            }

            if (hasProducts)
            {
                throw new InvalidOperationException("Bu kategoride ürün var. Önce ürünleri başka kategoriye taşıyın.");
            }

            const string deleteCategorySql = "delete from public.kategoriler where lower(name)=lower(@category)";
            await using var deleteCmd = new NpgsqlCommand(deleteCategorySql, conn, tx);
            deleteCmd.Parameters.AddWithValue("category", category);
            await deleteCmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            await tx.CommitAsync().ConfigureAwait(false);
        }

        // ─── Kullanıcı Giriş / Kayıt ──────────────────────────────────
        public async Task<UserSession?> LoginAsync(string email, string password)
        {
            return await TryLoginWithDbAsync(email, password).ConfigureAwait(false);
        }

        public async Task<bool> RegisterUserAsync(RegisterViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new InvalidOperationException("ConnectionStrings:SupabaseDb ayarlı değil.");

            await using var conn = new NpgsqlConnection(_dbConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            const string existsSql = "select 1 from public.kullanicilar where lower(email)=lower(@email) limit 1";
            await using (var existsCmd = new NpgsqlCommand(existsSql, conn))
            {
                existsCmd.Parameters.AddWithValue("email", model.Email ?? string.Empty);
                var exists = await existsCmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (exists != null) return false;
            }

            const string insertSql = @"insert into public.kullanicilar(ad_soyad,email,telefon,sifre,is_admin)
values(@ad_soyad,@email,@telefon,@sifre,@is_admin)";

            await using var insertCmd = new NpgsqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("ad_soyad", model.AdSoyad ?? string.Empty);
            insertCmd.Parameters.AddWithValue("email", model.Email ?? string.Empty);
            insertCmd.Parameters.AddWithValue("telefon", model.Telefon ?? string.Empty);
            insertCmd.Parameters.AddWithValue("sifre", BCrypt.Net.BCrypt.HashPassword(model.Sifre ?? string.Empty));
            insertCmd.Parameters.AddWithValue("is_admin", false);

            var rows = await insertCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return rows > 0;
        }
    }
}
