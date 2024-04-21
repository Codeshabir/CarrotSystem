
namespace CarrotSystem.Models.ViewModel
{
    public partial class ClaimRefItem
    {
        public int InvoiceId { get; set; }
        public string CompanyName { get; set; }
        public string DisplayDate { get; set; }
        public string DisplayOption { get; set; }

        public string InvoiceBn { get; set; }
        public int Revision { get; set; }
        public int CustPk { get; set; }
        public int AddressId { get; set; }
        public string Address { get; set; }
        public string CustPo { get; set; }
        public bool IsReturned { get; set; }
    }
}
