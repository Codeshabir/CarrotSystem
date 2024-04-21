using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Tax
    {
        public string? Code { get; set; } = null!;
        public double? Rate { get; set; }
        public short? SortId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
