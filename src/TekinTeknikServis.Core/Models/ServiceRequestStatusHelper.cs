using System;

namespace TekinTeknikServis.Core.Models
{
    public static class ServiceRequestStatusHelper
    {
        public const string Pending = "Pending";
        public const string Reviewed = "Reviewed";
        public const string WaitingCustomerApproval = "WaitingCustomerApproval";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Completed = "Completed";

        public static bool IsClosed(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;

            var normalized = status.Trim();
            return string.Equals(normalized, Completed, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, Rejected, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Kapatildi", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Cozuldu", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Kapali", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Kapalı", StringComparison.OrdinalIgnoreCase);
        }

        public static string ToDisplay(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Beklemede";

            var normalized = status.Trim();
            if (string.Equals(normalized, Pending, StringComparison.OrdinalIgnoreCase)) return "Beklemede";
            if (string.Equals(normalized, Reviewed, StringComparison.OrdinalIgnoreCase)) return "İncelendi";
            if (string.Equals(normalized, WaitingCustomerApproval, StringComparison.OrdinalIgnoreCase)) return "Müşteri Onayı Bekleniyor";
            if (string.Equals(normalized, Approved, StringComparison.OrdinalIgnoreCase)) return "Onaylandı";
            if (string.Equals(normalized, Rejected, StringComparison.OrdinalIgnoreCase)) return "Reddedildi";
            if (string.Equals(normalized, Completed, StringComparison.OrdinalIgnoreCase)) return "Tamamlandı";
            if (string.Equals(normalized, "Inceleniyor", StringComparison.OrdinalIgnoreCase)) return "İnceleniyor";
            if (string.Equals(normalized, "Bekliyor", StringComparison.OrdinalIgnoreCase)) return "Bekliyor";
            if (string.Equals(normalized, "Kapatildi", StringComparison.OrdinalIgnoreCase)) return "Kapatıldı";
            if (string.Equals(normalized, "Cozuldu", StringComparison.OrdinalIgnoreCase)) return "Çözüldü";
            if (string.Equals(normalized, "Kapali", StringComparison.OrdinalIgnoreCase)) return "Kapalı";
            if (string.Equals(normalized, "Kapalı", StringComparison.OrdinalIgnoreCase)) return "Kapalı";

            return normalized;
        }
    }
}
