using System;

namespace TekinTeknikServis.Core.Data
{
    public class OrderItemEntity
    {
        public long Id { get; set; }
        public Guid OrderId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string PriceText { get; set; } = string.Empty;
        public int UnitPriceTry { get; set; }
        public int Quantity { get; set; }
        public int LineTotalTry { get; set; }

        public OrderEntity? Order { get; set; }
    }
}
