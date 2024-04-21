using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Expense
    {
        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public string? ExpenseCode { get; set; }
        public double? Price { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
