
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class DispatchItemView
    {
        public int DispatchId { get; set; }
        public int DispatchItemId { get; set; }
        public int SaleItemId { get; set; }
        public int SortId { get; set; }
        public int InvoiceId { get; set; }
        
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerDesc { get; set; }

        public float Ordered { get; set; }
        public float Unfilled { get; set; }
        public float Dispatch { get; set; }
        public float Temp { get; set; }

        public string Grower { get; set; }
        public string StringBestBefore { get; set; }
        public DateTime BestBefore { get; set; }

        public bool isClosed { get; set; }
    }

    public partial class DispatchViewModel
    {
        public List<DispatchItemView> dispatchItemList { get; set; }
        public List<SaleDispatch> dispatchList { get; set;}
        public bool isClosed { get; set; }
    }

    public partial class DispatchView
    {
        public int DispatchId { get; set; }
        public int InvoiceId { get; set; }
     
        public string CustPO { get; set; }
        public string DispatchName { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }

        public string Comment { get; set; }
        public string VendorNumber { get; set; }

        public DateTime DispatchDate { get; set; }
    }

}
