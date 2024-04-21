
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class DetailViewPacking
    {
        public List<CustomisedProductItemModel> productList { get; set; }
        public List<Company> supplierList { get; set; }
        public List<PackingView> detailList { get; set; }
    }
}
