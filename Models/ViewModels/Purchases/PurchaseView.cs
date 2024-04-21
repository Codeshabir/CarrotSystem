
namespace CarrotSystem.Models.ViewModel
{
    public partial class PurchaseView
    {
        public int InvoiceId { get; set; }
        public int CompanyId { get; set; }

        public string Company { get; set; }
        public string CompanyNo { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string ClaimRef { get; set; }
        public string Comment { get; set; }

        public string SupplierName { get; set; }
        public string InvoiceBN { get; set; }
        public string BillingAddress { get; set; }

        public bool IsStockReturn { get; set; }

        public DateTime DeliveryDate { get; set; }

        public double Total { get; set; }
        public PurchaseTotalView TotalView { get; set; }
    }

    public partial class PurchaseClaimView
    {
        public int InvoiceId { get; set; }
        public string InvoiceBN { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal SubTotal { get; set; }
    }

    public partial class PurchaseTotalView
    {
        public decimal TaxTotal { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal SubTotal { get; set; }
    }
}
