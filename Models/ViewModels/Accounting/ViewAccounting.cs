using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewAccounting
    {
        public List<PeriodicView> periodList { get; set; }
        
        public int targetId { get; set; }
        public int lastPeriodId { get; set; }

        public string dataId { get; set; }
    }
}
