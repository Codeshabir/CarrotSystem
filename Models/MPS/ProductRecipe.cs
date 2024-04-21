using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductRecipe
    {
        public int Id { get; set; }
        public string? ProductCode { get; set; }
        public string? Component { get; set; }
        public double? Qty { get; set; }
        public double? OldQty { get; set; }
        public bool? Embeded { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
