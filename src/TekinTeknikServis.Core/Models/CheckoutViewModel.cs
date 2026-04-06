using System.ComponentModel.DataAnnotations;

namespace TekinTeknikServis.Core.Models
{
    public class CheckoutViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(120, ErrorMessage = "Ad Soyad en fazla 120 karakter olabilir.")]
        public string AdSoyad { get; set; } = "";

        [Required(ErrorMessage = "Kart numarası zorunludur.")]
        public string KartNo { get; set; } = "";

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur.")]
        public string SonKullanma { get; set; } = "";

        [Required(ErrorMessage = "CVC alanı zorunludur.")]
        public string Cvc { get; set; } = "";

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = "";

        public bool SartlarKabul { get; set; }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        public int TotalTry => CartItems.Sum(x => x.LineTotalTry);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!SartlarKabul)
            {
                yield return new ValidationResult("Ödeme şartlarını kabul etmelisiniz.", new[] { nameof(SartlarKabul) });
            }

            var cardDigits = new string((KartNo ?? string.Empty).Where(char.IsDigit).ToArray());
            if (cardDigits.Length != 16)
            {
                yield return new ValidationResult("Kart numarası 16 haneli olmalıdır.", new[] { nameof(KartNo) });
            }

            var cvcDigits = new string((Cvc ?? string.Empty).Where(char.IsDigit).ToArray());
            if (cvcDigits.Length != 3)
            {
                yield return new ValidationResult("CVC 3 haneli olmalıdır.", new[] { nameof(Cvc) });
            }

            var normalizedExpiry = (SonKullanma ?? string.Empty).Trim();
            if (!TryParseExpiry(normalizedExpiry, out var expiryYear, out var expiryMonth))
            {
                yield return new ValidationResult("Son kullanma tarihi AA/YY formatında olmalıdır.", new[] { nameof(SonKullanma) });
                yield break;
            }

            var now = DateTime.UtcNow;
            var currentYear = now.Year % 100;
            var currentMonth = now.Month;
            if (expiryYear < currentYear || (expiryYear == currentYear && expiryMonth < currentMonth))
            {
                yield return new ValidationResult("Kartın son kullanma tarihi geçmiş.", new[] { nameof(SonKullanma) });
            }
        }

        private static bool TryParseExpiry(string value, out int year, out int month)
        {
            year = 0;
            month = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var parts = value.Split('/');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out month)) return false;
            if (!int.TryParse(parts[1], out year)) return false;

            return month >= 1 && month <= 12 && year >= 0 && year <= 99;
        }
    }
}