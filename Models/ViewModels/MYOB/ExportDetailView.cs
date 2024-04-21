
namespace CarrotSystem.Models.ViewModel
{
    public partial class ExportDetailView
    {
        public int InvoiceID { get; set; }
        public DateTime DateBy { get; set; }
        public string CoLastName { get; set; }
        public string ItemNO { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public string TaxCode { get; set; }
    }
}
