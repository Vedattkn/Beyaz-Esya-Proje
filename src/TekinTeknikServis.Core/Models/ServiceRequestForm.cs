using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TekinTeknikServis.Core.Models
{
    public class ServiceRequestForm
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("kullanici_id")]
        public long? KullaniciId { get; set; }

        [Required]
        [JsonPropertyName("ad_soyad")]
        public string AdSoyad { get; set; } = "";

        [Required]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Telefon 10-11 haneli olmalı.")]
        [JsonPropertyName("telefon")]
        public string Telefon { get; set; } = "";

        [Required]
        [JsonPropertyName("cihaz_turu")]
        public string CihazTuru { get; set; } = "";

        [Required]
        [JsonPropertyName("ariza_aciklamasi")]
        public string ArizaAciklamasi { get; set; } = "";

        [JsonPropertyName("durum")]
        public string? Durum { get; set; } = "Bekliyor";

        [JsonPropertyName("admin_cevabi")]
        public string? AdminCevabi { get; set; }

        [JsonPropertyName("kullanici_cevabi")]
        public string? KullaniciCevabi { get; set; }

        [JsonPropertyName("kayit_tarihi")]
        public DateTime? KayitTarihi { get; set; }
    }
}

