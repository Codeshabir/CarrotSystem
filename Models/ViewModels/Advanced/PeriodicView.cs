using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class PeriodicView
    {
        public int PeriodId { get; set; }
        public int TargetId { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; }
        
    }
}
