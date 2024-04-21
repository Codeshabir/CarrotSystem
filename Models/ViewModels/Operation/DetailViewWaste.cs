
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class DetailViewWaste
    {
        public List<CustomisedProductItemModel> productList { get; set; }
        public List<Company> supplierList { get; set; }
        public List<WasteReason> reasonList { get; set; }
        public List<WasteView> detailList { get; set; }

    }
}
