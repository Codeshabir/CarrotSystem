using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductContPkg
    {
        public int Pk { get; set; }
        public short? PeriodId { get; set; }
        public string? ProductCode { get; set; }
        public string? Desc { get; set; }
        public string? State { get; set; }
        public double? SaleQty { get; set; }
        public double? SalePrice { get; set; }
        public double? Pc { get; set; }
        public double? Hf { get; set; }
        public double? Cont { get; set; }
        public double? Ytdsale { get; set; }
        public double? YtdsaleValue { get; set; }
        public double? Ytdcont { get; set; }
    }
}
