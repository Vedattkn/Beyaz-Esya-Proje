using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TekinTeknikServis.Core.Services
{
    public class Product
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("price_text")]
        public string PriceText { get; set; } = "";

        [JsonPropertyName("features")]
        public List<string> Features { get; set; } = new();

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = "";
    }

    public static class ProductCatalog
    {
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
                    ImageUrl = "/images/products/termostat.png"
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
                    ImageUrl = "/images/products/pompa.png"
                },
                ["deterjan-kutusu"] = new Product
                {
                    Id = "deterjan-kutusu",
                    Name = "Bulaşık Makinesi Deterjan Kutusu",
                    Description = "Orijinal yedek parça, kolay montaj.",
                    PriceText = "320 TL",
                    Features = new List<string>
                    {
                        "Malzeme: Dayanıklı Plastik",
                        "Renk: Gri",
                        "Uyumluluk: Arçelik, Beko, Altus",
                        "Özellik: Otomatik yaylı kapak"
                    },
                    ImageUrl = "/images/products/bulasik_kutusu.png"
                }

            };
    }
}

