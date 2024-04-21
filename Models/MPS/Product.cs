using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Product
    {
        public int Id { get; set; }
        public string? Code { get; set; } = null!;
        public string? Desc { get; set; }
        public bool? Inactive { get; set; }
        public int? MinorGroupId { get; set; }
        public string? Unit { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Tax { get; set; }
        public double? Weight { get; set; }
        public double? Space { get; set; }
        public int? LifeTime { get; set; }
        public string? Comment { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public string? File { get; set; }
    }
}
