using System;

namespace TekinTeknikServis.Core.Data
{
    public class ServiceRequestEntity
    {
        public long Id { get; set; }
        public long? KullaniciId { get; set; }
        public string AdSoyad { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string CihazTuru { get; set; } = "";
        public string ArizaAciklamasi { get; set; } = "";
        public string? Durum { get; set; }
        public string? AdminCevabi { get; set; }
        public string? KullaniciCevabi { get; set; }
        public DateTime? KayitTarihi { get; set; }
        public string? CustomerEmail { get; set; }
        public string? FaultyPart { get; set; }
        public string? ReplacementPart { get; set; }
        public string? RepairDetails { get; set; }
        public decimal? LaborPriceTry { get; set; }
        public decimal? PartPriceTry { get; set; }
        public decimal? TotalPriceTry { get; set; }
        public string? AdminNotes { get; set; }
        public string? ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? ApprovalToken { get; set; }
        public DateTime? ApprovalRequestedAt { get; set; }
    }
}
