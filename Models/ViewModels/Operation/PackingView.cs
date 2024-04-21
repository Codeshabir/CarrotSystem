using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class PackingView
    {
        public int ItemPK { get; set; }
        public bool IsClosed { get; set; }
        public int PackingId { get; set; }
        public DateTime PackingDate { get; set; }
        public string PackingDateString { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public int ProductQty { get; set; }
        public DateTime BestBefore { get; set; }
        public string BestBeforeString { get; set; }
        public int SupplierPK { get; set; }
        public string Supplier { get; set; }
        public string PackingDesc { get; set; }
    }

    public partial class PackingJsonView
    {
        public int DataId { get; set; }
        public DateTime PackingDate { get; set; }
        public string PackingDateString { get; set; }
        public string ProductId { get; set; }
        public string ProductDesc { get; set; }
        public int ProductQty { get; set; }
        public DateTime BestBefore { get; set; }
        public string BestBeforeString { get; set; }
        public string SupplierId { get; set; }
        public string PackingDesc { get; set; }
    }

}
