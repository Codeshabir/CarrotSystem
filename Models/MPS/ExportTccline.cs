using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ExportTccline
    {
        public int Pk { get; set; }
        public string? JournalNo { get; set; }
        public DateTime DateOccurred { get; set; }
        public string? Memo { get; set; }
        public string? Gstreporting { get; set; }
        public string? Inclusive { get; set; }
        public string? AccountNo { get; set; }
        public decimal DebitExTax { get; set; }
        public decimal DebitInTax { get; set; }
        public decimal CreditExTax { get; set; }
        public decimal CreditInTax { get; set; }
        public string? Job { get; set; }
        public string? TaxCode { get; set; }
        public string? ExportedBy { get; set; }
        public DateTime ExportedOn { get; set; }
    }
}
