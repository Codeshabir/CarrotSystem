using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class SalesView
    {
        public int InvoiceId { get; set; }
        public DateTime ShippingDate { get; set; }
        public int CustId { get; set; }
        public string Customer { get; set; }
        
        public string State { get; set; }
        public string CustPO { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string ClaimRef { get; set; }
        public string Comment { get; set; }

        //Details
        public int AddId { get; set; }
        public string Address { get; set; }
        public float Revision { get; set; }

        public string SupplierCode { get; set; }
        public string ABN { get; set; }

        public string strDelDate { get; set; }
        public string strArrDate { get; set; }

        public DateTime DeliveryDate { get; set; }
        public DateTime ArrivalDate { get; set; }

        public bool IsStockReturn { get; set; }

        //Total
        public SalesTotalView invTotal { get; set; }
        public SalesCratesTotalView cratesTotal { get; set; }
    }

    public partial class SalesTotalView
    {
        public decimal OrderedSubTotal { get; set; }
        public decimal InvoiceSubTotal { get; set; }
        public decimal OrderedTaxTotal { get; set; }
        public decimal InvoiceTaxTotal { get; set; }
        public decimal OrderedTotal { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal Total { get; set; }
    }

    public partial class SalesCratesTotalView
    {
        public int CratesA { get; set; }
        public int CratesB { get; set; }
        public int CratesC { get; set; }
        public int CratesD { get; set; }
        public int CratesE { get; set; }
        public int CratesF { get; set; }
    }

}
