namespace TekinTeknikServis.Core.Models
{
    public class UserSession
    {
        public string Id { get; set; } = "";
        public string AdSoyad { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefon { get; set; } = "";
        public bool IsAdmin { get; set; } = false;
        public DateTime GirisTarihi { get; set; } = DateTime.Now;
    }
}
