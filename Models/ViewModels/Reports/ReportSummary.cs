
namespace CarrotSystem.Models.ViewModel
{
    public partial class ReportSummary
    {
        public int ReportId { get; set; }
        public string? CompName { get; set; }
        public string? Type { get; set; }
        public string? BoxTitle { get; set; }
        public decimal BoxNumber { get; set; }
        public string? BoxId { get; set; }
        public string? LeftBtnId { get; set; }
        public string? RightBtnId { get; set; }
        public bool IsMoney { get; set; }
        public decimal Percentage { get; set; }
        public bool IsIncreased { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
