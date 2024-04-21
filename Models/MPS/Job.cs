using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Job
    {
        public string? Code { get; set; } = null!;
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
