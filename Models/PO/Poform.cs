using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.PO
{
    public partial class Poform
    {
        public int FormId { get; set; }
        public string? Status { get; set; }
        public string? IssuedBy { get; set; }
        public DateTime DateIssued { get; set; }
        public string? SentBy { get; set; }
        public DateTime SentOn { get; set; }
        public string? Ponumber { get; set; }
        public string? RequiredBy { get; set; }
        public string? Supplier { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int FileSetId { get; set; }
        public string? Code { get; set; }
        public string? PaymentMethod { get; set; }
        public string? CheckedBy { get; set; }
        public DateTime DateChecked { get; set; }
        public string? VerificationBy { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
