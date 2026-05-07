using System;
using System.Collections.Generic;

namespace TekinTeknikServis.Core.Data
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int TotalTry { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public List<OrderItemEntity> Items { get; set; } = new();
    }
}
