using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewXero
    {
        public string focusDate { get; set; }
        public int focusId { get; set; }

        public List<PeriodicView> periodList { get; set; }
        public List<XeroLog> responseLogList { get; set; }
        public List<ExportDetailView> exportSalesList { get; set; }
        public List<ExportDetailView> exportPurchasesList { get; set; }
    }
}
