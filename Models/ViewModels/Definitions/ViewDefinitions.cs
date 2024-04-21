
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewDefinitions
    {
        public CompanyView companyView { get; set; }
        public Company company { get; set; }
        public CompanyModel targetModel { get; set; }
        public List<CompanyView> companyList { get; set; }

        public Address address { get; set; }
        public Address billAddress { get; set; }
        public Address shipAddress { get; set; }
        public List<Address> shippingAddressList { get; set; }

        public List<ProductMainGroup> productMainGroupList { get; set; }
        public List<ProductView> productViewList { get; set; }

        public List<MappingView> mappingList { get; set; }

        public string show { get; set; }
        public string status { get; set; }
        public string filter { get; set; }

        public string dataId { get; set; }

        public string pageTitle { get; set; }
        public string pageType { get; set; }

        public int addressId { get; set; }
        public string compPK { get; set; }
    }
}
