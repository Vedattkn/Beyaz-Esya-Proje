namespace TekinTeknikServis.Core.Data
{
    public class CategoryEntity
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}