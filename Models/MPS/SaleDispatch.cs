using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class SaleDispatch
    {
        public int DispatchId { get; set; }
        public string? DispatchName { get; set; }
        public int? SaleInvoiceId { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
