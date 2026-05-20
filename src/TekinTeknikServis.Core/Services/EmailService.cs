using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using TekinTeknikServis.Core.Models;

namespace TekinTeknikServis.Core.Services
{
    public class EmailService
    {
        private readonly string? _smtpHost;
        private readonly int _smtpPort;
        private readonly string? _smtpUser;
        private readonly string? _smtpPassword;
        private readonly string? _toEmail;
        private readonly string? _fromEmail;

        public EmailService(IConfiguration config)
        {
            _smtpHost = config["Email:SmtpHost"]?.Trim();
            _smtpPort = int.TryParse(config["Email:SmtpPort"], out var p) ? p : 587;
            _smtpUser = config["Email:SmtpUser"]?.Trim();
            _smtpPassword = config["Email:SmtpPassword"]?.Trim();
            _toEmail = config["Email:To"]?.Trim();
            _fromEmail = config["Email:From"]?.Trim() ?? _smtpUser;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_smtpHost) &&
            !string.IsNullOrWhiteSpace(_smtpUser) &&
            !string.IsNullOrWhiteSpace(_smtpPassword) &&
            !string.IsNullOrWhiteSpace(_toEmail);

        public async Task SendServisTalebiAsync(ServiceRequestForm form, CancellationToken ct = default)
        {
            if (!IsConfigured) return;

            var subject = "[Tekin Teknik Servis] Yeni Servis Talebi";
            var body = new StringBuilder();
            body.AppendLine("Yeni bir servis talebi alındı.");
            body.AppendLine();
            body.AppendLine("--- TALEP DETAYLARI ---");
            body.AppendLine("Ad Soyad: " + form.AdSoyad);
            body.AppendLine("Telefon: " + form.Telefon);
            body.AppendLine("Kategori: " + form.CihazTuru);
            if (!string.IsNullOrWhiteSpace(form.SecilenParcaAdi))
            {
                body.AppendLine("Seçilen Parça: " + form.SecilenParcaAdi);
            }
            body.AppendLine("Arıza Açıklaması: " + form.ArizaAciklamasi);
            body.AppendLine();
            body.AppendLine("---");

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
            };

            var msg = new MailMessage
            {
                From = new MailAddress(_fromEmail ?? _smtpUser!, "Tekin Teknik Servis"),
                Subject = subject,
                Body = body.ToString(),
                IsBodyHtml = false
            };
            msg.To.Add(_toEmail!);

            await client.SendMailAsync(msg, ct).ConfigureAwait(false);
        }

        public async Task SendServiceRequestApprovalAsync(ServiceRequestForm form, string approvalLink, CancellationToken ct = default)
        {
            if (!IsConfigured) return;

            var toCustomer = form.CustomerEmail?.Trim();
            if (string.IsNullOrWhiteSpace(toCustomer)) return;

            var subject = "[Tekin Teknik Servis] Onay Bekleyen Servis İşlemi";
            var body = new StringBuilder();
            body.AppendLine("Merhaba " + form.AdSoyad + ",");
            body.AppendLine();
            body.AppendLine("Servis işlemi için fiyat teklifimizi onaylamanız gerekiyor.");
            body.AppendLine("Toplam Tutar: " + (form.TotalPriceTry?.ToString("N2") ?? "-") + " TL");
            body.AppendLine();
            body.AppendLine("Onaylamak veya reddetmek için aşağıdaki linke tıklayın:");
            body.AppendLine(approvalLink);
            body.AppendLine();
            body.AppendLine("Teşekkürler.");

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
            };

            var msg = new MailMessage
            {
                From = new MailAddress(_fromEmail ?? _smtpUser!, "Tekin Teknik Servis"),
                Subject = subject,
                Body = body.ToString(),
                IsBodyHtml = false
            };
            msg.To.Add(toCustomer);

            await client.SendMailAsync(msg, ct).ConfigureAwait(false);
        }
    }
}
