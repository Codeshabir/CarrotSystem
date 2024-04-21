using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class WasteView
    {
        public int WasteId { get; set; }
        public bool IsClosed { get; set; }
        public DateTime WasteDate { get; set; }
        public string WasteDateString { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public int ProductQty { get; set; }
        public int SupplierPK { get; set; }
        public string Supplier { get; set; }
        public string Reason { get; set; }
    }

    public partial class WasteJsonView
    {
        public int DataId { get; set; }
        public DateTime WasteDate { get; set; }
        public string WasteDateString { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public int ProductQty { get; set; }
        public string Supplier { get; set; }
        public int SupplierId { get; set; }
        public string Reason { get; set; }
    }
}
