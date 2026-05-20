using System;

namespace TekinTeknikServis.Core.Models
{
    public class ServiceRequestApprovalViewModel
    {
        public long Id { get; set; }
        public string Token { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string DeviceType { get; set; } = "";
        public string? FaultyPart { get; set; }
        public string? ReplacementPart { get; set; }
        public string? RepairDetails { get; set; }
        public decimal? LaborPriceTry { get; set; }
        public decimal? PartPriceTry { get; set; }
        public decimal? TotalPriceTry { get; set; }
        public string? AdminNotes { get; set; }
        public string Status { get; set; } = ServiceRequestStatusHelper.Pending;
        public string? ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool CanRespond { get; set; }
    }
}
