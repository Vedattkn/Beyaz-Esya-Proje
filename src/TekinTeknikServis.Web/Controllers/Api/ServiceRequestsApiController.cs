using Microsoft.AspNetCore.Mvc;
using TekinTeknikServis.Core.Models;
using TekinTeknikServis.Core.Services;

namespace TekinTeknikServis.Core.Controllers.Api
{
    [ApiController]
    [Route("api/service-requests")]
    public class ServiceRequestsApiController : ControllerBase
    {
        private readonly SupabaseService _supabase;

        public ServiceRequestsApiController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _supabase.GetAllServiceRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var request = await _supabase.GetServiceRequestByIdForAdminAsync(id);
            if (request == null) return NotFound();

            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceRequestForm form)
        {
            if (form == null) return BadRequest("Talep verisi zorunludur.");
            if (string.IsNullOrWhiteSpace(form.AdSoyad)) return BadRequest("Ad Soyad zorunludur.");
            if (string.IsNullOrWhiteSpace(form.Telefon)) return BadRequest("Telefon zorunludur.");
            if (string.IsNullOrWhiteSpace(form.CihazTuru)) return BadRequest("Cihaz türü zorunludur.");
            if (string.IsNullOrWhiteSpace(form.ArizaAciklamasi)) return BadRequest("Arıza açıklaması zorunludur.");

            await _supabase.InsertServisTalebiAsync(form);
            return Ok(new { message = "Servis talebi oluşturuldu." });
        }

        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateServiceRequestStatusRequest request)
        {
            if (request == null) return BadRequest("Durum verisi zorunludur.");
            if (string.IsNullOrWhiteSpace(request.Durum)) return BadRequest("Durum zorunludur.");

            await _supabase.UpdateAdminReplyAsync(id, request.AdminCevabi ?? string.Empty, request.Durum);
            return Ok(new { message = "Servis talebi güncellendi." });
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _supabase.DeleteServisTalebiAsync(id);
            return NoContent();
        }

        public class UpdateServiceRequestStatusRequest
        {
            public string Durum { get; set; } = "Bekliyor";
            public string? AdminCevabi { get; set; }
        }
    }
}
