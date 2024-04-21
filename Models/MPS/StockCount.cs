using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class StockCount
    {
        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public string? ProductCode { get; set; }
        public double? Qty { get; set; }
        public double? LogLastQty { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public string? BatchCode { get; set; }
    }
}
