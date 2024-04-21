using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class CompanyType
    {
        public string? Type { get; set; } = null!;
        public int? SortId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
