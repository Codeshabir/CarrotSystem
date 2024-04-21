using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Company
    {
        public int Pk { get; set; }
        public string? CompanyName { get; set; } = null!;
        public bool? Inactive { get; set; }
        public string? Type { get; set; }
        public string? Abn { get; set; }
        public string? VendorNumber { get; set; }
        public string? Comment { get; set; }
        public string? File { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
