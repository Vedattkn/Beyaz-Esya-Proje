using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TekinTeknikServis.Core.Models;

namespace TekinTeknikServis.Core.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private const string DefaultBaseUrl = "https://graph.facebook.com/v20.0";

        private readonly HttpClient _http;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly WhatsAppCloudOptions _options;

        public WhatsAppService(HttpClient http, IOptions<WhatsAppCloudOptions> options, ILogger<WhatsAppService> logger)
        {
            _http = http;
            _logger = logger;
            _options = options.Value ?? new WhatsAppCloudOptions();
        }

        public bool IsConfigured => ResolveOptions().IsConfigured;

        public async Task<WhatsAppSendResult> SendServiceRequestConfirmationAsync(ServiceRequestForm form, CancellationToken ct = default)
        {
            var options = ResolveOptions();
            if (!options.IsConfigured)
            {
                _logger.LogWarning("WhatsApp Cloud API configuration is missing. Skipping message send.");
                return WhatsAppSendResult.Fail("WhatsApp configuration is missing.");
            }

            var to = NormalizeToE164(form.Telefon);
            if (string.IsNullOrWhiteSpace(to))
            {
                return WhatsAppSendResult.Fail("Customer phone number is invalid.");
            }

            var confirmationText = string.IsNullOrWhiteSpace(options.ConfirmationText)
                ? "Servis talebiniz basariyla alinmistir."
                : options.ConfirmationText.Trim();

            var request = new WhatsAppTemplateRequest
            {
                To = to,
                Template = new WhatsAppTemplate
                {
                    Name = options.TemplateName ?? string.Empty,
                    Language = new WhatsAppTemplateLanguage { Code = options.TemplateLanguage ?? string.Empty },
                    Components =
                    {
                        new WhatsAppTemplateComponent
                        {
                            Parameters =
                            {
                                new WhatsAppTemplateParameter { Text = form.AdSoyad },
                                new WhatsAppTemplateParameter { Text = form.CihazTuru },
                                new WhatsAppTemplateParameter { Text = confirmationText },
                                new WhatsAppTemplateParameter { Text = options.BusinessContactText ?? string.Empty }
                            }
                        }
                    }
                }
            };

            return await SendTemplateAsync(request, options, ct).ConfigureAwait(false);
        }

        public async Task<WhatsAppSendResult> SendServiceRequestApprovalAsync(ServiceRequestForm form, string approvalLink, CancellationToken ct = default)
        {
            var options = ResolveOptions();
            if (string.IsNullOrWhiteSpace(options.PhoneNumberId) || string.IsNullOrWhiteSpace(options.AccessToken) || string.IsNullOrWhiteSpace(options.BaseUrl)
                || string.IsNullOrWhiteSpace(options.ApprovalTemplateName) || string.IsNullOrWhiteSpace(options.ApprovalTemplateLanguage))
            {
                _logger.LogWarning("WhatsApp approval template configuration is missing. Skipping approval message send.");
                return WhatsAppSendResult.Fail("WhatsApp approval template configuration is missing.");
            }

            var to = NormalizeToE164(form.Telefon);
            if (string.IsNullOrWhiteSpace(to))
            {
                return WhatsAppSendResult.Fail("Customer phone number is invalid.");
            }

            var totalText = form.TotalPriceTry.HasValue
                ? form.TotalPriceTry.Value.ToString("N2") + " TL"
                : string.Empty;

            var request = new WhatsAppTemplateRequest
            {
                To = to,
                Template = new WhatsAppTemplate
                {
                    Name = options.ApprovalTemplateName,
                    Language = new WhatsAppTemplateLanguage { Code = options.ApprovalTemplateLanguage },
                    Components =
                    {
                        new WhatsAppTemplateComponent
                        {
                            Parameters =
                            {
                                new WhatsAppTemplateParameter { Text = form.AdSoyad },
                                new WhatsAppTemplateParameter { Text = form.CihazTuru },
                                new WhatsAppTemplateParameter { Text = totalText },
                                new WhatsAppTemplateParameter { Text = approvalLink }
                            }
                        }
                    }
                }
            };

            return await SendTemplateAsync(request, options, ct).ConfigureAwait(false);
        }

        private async Task<WhatsAppSendResult> SendTemplateAsync(WhatsAppTemplateRequest request, WhatsAppCloudOptions options, CancellationToken ct)
        {
            var url = BuildMessagesUrl(options);
            var json = JsonSerializer.Serialize(request);
            using var message = new HttpRequestMessage(HttpMethod.Post, url);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
            message.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var response = await _http.SendAsync(message, ct).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return WhatsAppSendResult.Success();
                }

                var payload = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _logger.LogWarning("WhatsApp Cloud API failed with status {StatusCode}. Response: {Response}",
                    response.StatusCode, payload);
                return WhatsAppSendResult.Fail("WhatsApp Cloud API request failed.", response.StatusCode);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "WhatsApp Cloud API request timed out.");
                return WhatsAppSendResult.Fail("WhatsApp Cloud API request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "WhatsApp Cloud API request failed.");
                return WhatsAppSendResult.Fail("WhatsApp Cloud API request failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending WhatsApp message.");
                return WhatsAppSendResult.Fail("Unexpected WhatsApp error.");
            }
        }

        private WhatsAppCloudOptions ResolveOptions()
        {
            return new WhatsAppCloudOptions
            {
                BaseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? DefaultBaseUrl : _options.BaseUrl.Trim(),
                PhoneNumberId = _options.PhoneNumberId?.Trim(),
                AccessToken = _options.AccessToken?.Trim(),
                TemplateName = _options.TemplateName?.Trim(),
                TemplateLanguage = _options.TemplateLanguage?.Trim(),
                BusinessContactText = _options.BusinessContactText?.Trim(),
                ConfirmationText = _options.ConfirmationText?.Trim(),
                ApprovalTemplateName = _options.ApprovalTemplateName?.Trim(),
                ApprovalTemplateLanguage = _options.ApprovalTemplateLanguage?.Trim(),
                ApprovalMessageText = _options.ApprovalMessageText?.Trim()
            };
        }

        private static string BuildMessagesUrl(WhatsAppCloudOptions options)
        {
            var baseUrl = options.BaseUrl?.Trim().TrimEnd('/') ?? DefaultBaseUrl;
            var phoneId = options.PhoneNumberId?.Trim().TrimEnd('/') ?? string.Empty;
            return $"{baseUrl}/{phoneId}/messages";
        }

        private static string NormalizeToE164(string? rawPhone)
        {
            if (string.IsNullOrWhiteSpace(rawPhone)) return string.Empty;

            var digits = Regex.Replace(rawPhone, "[^0-9]", "");
            if (digits.StartsWith("0") && digits.Length == 11)
            {
                return "90" + digits.Substring(1);
            }

            if (digits.Length == 10)
            {
                return "90" + digits;
            }

            if (digits.StartsWith("90") && digits.Length == 12)
            {
                return digits;
            }

            return digits;
        }
    }
}
