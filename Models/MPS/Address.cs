using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Address
    {
        public int Id { get; set; }
        public string? AddressName { get; set; }
        public string? Company { get; set; }
        public string? Type { get; set; }
        public double? FreightRate { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
        public string? ContactName { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Www { get; set; }
        public string? Comment { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
