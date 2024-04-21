using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ExpenseDetailView
    {
        public int ExpenseId { get; set; }
        public int PeriodId { get; set; }
        public string ExpenseCode { get; set; }
        public string ExpenseDesc { get; set; }
        public double Price { get; set; }
        public bool IsClosed { get; set; }
    }
}
