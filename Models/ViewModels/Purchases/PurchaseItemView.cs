using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class PurchaseItemView
    {
        public int ItemId { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }

        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyDesc { get; set; }
        public string Job { get; set; }
        public string Tax { get; set; }

        public double InvoicedQty { get; set; }
        public decimal Price { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal TaxTotal { get; set; }

        public DateTime UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }
}
