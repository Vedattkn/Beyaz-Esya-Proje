using System;
using System.Text.Json.Serialization;

namespace TekinTeknikServis.Core.Models
{
    public class AppUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("ad_soyad")]
        public string AdSoyad { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("telefon")]
        public string Telefon { get; set; } = "";

        [JsonPropertyName("sifre")]
        public string Sifre { get; set; } = "";

        [JsonPropertyName("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonPropertyName("olusturma_tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
    }
}
