using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Constant
    {
        public int Pk { get; set; }
        public string? Company { get; set; }
        public string? FullName { get; set; }
        public string? Abn { get; set; }
        public string? Acn { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Postcode { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public string? Www { get; set; }
        public string? File { get; set; }
        public string? Comment { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public string? AccountingEmail { get; set; }
        public string? EmailNewProduct { get; set; }
        public string? EmailNewCompany { get; set; }
        public string? EmailNewInvoice { get; set; }
    }
}
