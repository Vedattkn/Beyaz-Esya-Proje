using System;
using System.Text.Json.Serialization;

namespace TekinTeknikServis.Core.Models
{
    public class ServiceRequestMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "";

        [JsonPropertyName("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}