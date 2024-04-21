﻿using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class PurchaseItem
    {
        public int Id { get; set; }
        public int? InvoiceId { get; set; }
        public string? ProductCode { get; set; }
        public double? InvoicedQty { get; set; }
        public double? Price { get; set; }
        public string? Job { get; set; }
        public string? Tax { get; set; }
        public float? SortId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
