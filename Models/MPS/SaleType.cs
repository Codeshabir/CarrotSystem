using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class SaleType
    {
        public string? Type { get; set; } = null!;
        public string? Desc { get; set; }
        public int? SortId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
