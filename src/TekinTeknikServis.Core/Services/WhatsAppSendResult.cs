using System.Net;

namespace TekinTeknikServis.Core.Services
{
    public sealed class WhatsAppSendResult
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public HttpStatusCode? StatusCode { get; init; }

        public static WhatsAppSendResult Success() => new WhatsAppSendResult { IsSuccess = true };

        public static WhatsAppSendResult Fail(string message, HttpStatusCode? statusCode = null) =>
            new WhatsAppSendResult { IsSuccess = false, ErrorMessage = message, StatusCode = statusCode };
    }
}
