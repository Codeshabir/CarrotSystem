using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class TransferJsonView
    {
        public int DataId { get; set; }
        public bool IsClosed { get; set; }
        public int InvoiceId { get; set; }
        public DateTime TransferDate { get; set; }
        public string TransferDateString { get; set; }
        public string FromProduct { get; set; }
        public string FromProductId { get; set; }
        public double FromQty { get; set; }
        public string ToProduct { get; set; }
        public string ToProductId { get; set; }
        public double ToQty { get; set; }
        public double Price { get; set; }
    }
}
