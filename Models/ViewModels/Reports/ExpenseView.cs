
namespace CarrotSystem.Models.ViewModel
{
    public partial class ExpenseView
    {
        public int ExpenseId { get; set; }
        public int PeriodId { get; set; }
        public string ExpenseCode { get; set; }
        public string ExpenseDesc { get; set; }
        public double Price { get; set; }
        public bool IsClosed { get; set; }
    }
}
