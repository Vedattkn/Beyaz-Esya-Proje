using System.Collections.Generic;

namespace TekinTeknikServis.Core.Models
{
    public class AdminServiceRequestsViewModel
    {
        public string Query { get; set; } = "";
        public List<ServiceRequestForm> Requests { get; set; } = new List<ServiceRequestForm>();
        public ServiceRequestForm? SelectedRequest { get; set; }
    }
}