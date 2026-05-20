using System.Threading;
using System.Threading.Tasks;
using TekinTeknikServis.Core.Models;

namespace TekinTeknikServis.Core.Services
{
    public interface IWhatsAppService
    {
        bool IsConfigured { get; }
        Task<WhatsAppSendResult> SendServiceRequestConfirmationAsync(ServiceRequestForm form, CancellationToken ct = default);
        Task<WhatsAppSendResult> SendServiceRequestApprovalAsync(ServiceRequestForm form, string approvalLink, CancellationToken ct = default);
    }
}
