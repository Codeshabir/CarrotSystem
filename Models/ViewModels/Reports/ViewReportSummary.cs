using CarrotSystem.Models.MPS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewReportSummary
    {
        public ViewReportSummary()
        {   
        }

        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }

        public string? dateForm { get; set; }

        public string? stringDateFrom { get; set; }
        public string? stringDateTo { get; set; }
        
        public string? pageTitle { get; set; }
        public string? pageType { get; set; }

        public string? companyColor { get; set; }
        public string? companyName { get; set; }

        public List<EmailGroup> emailList { get; set; }

        public List<PurchaseView> purchaseList { get; set; }
        public List<SalesView> saleList { get; set; }
        public List<PackingView> packingList { get; set; }
        public List<WasteView> wasteList { get; set; }
        public List<ExpenseView> expenseList { get; set; }
        public List<DispatchView> dispatchList { get; set; }

        public List<RawProduceView> rawProduceList { get; set; }
    }
}
