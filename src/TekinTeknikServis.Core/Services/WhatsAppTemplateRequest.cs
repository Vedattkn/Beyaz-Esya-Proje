using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TekinTeknikServis.Core.Services
{
    public class WhatsAppTemplateRequest
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonPropertyName("to")]
        public string To { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "template";

        [JsonPropertyName("template")]
        public WhatsAppTemplate Template { get; set; } = new WhatsAppTemplate();
    }

    public class WhatsAppTemplate
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("language")]
        public WhatsAppTemplateLanguage Language { get; set; } = new WhatsAppTemplateLanguage();

        [JsonPropertyName("components")]
        public List<WhatsAppTemplateComponent> Components { get; set; } = new List<WhatsAppTemplateComponent>();
    }

    public class WhatsAppTemplateLanguage
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";
    }

    public class WhatsAppTemplateComponent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "body";

        [JsonPropertyName("parameters")]
        public List<WhatsAppTemplateParameter> Parameters { get; set; } = new List<WhatsAppTemplateParameter>();
    }

    public class WhatsAppTemplateParameter
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }
}
