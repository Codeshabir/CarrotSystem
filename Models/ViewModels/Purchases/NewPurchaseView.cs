using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class NewPurchaseView
    {
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyDesc { get; set; }
        public string Tax { get; set; }
    }

    public partial class NewPurchaseJson
    {
        public int InvoiceId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string DeliveryDate { get; set; }
        public string InvoiceBN { get; set; }
        public string Comment { get; set; }
        public string BillingAddress { get; set; }
        public int Supplier { get; set; }
        public string ClaimRef { get; set; }
        public bool ReturnStock { get; set; }
    }

    public partial class NewPurchaseItemJson
    {
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public decimal InvoiceQty { get; set; }
        public decimal Price { get; set; }
        public string Job { get; set; }
        public string Tax { get; set; }
    }
}
