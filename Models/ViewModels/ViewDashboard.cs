using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewDashboard
    {
        public ViewDashboard()
        {   
        }

        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }

        public string? stringDateFrom { get; set; }
        public string? stringDateTo { get; set; }
        
        public string? dateForm { get; set; }
        public string? status { get; set; }

        public string? btnStatus { get; set; }
        public string? summaryType { get; set; }

        public string? pageTitle { get; set; }
        public string? companyColor { get; set; }
        public string? companyName { get; set; }

        public string? produceDateForm { get; set; }

        public List<ReportData>? dataList { get; set; }
        public List<ReportSummary>? summaryList { get; set; }
    }
}
