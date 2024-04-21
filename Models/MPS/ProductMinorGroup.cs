using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class ProductMinorGroup
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public int? SubGroupId { get; set; }
        public string? Prefix { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
