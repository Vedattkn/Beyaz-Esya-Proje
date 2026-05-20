using System;
using System.Collections.Generic;
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
        public string Adres { get; set; } = "";


        [Required]
        [JsonPropertyName("ariza_aciklamasi")]
        public string ArizaAciklamasi { get; set; } = "";

        [JsonPropertyName("durum")]
        public string? Durum { get; set; } = ServiceRequestStatusHelper.Pending;

        [JsonPropertyName("customer_email")]
        public string? CustomerEmail { get; set; }

        [JsonPropertyName("admin_cevabi")]
        public string? AdminCevabi { get; set; }

        [JsonPropertyName("kullanici_cevabi")]
        public string? KullaniciCevabi { get; set; }

        [JsonPropertyName("kayit_tarihi")]
        public DateTime? KayitTarihi { get; set; }

        [JsonPropertyName("faulty_part")]
        public string? FaultyPart { get; set; }

        [JsonPropertyName("replacement_part")]
        public string? ReplacementPart { get; set; }

        [JsonPropertyName("repair_details")]
        public string? RepairDetails { get; set; }

        [JsonPropertyName("labor_price")]
        public decimal? LaborPriceTry { get; set; }

        [JsonPropertyName("part_price")]
        public decimal? PartPriceTry { get; set; }

        [JsonPropertyName("total_price")]
        public decimal? TotalPriceTry { get; set; }

        [JsonPropertyName("admin_notes")]
        public string? AdminNotes { get; set; }

        [JsonPropertyName("approval_status")]
        public string? ApprovalStatus { get; set; }

        [JsonPropertyName("approval_date")]
        public DateTime? ApprovalDate { get; set; }

        [JsonPropertyName("approval_token")]
        public string? ApprovalToken { get; set; }

        [JsonPropertyName("approval_requested_at")]
        public DateTime? ApprovalRequestedAt { get; set; }

        [JsonIgnore]
        public List<ServiceRequestMessage> SohbetMesajlari { get; set; } = new List<ServiceRequestMessage>();

        [JsonIgnore]
        public string? SecilenParcaId { get; set; }

        [JsonIgnore]
        public string? SecilenParcaAdi { get; set; }
    }
}

