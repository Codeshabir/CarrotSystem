using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Waste
    {
        public int Id { get; set; }
        public DateTime? WasteDate { get; set; }
        public string? ProductCode { get; set; }
        public int? Qty { get; set; }
        public string? Supplier { get; set; }
        public string? Reason { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
