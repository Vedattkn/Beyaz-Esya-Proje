using Microsoft.AspNetCore.Mvc;
using TekinTeknikServis.Core.Models;
using TekinTeknikServis.Core.Services;

namespace TekinTeknikServis.Core.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly SupabaseService _supabase;

        public UsersApiController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _supabase.GetAllUsersAsync();
            return Ok(users.Select(ToSafeResponse));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var user = await _supabase.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(ToSafeResponse(user));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppUser user)
        {
            if (user == null) return BadRequest("Kullanıcı verisi zorunludur.");
            if (string.IsNullOrWhiteSpace(user.AdSoyad)) return BadRequest("Ad Soyad zorunludur.");
            if (string.IsNullOrWhiteSpace(user.Email)) return BadRequest("E-posta zorunludur.");
            if (string.IsNullOrWhiteSpace(user.Sifre)) return BadRequest("Şifre zorunludur.");

            var newId = await _supabase.CreateUserAsync(user);
            var created = await _supabase.GetUserByIdAsync(newId);
            if (created == null) return StatusCode(500, "Kullanıcı oluşturuldu ancak okunamadı.");

            return CreatedAtAction(nameof(GetById), new { id = newId }, ToSafeResponse(created));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] AppUser user)
        {
            if (user == null) return BadRequest("Kullanıcı verisi zorunludur.");
            if (id <= 0) return BadRequest("Geçersiz kullanıcı id.");

            user.Id = id;
            await _supabase.UpdateUserAsync(user);

            var updated = await _supabase.GetUserByIdAsync(id);
            return Ok(updated == null ? null : ToSafeResponse(updated));
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _supabase.DeleteUserAsync(id);
            return NoContent();
        }

        private static object ToSafeResponse(AppUser user)
        {
            return new
            {
                user.Id,
                user.AdSoyad,
                user.Email,
                user.Telefon,
                user.IsAdmin,
                user.OlusturmaTarihi
            };
        }
    }
}
