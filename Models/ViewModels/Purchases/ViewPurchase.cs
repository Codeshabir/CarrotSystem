
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewPurchase
    {
        public List<PurchaseView> purchaseViewList { get; set; }
        public List<PurchaseItemView> purchaseItemViewList { get; set; }
        public List<PurchaseClaimView> claimList { get; set; }
        public List<Company> detailSupplierList { get; set; }
        public List<Purchase> claimRefList { get; set; }

        public PurchaseTotalView totalView { get; set; }
        public PurchaseView purchaseView { get; set; }

        public NewPurchaseJson newPurchase { get; set; }

        public List<string> supplierList { get; set; }
        public List<string> typeList { get; set; }
        public List<CustomisedProductItemModel> productList { get; set; }

        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }

        public string strDateFrom { get; set; }
        public string strDateTo { get; set; }

        public string supplier { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string isShowAll { get; set; }

        public string dataId { get; set; }
        public string pageTitle { get; set; }
        
        public bool isNew { get; set; }
        public bool isClosed { get; set; }
    }
}
