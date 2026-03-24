using System;

namespace TekinTeknikServis.Web.Models
{
    [Serializable]
    public class CartItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PriceText { get; set; } // ör: "450 TL"
        public int Quantity { get; set; }

        public int UnitPriceTry
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PriceText)) return 0;
                var normalized = PriceText.Replace("TL", "").Replace("tl", "").Trim();
                int value;
                return int.TryParse(normalized, out value) ? value : 0;
            }
        }

        public int LineTotalTry => UnitPriceTry * Quantity;
    }
}

