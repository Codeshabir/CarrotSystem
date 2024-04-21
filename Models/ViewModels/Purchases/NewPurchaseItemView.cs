using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class NewPurchaseItemView
    {
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyDesc { get; set; }
        public string Tax { get; set; }
    }
}
