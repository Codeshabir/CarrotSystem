using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class SaleStatus
    {
        public string? Status { get; set; } = null!;
        public int? SortId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
