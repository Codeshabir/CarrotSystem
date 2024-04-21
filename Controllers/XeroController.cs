using CarrotSystem.IO;
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace CarrotSystem.Controllers
{
    public class XeroController : ApiAccessorController<AccountingApi>
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly ISystemService _system;
        private readonly IEventWriter _logger;

        public XeroController(IEventWriter logger, IContextService context, ISystemService system, IAPIService api, IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig)
        {
            _logger = logger;
            _context = context;
            _system = system;
            _api = api;
            _xeroConfig = xeroConfig;
        }

        public IActionResult ExportToXeroList()
        {
            ViewXero viewModel = new ViewXero();

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

            ViewXero detailView = new ViewXero();
            List<XeroLog> myobLogList = new List<XeroLog>();

            if (_context.GetMPSContext().XeroLog.Any(w => w.PeriodId.HasValue && w.PeriodId.Equals(period.Id)))
            {
                myobLogList = _context.GetMPSContext().XeroLog.Where(w => w.PeriodId.HasValue && w.PeriodId.Equals(period.Id)).ToList();
            }

            detailView.responseLogList = myobLogList;
            return PartialView("_ExportResponseDetails", detailView);
        }

        public IActionResult GetExportSalesList(string target)
        {
            var targetDate = DateTime.ParseExact(target, "ddMMyyyy", CultureInfo.InvariantCulture);

            var period = _api.GetLatePeriod(targetDate);
            var isClosed = _api.IsClosedPeriodByDate(targetDate);

            ViewXero detailView = new ViewXero();
            List<ExportDetailView> expSaleList = new List<ExportDetailView>();

            var salesList = _context.GetMPSContext().Sale.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();
            if (salesList.Count > 0)
            {
                foreach (var sale in salesList)
                {
                    List<SaleItem> saleItemList = new List<SaleItem>();

                    saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Value.Equals(sale.InvoiceId)).ToList();

                    foreach (var saleItem in saleItemList)
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

            ViewXero detailView = new ViewXero();
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

        [HttpPost]
        public async Task<IActionResult> ExportToXero(string type, string exportDate)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (type.Equals("Sales"))
            {
                var exportType = type;

                var targetDate = DateTime.ParseExact(exportDate, "ddMMyyyy", CultureInfo.InvariantCulture);
                var period = _api.GetLatePeriod(targetDate);
                var isClosed = _api.IsClosedPeriodByDate(targetDate);

                List<ExportDetailView> expSaleList = new List<ExportDetailView>();

                var salesList = _context.GetMPSContext().Sale.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

                if (salesList.Count > 0)
                {
                    foreach (var sale in salesList)
                    {
                        List<string> errorList = new List<string>();

                        var contact = new Contact { Name = sale.Company };

                        var lines = new List<LineItem>();

                        List<SaleItem> saleItemList = new List<SaleItem>();
                        saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Value.Equals(sale.InvoiceId)).ToList();

                        foreach (var saleItem in saleItemList)
                        {
                            var line = new LineItem();

                            if (string.IsNullOrEmpty(saleItem.ProductDesc))
                            {
                                line.Description = saleItem.CompanyDesc;
                            }
                            else
                            {
                                line.Description = saleItem.ProductDesc;
                            }

                            line.Quantity = (decimal)saleItem.InvoicedQty.Value;
                            line.UnitAmount = (decimal)saleItem.Price.Value;
                            line.AccountCode = saleItem.Job;

                            lines.Add(line);
                        }

                        var invoice = new Invoice
                        {
                            Type = Invoice.TypeEnum.ACCREC,
                            Contact = contact,
                            Date = sale.DeliveryDate,
                            DueDate = DateTime.Today.AddDays(30),
                            LineItems = lines
                        };

                        var invoices = new Invoices
                        {
                            _Invoices = new List<Invoice> { invoice }
                        };

                        if(await Api.CreateInvoicesAsync(XeroToken.AccessToken, TenantId, invoices) != null)
                        {
                            XeroLog newXeroLog = new XeroLog();

                            newXeroLog.Target = "TCC";
                            newXeroLog.PeriodId = period.Id;
                            newXeroLog.InvoiceId = sale.InvoiceId;
                            newXeroLog.Type = "Sale";
                            newXeroLog.Result = "Success";
                            newXeroLog.ErrorNumber = "";
                            newXeroLog.ErrorDescription = "";
                            newXeroLog.ExportedOn = DateTime.Now;
                            newXeroLog.ExportedBy = userName;

                            _context.GetMPSContext().XeroLog.Add(newXeroLog);
                            _context.GetMPSContext().SaveChanges();

                            //sale.Status = "Exported";
                            sale.UpdatedBy = userName;
                            sale.UpdatedOn = DateTime.Now;

                            _context.GetMPSContext().Sale.Update(sale);
                            if (_context.GetMPSContext().SaveChanges() > 0)
                            {
                                _logger.WriteEvents(userName, "XERO", "Sales Exported", "Sales ID#" + sale.InvoiceId + " Exported [" + newXeroLog.Result + "]");
                            }
                        }
                        else
                        {
                            XeroLog newXeroLog = new XeroLog();

                            newXeroLog.Target = "TCC";
                            newXeroLog.PeriodId = period.Id;
                            newXeroLog.InvoiceId = sale.InvoiceId;
                            newXeroLog.Type = "Sale";
                            newXeroLog.Result = "Failed";
                            newXeroLog.ErrorNumber = "";
                            newXeroLog.ErrorDescription = "Failed";
                            newXeroLog.ExportedOn = DateTime.Now;
                            newXeroLog.ExportedBy = userName;

                            _context.GetMPSContext().XeroLog.Add(newXeroLog);
                            _context.GetMPSContext().SaveChanges();

                            //sale.Status = "Exported";
                            sale.UpdatedBy = userName;
                            sale.UpdatedOn = DateTime.Now;

                            _context.GetMPSContext().Sale.Update(sale);
                            if (_context.GetMPSContext().SaveChanges() > 0)
                            {
                                _logger.WriteEvents(userName, "XERO", "Sales Exported", "Sales ID#" + sale.InvoiceId + " Exported [" + newXeroLog.Result + "]");
                            }
                        }
                    }
                }
            }
            else if (type.Equals("Purchases"))
            {
                var exportType = type;

                var targetDate = DateTime.ParseExact(exportDate, "ddMMyyyy", CultureInfo.InvariantCulture);
                var period = _api.GetLatePeriod(targetDate);
                var isClosed = _api.IsClosedPeriodByDate(targetDate);

                List<ExportDetailView> expSaleList = new List<ExportDetailView>();

                var purchaseList = _context.GetMPSContext().Purchase.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

                if (purchaseList.Count > 0)
                {
                    var purchaseOrders = new PurchaseOrders();
                    var purchaseOrderList = new List<PurchaseOrder>();

                    foreach (var purchase in purchaseList)
                    {
                        List<PurchaseItem> purchaseItemList = new List<PurchaseItem>();
                        purchaseItemList = _context.GetMPSContext().PurchaseItem.Where(w => w.InvoiceId.Value.Equals(purchase.InvoiceId)).ToList();

                        var company = _context.GetMPSContext().Company.First(x => x.CompanyName == purchase.Company);

                        var lines = new List<LineItem>();

                        foreach (var purchaseItem in purchaseItemList)
                        {
                            var line = new LineItem();

                            line.Description = purchaseItem.ProductCode;
                            line.Quantity = (decimal)purchaseItem.InvoicedQty.Value;
                            line.UnitAmount = (decimal)purchaseItem.Price.Value;
                            line.AccountCode = purchaseItem.ProductCode;

                            lines.Add(line);
                        }

                        var purchaseOrder = new PurchaseOrder
                        {
                            Contact = new Contact { ContactID = Guid.Parse(await GetContactIDAsync(company)) },
                            Date = purchase.DeliveryDate,
                            DeliveryDate = DateTime.Today.AddDays(30),
                            LineAmountTypes = LineAmountTypes.Exclusive,
                            LineItems = lines
                        };

                        purchaseOrderList.Add(purchaseOrder);

                        XeroLog newXeroLog = new XeroLog();

                        newXeroLog.Target = "TCC";
                        newXeroLog.PeriodId = period.Id;
                        newXeroLog.InvoiceId = purchase.InvoiceId;
                        newXeroLog.Type = "Purchase";
                        newXeroLog.Result = "Success";
                        newXeroLog.ErrorNumber = "";
                        newXeroLog.ErrorDescription = "";
                        newXeroLog.ExportedOn = DateTime.Now;
                        newXeroLog.ExportedBy = userName;

                        _context.GetMPSContext().XeroLog.Add(newXeroLog);
                        _context.GetMPSContext().SaveChanges();

                        //purchase.Status = "Exported";
                        purchase.UpdatedBy = userName;
                        purchase.UpdatedOn = DateTime.Now;

                        _context.GetMPSContext().Purchase.Update(purchase);
                        if (_context.GetMPSContext().SaveChanges() > 0)
                        {
                            _logger.WriteEvents(userName, "XERO", "Purchase Exported", "Purchase ID#" + purchase.InvoiceId + " Exported [" + newXeroLog.Result + "]");
                        }
                    }

                    purchaseOrders._PurchaseOrders = purchaseOrderList;

                    await Api.CreatePurchaseOrdersAsync(XeroToken.AccessToken, TenantId, purchaseOrders);

                }
            }

            return RedirectToAction("ExportToXeroList");
        }

        [HttpPost]
        public IActionResult GetExportCount(string type, string exportDate)
        {
            var maxWidth = 1000;

            var targetDate = DateTime.ParseExact(exportDate, "ddMMyyyy", CultureInfo.InvariantCulture);
            var period = _api.GetLatePeriod(targetDate);

            if (type.Equals("Sales"))
            {
                maxWidth = _context.GetMPSContext().Sale.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList().Count();
            }
            else if (type.Equals("Purchases"))
            {
                maxWidth = _context.GetMPSContext().Purchase.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList().Count();
            }
            else
            {
                maxWidth = 1000;
            }

            return new JsonResult(maxWidth);
        }


        public IActionResult Index([FromQuery] Guid? tenantId)
        {
            var tokenIO = LocalStorageTokenIO.Instance;
            if (tenantId != null) tokenIO.StoreTenantId(tenantId.ToString());

            return View(tokenIO.TokenExists());
        }

        public async Task<string> GetContactIDAsync(Company company)
        {
            var response = await Api.GetContactsAsync(XeroToken.AccessToken, TenantId);

            if(response._Contacts.Any(x=>x.Name == company.CompanyName))
            {
                return response._Contacts.First(x => x.Name == company.CompanyName).ContactID.ToString();
            }
            else
            {
                var contact = new Contact();

                contact.Name = company.CompanyName;
                
                if(company.Type == "Supplier")
                {
                    contact.IsSupplier = true;
                }
                else
                {
                    contact.IsCustomer = true;
                }


                var contacts = new Contacts { _Contacts = new List<Contact> { contact } };

                var taskContact = await Api.CreateContactsAsync(XeroToken.AccessToken, TenantId, contacts);

                return taskContact._Contacts.First(x => x.Name == company.CompanyName).ContactID.ToString();
            }
        }


    }
}
