using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductInventoryValue
    {
        public int Pk { get; set; }
        public int? PeriodId { get; set; }
        public string? ProductCode { get; set; }
        public double? Opening { get; set; }
        public double? Purchased { get; set; }
        public double? PClaimed { get; set; }
        public double? Sold { get; set; }
        public double? Sclaimed { get; set; }
        public double? Wasted { get; set; }
        public double? TransUsed { get; set; }
        public double? Transferred { get; set; }
        public double? PackUsed { get; set; }
        public double? Packed { get; set; }
        public double? NetMove { get; set; }
        public double? Closing { get; set; }
        public double? StockCount { get; set; }
        public double? Variance { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
