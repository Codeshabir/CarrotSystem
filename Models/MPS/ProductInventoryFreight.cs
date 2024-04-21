using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductInventoryFreight
    {
        public int Pk { get; set; }
        public int? PeriodId { get; set; }
        public string? State { get; set; }
        public string? ProductCode { get; set; }
        public double? TotalQty { get; set; }
        public double? TotalFreight { get; set; }
        public double? FreightUnitCost { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
