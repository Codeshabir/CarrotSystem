using MYOB.AccountRight.SDK.Contracts.Version2.Contact;
using MYOB.AccountRight.SDK.Contracts.Version2.GeneralLedger;
using MYOB.AccountRight.SDK.Contracts.Version2.Inventory;

namespace CarrotSystem.Models.ViewModel
{
    public partial class JsonConvertCustomer
    {
        public Customer[] Items { get; set; }
    }

    public partial class JsonConvertSupplier
    {
        public Supplier[] Items { get; set; }
    }

    public partial class JsonConvertAccount
    {
        public Account[] Items { get; set; }
    }

    public partial class JsonConvertJob
    {
        public MYOB.AccountRight.SDK.Contracts.Version2.GeneralLedger.Job[] Items { get; set; }
    }

    public partial class JsonConvertTaxCode
    {
        public TaxCode[] Items { get; set; }
    }

    public partial class JsonConvertItem
    {
        public Item[] Items { get; set; }
    }

    public partial class JsonConvertLocation
    {
        public Location[] Items { get; set; }
    }
}
