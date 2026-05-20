namespace TekinTeknikServis.Core.Services
{
    public class WhatsAppCloudOptions
    {
        public string? BaseUrl { get; set; }
        public string? PhoneNumberId { get; set; }
        public string? AccessToken { get; set; }
        public string? TemplateName { get; set; }
        public string? TemplateLanguage { get; set; }
        public string? BusinessContactText { get; set; }
        public string? ConfirmationText { get; set; }
        public string? ApprovalTemplateName { get; set; }
        public string? ApprovalTemplateLanguage { get; set; }
        public string? ApprovalMessageText { get; set; }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(BaseUrl) &&
            !string.IsNullOrWhiteSpace(PhoneNumberId) &&
            !string.IsNullOrWhiteSpace(AccessToken) &&
            !string.IsNullOrWhiteSpace(TemplateName) &&
            !string.IsNullOrWhiteSpace(TemplateLanguage);
    }
}
