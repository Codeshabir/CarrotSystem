using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Sale
    {
        public int InvoiceId { get; set; }
        public string? Company { get; set; }
        public string? CompanyNo { get; set; }
        public float? Revision { get; set; }
        public int? ShippingAddress { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public int? ClaimReference { get; set; }
        public string? Comment { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
