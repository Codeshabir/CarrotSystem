using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class SaleItemView
    {
        public int ItemId { get; set; }
        public int? ProductId { get; set; }
        public int? CustProductId { get; set; }
        public int InvoiceId { get; set; }
        
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerDesc { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyDesc { get; set; }

        public double OrderedQty { get; set; }
        public double InvoicedQty { get; set; }
        public double Price { get; set; }
        public double Gst { get; set; }

        public string Job { get; set; }
        public string Tax { get; set; }
        public string CratesType { get; set; }

        public double FreightProportion { get; set; }
        public float SortID { get; set; }

        public DateTime UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public decimal OrderedTotal { get; set; }
        public decimal InvoiceTotal { get; set; }

    }
}
