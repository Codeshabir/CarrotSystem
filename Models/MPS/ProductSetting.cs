using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductSetting
    {
        public int Id { get; set; }
        public string? ProductCode { get; set; }
        public int ProductId { get; set; }
        public bool IsStockCountable { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
