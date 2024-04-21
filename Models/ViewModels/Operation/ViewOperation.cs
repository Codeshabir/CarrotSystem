using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewOperation
    {
        public List<PeriodicView> periodicList { get; set; }
        public List<DateTime> selectDateList { get; set; }
        
        public int focusId { get; set; }
        public string focusDate { get; set; }
        
        public int targetId { get; set; }

        public string dataId { get; set; }
        public string pageTitle { get; set; }

        public List<WasteReason> reasonList { get; set; }
        public List<CustomisedProductItemModel> productList { get; set; }
        public List<Company> supplierList { get; set; }

        public PackingJsonView newPacking { get; set;}
        public WasteJsonView newWaste { get; set; }
        public StockCountJsonView newCount { get; set; }
        public TransferJsonView newTransfer { get; set; }

    }
}
