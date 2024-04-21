using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductInventory
    {
        public int Pk { get; set; }
        public int? PeriodId { get; set; }
        public string? ProductCode { get; set; }
        public double? Opening { get; set; }
        public double? OpeningValue { get; set; }
        public double? Purchased { get; set; }
        public double? PClaimed { get; set; }
        public double? RClaimed { get; set; }
        public double? Sold { get; set; }
        public double? SClaimed { get; set; }
        public double? UcWasted { get; set; }
        public double? TransFrom { get; set; }
        public double? TransTo { get; set; }
        public double? PackUsed { get; set; }
        public double? Packed { get; set; }
        public double? Calculated { get; set; }
        public double? StockCount { get; set; }
        public double? Variance { get; set; }
        public double? Closing { get; set; }
        public double? Added { get; set; }
        public double? AddedValue { get; set; }
        public double? PackedValue { get; set; }
        public double? Reduction { get; set; }
        public double? ReductionValue { get; set; }
        public double? ClosingValue { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public double? PClaimedValue { get; set; }
        public double? Ytdwaste { get; set; }
        public double? YtdwasteValue { get; set; }
        public double? Ytdclaim { get; set; }
        public double? YtdclaimValue { get; set; }
        public double? Ytd { get; set; }
        public double? Ytdvalue { get; set; }
        public double? Nc { get; set; }
        public double? Ncv { get; set; }
        public int? PackSold { get; set; }
    }
}
