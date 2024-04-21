using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductTransfer
    {
        public int Id { get; set; }
        public DateTime? TransferDate { get; set; }
        public string? FromProduct { get; set; }
        public double? FromQty { get; set; }
        public string? ToProduct { get; set; }
        public double? ToQty { get; set; }
        public double? Price { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public string? InvoiceNo { get; set; }
    }
}
