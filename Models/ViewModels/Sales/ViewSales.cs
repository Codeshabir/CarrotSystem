using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewSales
    {
        public ViewSales()
        { }

        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }

        public string stringDateFrom { get; set; }
        public string stringDateTo { get; set; }
        public string stringStatus { get; set; }
        public string stringCustomer { get; set; }

        public List<Company> customerList { get; set; }
        public List<SalesView> invoiceList { get; set; }
        public List<SaleItemView> invoiceItemList { get; set; }
        public List<SaleItem> saleItemList { get;set;}
        public List<EmailGroup> emailList { get; set; }

        //Details
        public List<CustomisedProductItemModel> productList { get; set; }
        public List<SaleType> typeList { get; set; }
        public List<Address> shipList { get; set; }
        public List<SaleDispatch> dispatchList { get; set; }
        public List<ClaimRefItem> claimRefList { get; set; }

        public SalesView saleView { get; set; }
        public SaleItemView itemView { get; set; }
        public SaleItem saleItem { get;set; }
        public Sale sale { get; set; }

        public Address address { get;set;}
        public Tax tax { get; set; }

        public IFormFile importFile { get; set; }
        
        public int emailId { get; set; }

        public bool isNew { get; set; }
        public bool isClosed { get; set; }
        public bool canHold { get; set; }

        public int invoiceId { get; set; }
        public int dispatchId { get; set; }
        public string dispatchActionType { get; set; }
        public string isShowAll { get; set; }
    }
}
