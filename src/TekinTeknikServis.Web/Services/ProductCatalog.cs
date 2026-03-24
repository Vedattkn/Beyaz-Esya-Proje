using System.Collections.Generic;

namespace TekinTeknikServis.Web.Services
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PriceText { get; set; }
        public List<string> Features { get; set; }
        public string ImageUrl { get; set; }
    }

    public static class ProductCatalog
    {
        // Node'daki server.js içindeki products objesinin karşılığı.
        public static readonly IReadOnlyDictionary<string, Product> Products =
            new Dictionary<string, Product>
            {
                ["termostat"] = new Product
                {
                    Id = "termostat",
                    Name = "Buzdolabı Termostatı",
                    Description = "Orijinal buzdolabı termostatı. Çoğu marka ile uyumludur.",
                    PriceText = "450 TL",
                    Features = new List<string>
                    {
                        "Uzunluk: 12 cm",
                        "Genişlik: 5 cm",
                        "Ağırlık: 150 gr",
                        "Malzeme: Paslanmaz metal",
                        "Uyumluluk: Arçelik, Beko, Bosch"
                    },
                    ImageUrl = "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400&h=400&fit=crop"
                },
                ["pompa"] = new Product
                {
                    Id = "pompa",
                    Name = "Çamaşır Makinesi Pompası",
                    Description = "Yüksek performanslı su tahliye pompası.",
                    PriceText = "350 TL",
                    Features = new List<string>
                    {
                        "Uzunluk: 10 cm",
                        "Genişlik: 6 cm",
                        "Ağırlık: 200 gr",
                        "Güç: 40W",
                        "Uyumluluk: Arçelik, Vestel, Siemens"
                    },
                    ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=400&fit=crop"
                },
                ["kumanda"] = new Product
                {
                    Id = "kumanda",
                    Name = "Klima Kumandası",
                    Description = "Tüm klima modelleri ile uyumlu akıllı kumanda.",
                    PriceText = "250 TL",
                    Features = new List<string>
                    {
                        "Uzunluk: 15 cm",
                        "Genişlik: 4 cm",
                        "Ağırlık: 100 gr",
                        "Pil Türü: AAA x2",
                        "Uyumluluk: LG, Samsung, Daikin"
                    },
                    ImageUrl = "https://images.unsplash.com/photo-1609389041884-505ee5f37896?w=400&h=400&fit=crop"
                }
            };
    }
}

