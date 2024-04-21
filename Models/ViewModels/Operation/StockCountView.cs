using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class StockCountView
    {
        public int StockCountId { get; set; }
        public DateTime StockDate { get; set; }
        public string StockDateString { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string MainGroup { get; set; }
        public double ProductQty { get; set; }
        public string BatchCode { get; set; }
        public bool IsClosed { get; set; }
    }

    public partial class StockCountJsonView
    {
        public int DataId { get; set; }
        public DateTime StockDate { get; set; }
        public string StockDateString { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public double ProductQty { get; set; }
        public string BatchCode { get; set; }
    }
}
