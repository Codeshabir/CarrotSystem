using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductContGgp
    {
        public int Pk { get; set; }
        public short? PeriodId { get; set; }
        public string? ProductCode { get; set; }
        public string? Desc { get; set; }
        public double? Qty { get; set; }
        public double? Pc { get; set; }
        public double? Lc { get; set; }
        public double? Lr { get; set; }
        public double? Pf { get; set; }
        public double? Cont { get; set; }
        public double? Ytdcont { get; set; }
    }
}
