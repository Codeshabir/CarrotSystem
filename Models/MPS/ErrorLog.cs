using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ErrorLog
    {
        public int Index { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Interface { get; set; }
        public string? Module { get; set; }
        public string? ErrorNumber { get; set; }
        public string? ErrorDescription { get; set; }
        public string? ErrorSource { get; set; }
    }
}
