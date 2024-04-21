
namespace CarrotSystem.Models.ViewModel
{
    public partial class RawProduceView
    {
        public int ReportId { get; set; }
        public int PeriodId { get; set; }

        public bool IsDisplay { get; set; }

        public string ProductCode { get; set; }
        public string Description { get; set; }

        public double OpeningStock { get; set; }
        public double OpeningValue { get; set; }
        
        public double Purchased { get; set; }
        public double PClaimed { get; set; }
        public double RClaimed { get; set; }

        public double Sold { get; set; }
        public double SClaimed { get; set; }

        public double Wasted { get; set; }
        public double TransFrom { get; set; }
        public double TransTo { get; set; }

        public double PackUsed { get; set; }
        public double Packed { get; set; }

        public double Calculated { get; set; }
        public double StockCount { get; set; }
        public double Variance { get; set; }

        public double Closing { get; set; }
        public double ClosingValue { get; set; }

        public double Added { get; set; }
        public double Taken { get; set; }
        public double StockCalc { get; set; }
        public double VarUnits { get; set; }
        public double CostingUnit { get; set; }
        public double VarCost { get; set; }
        public double ClosingStock { get; set; }

        public double YTDWasteUnits { get; set; }
        public double YTDWasteAmount { get; set; }
        public double YTDVarUnits { get; set; }
        public double YTDVarAmount { get; set; }
    }
}
