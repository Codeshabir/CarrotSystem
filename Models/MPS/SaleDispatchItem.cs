using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class SaleDispatchItem
    {
        public int Id { get; set; }
        public int? DispatchId { get; set; }
        public int? SaleItemId { get; set; }
        public int? Qty { get; set; }
        public double? Temp { get; set; }
        public string? Grower { get; set; }
        public DateTime? BestBefore { get; set; }
        public int? SortId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
