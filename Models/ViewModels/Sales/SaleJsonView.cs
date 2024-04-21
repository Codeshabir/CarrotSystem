using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class SaleJsonView
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }

        public string CustomerCode { get; set; }
        public string CustomerDesc { get; set; }

    }

    public partial class ProductJsonView
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }

        public string CustomerCode { get; set; }
        public string CustomerDesc { get; set; }

        public int TaxId { get; set; }
        public string TaxName { get; set; }
    }
}
