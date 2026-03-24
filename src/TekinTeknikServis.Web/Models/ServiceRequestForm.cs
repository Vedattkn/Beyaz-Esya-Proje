using System.ComponentModel.DataAnnotations;

namespace TekinTeknikServis.Web.Models
{
    public class ServiceRequestForm
    {
        [Required]
        public string AdSoyad { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Telefon 10-11 haneli olmalı.")]
        public string Telefon { get; set; }

        [Required]
        public string CihazTuru { get; set; }

        [Required]
        public string ArizaAciklamasi { get; set; }
    }
}

