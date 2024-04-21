
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class JsonViewModels
    {
        public JsonViewModels()
        {   
        }

        public PurchaseView purchaseView { get; set; }
        public List<PurchaseView> purchaseList { get; set; }
        
        public List<PurchaseItem> tccPurchaseItemList { get; set; }

        public SalesView saleView { get; set; }
        public List<SalesView> saleList { get; set; }
        
        public List<SaleItem> tccSaleItemList { get; set; }

        public DispatchView dispatchView { get; set; }
        public List<SaleDispatchItem> tccDispatchItemList { get; set; }
    }
}
