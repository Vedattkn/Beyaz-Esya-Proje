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

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";
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
                    Category = "Buzdolabı",
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
                    Category = "Çamaşır Makinesi",
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
                    Category = "Bulaşık Makinesi",
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
                },
                ["buzdolabi-kapisi"] = new Product
                {
                    Id = "buzdolabi-kapisi",
                    Name = "Buzdolabı Kapı Contası",
                    Category = "Buzdolabı",
                    Description = "Yüksek kaliteli su geçirmez kapı contası.",
                    PriceText = "280 TL",
                    Features = new List<string>
                    {
                        "Malzeme: Kauçuk",
                        "Uzunluk: 180 cm",
                        "Su geçirmez",
                        "Kolay installation"
                    },
                    ImageUrl = "/images/products/kapı_contası.png"
                },
                ["camasir-makinesi-motor"] = new Product
                {
                    Id = "camasir-makinesi-motor",
                    Name = "Çamaşır Makinesi Motoru",
                    Category = "Çamaşır Makinesi",
                    Description = "Yüksek performans elektrik motoru.",
                    PriceText = "890 TL",
                    Features = new List<string>
                    {
                        "Güç: 1800W",
                        "RPM: 1200",
                        "Ağırlık: 5 kg",
                        "Enerji sınıfı: A"
                    },
                    ImageUrl = "/images/products/motor.png"
                },
                ["bulasik-makinesi-filtre"] = new Product
                {
                    Id = "bulasik-makinesi-filtre",
                    Name = "Bulaşık Makinesi Filtresi",
                    Category = "Bulaşık Makinesi",
                    Description = "Yedek ön ve arka filtre seti.",
                    PriceText = "180 TL",
                    Features = new List<string>
                    {
                        "2 parça filtre seti",
                        "Çin kalitesi",
                        "Kolay temizleme",
                        "Uzun ömürlü"
                    },
                    ImageUrl = "/images/products/filtre.png"
                },
                ["buzdolabi-dondurucu-fanı"] = new Product
                {
                    Id = "buzdolabi-dondurucu-fanı",
                    Name = "Buzdolabı Dondurucu Fanı",
                    Category = "Buzdolabı",
                    Description = "Orjinal dondurucu fanı yedek parça.",
                    PriceText = "520 TL",
                    Features = new List<string>
                    {
                        "Güç: 8W",
                        "Hava akışı: 15m³/h",
                        "Sessiz çalışma",
                        "Enerji tasarruflu"
                    },
                    ImageUrl = "/images/products/fan.png"
                },
                ["camasir-makinesi-kapisi"] = new Product
                {
                    Id = "camasir-makinesi-kapisi",
                    Name = "Çamaşır Makinesi Kapı Camı",
                    Category = "Çamaşır Makinesi",
                    Description = "Temperli cam kapı yedek parçası.",
                    PriceText = "750 TL",
                    Features = new List<string>
                    {
                        "Temperli cam",
                        "Şok taşıyıcı",
                        "Su geçirmez",
                        "Güvenli kurulum"
                    },
                    ImageUrl = "/images/products/cam-kapi.png"
                }
            };
    }
}

