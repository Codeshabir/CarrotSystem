using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductMapping
    {
        public int Id { get; set; }
        public string? Company { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyDesc { get; set; }
        public string? MercCode { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
