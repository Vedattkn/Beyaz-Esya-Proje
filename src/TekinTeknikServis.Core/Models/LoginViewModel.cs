using System.ComponentModel.DataAnnotations;

namespace TekinTeknikServis.Core.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = "";

        [Display(Name = "Beni Hatırla")]
        public bool BeniHatirla { get; set; }
    }
}
