using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Myoblog
    {
        public int Index { get; set; }
        public int? PeriodId { get; set; }
        public string? Type { get; set; }
        public string? Target { get; set; }
        public int? InvoiceId { get; set; }
        public string? Result { get; set; }
        public string? ErrorNumber { get; set; }
        public string? ErrorDescription { get; set; }
        public string? ErrorSource { get; set; }
        public DateTime? ExportedOn { get; set; }
        public string? ExportedBy { get; set; }
    }
}
