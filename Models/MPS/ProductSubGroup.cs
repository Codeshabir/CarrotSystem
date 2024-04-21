using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductSubGroup
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public int? MainGroupId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
