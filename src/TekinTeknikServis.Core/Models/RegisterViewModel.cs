using System.ComponentModel.DataAnnotations;

namespace TekinTeknikServis.Core.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        public string AdSoyad { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Telefon numarası 11 haneli olmalı ve sadece rakamlardan oluşmalıdır (Örn: 05xxxxxxxxx).")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarası tam 11 hane olmalıdır.")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; } = "";

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Sifre", ErrorMessage = "Şifreler uyuşmuyor.")]
        [Display(Name = "Şifre Tekrarı")]
        [DataType(DataType.Password)]
        public string SifreTekrar { get; set; } = "";
    }
}
