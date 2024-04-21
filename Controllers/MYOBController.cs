using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Globalization;

namespace CarrotSystem.Controllers
{
    //[Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class MYOBController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly IMYOBService _myob;
        private readonly ISystemService _system;
        private readonly IEventWriter _logger;
        
        public string eventBy = "MYOB";

        public MYOBController(IEventWriter logger, IContextService context, ISystemService system, IAPIService api, IMYOBService myob)
        {
            _logger = logger;
            _context = context;
            _system = system;
            _api = api;
            _myob = myob;
        }

        public IActionResult ExportTCC()
        {
            ViewMYOB viewModel = new ViewMYOB();

            List<PeriodicView> periodList = new List<PeriodicView>();
            periodList = _api.GetPeriodicList(DateTime.Now, "Export");

            viewModel.focusDate = _api.GetLateUpdateDate("Export");
            viewModel.focusId = _api.GetLatePeriodId("Export");
            viewModel.periodList = periodList;

            return View(viewModel);
        }

        public IActionResult GetExportResponseList(string target)
        {
            var targetDate = DateTime.ParseExact(target, "ddMMyyyy", CultureInfo.InvariantCulture);

            var period = _api.GetLatePeriod(targetDate); 
            var isClosed = _api.IsClosedPeriodByDate(targetDate);

            ViewMYOB detailView = new ViewMYOB();
            List<Myoblog> myobLogList = new List<Myoblog>();

            if (_context.GetMPSContext().Myoblog.Any(w => w.PeriodId.HasValue && w.PeriodId.Equals(period.Id)))
            {
                myobLogList = _context.GetMPSContext().Myoblog.Where(w => w.PeriodId.HasValue && w.PeriodId.Equals(period.Id)).ToList();
            }

            detailView.responseLogList = myobLogList;
            return PartialView("_ExportResponseDetails", detailView);
        }

        public IActionResult GetExportSalesList(string target)
        {
            var targetDate = DateTime.ParseExact(target, "ddMMyyyy", CultureInfo.InvariantCulture);

            var period  = _api.GetLatePeriod(targetDate);
            var isClosed = _api.IsClosedPeriodByDate(targetDate);

            ViewMYOB detailView = new ViewMYOB();
            List<ExportDetailView> expSaleList = new List<ExportDetailView>();

            var salesList = _context.GetMPSContext().Sale.Where(w=>(!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();
            if(salesList.Count > 0)
            {
                foreach(var sale in salesList)
                {
                    List<SaleItem> saleItemList = new List<SaleItem>();

                    saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Value.Equals(sale.InvoiceId)).ToList();

                    foreach(var saleItem in saleItemList)
                    {
                        ExportDetailView newDetail = new ExportDetailView();

                        newDetail.InvoiceID = saleItem.InvoiceId.Value;
                        newDetail.DateBy = sale.DeliveryDate.Value;
                        newDetail.CoLastName = sale.Company;
                        newDetail.ItemNO = saleItem.ProductCode;
                        newDetail.Qty = saleItem.InvoicedQty.Value;
                        newDetail.Price = saleItem.Price.Value;
                        newDetail.TaxCode = saleItem.Tax;
                        
                        expSaleList.Add(newDetail);
                    }
                }
            }

            detailView.exportSalesList = expSaleList;
            return PartialView("_ExportSalesDetails", detailView);
        }

        public IActionResult GetExportPurchasesList(string target)
        {
            var targetDate = DateTime.ParseExact(target, "ddMMyyyy", CultureInfo.InvariantCulture);

            var period = _api.GetLatePeriod(targetDate);
            var isClosed = _api.IsClosedPeriodByDate(targetDate);

            ViewMYOB detailView = new ViewMYOB();
            List<ExportDetailView> expSaleList = new List<ExportDetailView>();

            var purchaseList = _context.GetMPSContext().Purchase.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();
            if (purchaseList.Count > 0)
            {
                foreach (var purchase in purchaseList)
                {
                    List<PurchaseItem> purchaseItemList = new List<PurchaseItem>();

                    purchaseItemList = _context.GetMPSContext().PurchaseItem.Where(w => w.InvoiceId.Value.Equals(purchase.InvoiceId)).ToList();

                    foreach (var purchaseItem in purchaseItemList)
                    {
                        ExportDetailView newDetail = new ExportDetailView();

                        newDetail.InvoiceID = purchaseItem.InvoiceId.Value;
                        newDetail.DateBy = purchase.DeliveryDate.Value;
                        newDetail.CoLastName = purchase.Company;
                        newDetail.ItemNO = purchaseItem.ProductCode;
                        newDetail.Qty = purchaseItem.InvoicedQty.Value;
                        newDetail.Price = purchaseItem.Price.Value;
                        newDetail.TaxCode = purchaseItem.Tax;

                        expSaleList.Add(newDetail);
                    }
                }
            }

            detailView.exportPurchasesList = expSaleList;
            return PartialView("_ExportPurchasesDetails", detailView);

        }

        //ClearRecords
        [HttpPost]
        public IActionResult ClearRecords(string type, string exportDate)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            _myob.ClearRecords(type, exportDate);

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult ExportToMYOB(string type, string exportDate)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            return new JsonResult(_myob.ExportToMYOB(type, exportDate, userName));
        }

        [HttpPost]
        public IActionResult GetExportCount(string type, string exportDate)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            return new JsonResult(_myob.GetExportCount(type, exportDate));
        }

        public IActionResult MYOBtoDB()
        {
            _logger.WriteTestLog("Sync MYOB to DB Started");
            
            _myob.SyncMYOBtoDB("All");

            return RedirectToAction("ExportTCC");
        }

        //End
    }
}
