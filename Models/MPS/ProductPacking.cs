using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductPacking
    {
        public int Pk { get; set; }
        public int Id { get; set; }
        public DateTime? PackingDate { get; set; }
        public string? ProductCode { get; set; }
        public int? ProductQty { get; set; }
        public DateTime? BestBefore { get; set; }
        public string? LabourCode { get; set; }
        public int? LabourQty { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public bool? Ready { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Description { get; set; }
        public string? Supplier { get; set; }
    }
}
