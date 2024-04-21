using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.PO
{
    public partial class PoformTemplate
    {
        public int FormId { get; set; }
        public int TemplateOrder { get; set; }
        public string? TemplateName { get; set; }
        public string? IssuedBy { get; set; }
        public string? RequiredBy { get; set; }
        public string? Supplier { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public string? PaymentMethod { get; set; }
        public bool? IsActivated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
