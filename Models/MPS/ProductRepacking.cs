using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductRepacking
    {
        public int Id { get; set; }
        public DateTime? RepackingDate { get; set; }
        public string? ProductCode { get; set; }
        public int? ProductQty { get; set; }
        public string? LabourCode { get; set; }
        public double? LabourCost { get; set; }
        public string? Reason { get; set; }
        public string? Comment { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
