
using CarrotSystem.Helpers;
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarrotSystem.Controllers
{
    [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class SalesController : Controller
    {
        private readonly IContextService _context;
        private readonly IEventWriter _logger;
        private readonly IAPIService _api;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IGenPDFService _pdfService;
        private string eventType = "Sales";
        private IEmailService _emailService;

        public SalesController(IEventWriter logger, IContextService context, IWebHostEnvironment hostingEnvironment, IGenPDFService pdfService, IAPIService api, IEmailService emailService)
        {
            _logger = logger;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _pdfService = pdfService;
            _api = api;
            _emailService = emailService;
        }

        public IActionResult SalesList()
        {
            ViewSales viewModel = new ViewSales();

            viewModel.dateFrom = _context.GetDateByNow(-15);
            viewModel.dateTo = DateTime.Now.AddDays(7).Date;

            List<EmailGroup> emailList = new List<EmailGroup>();
            emailList = _context.GetMPSContext().EmailGroup.ToList();

            string status = "All";
            string customer = "All";
            string isShowAll = "No";

            List<Company> customerList = new List<Company>();
            customerList = _context.GetMPSContext().Company.Where(w => w.Type.Equals("Customer") || w.Type.Equals("CustSup")).ToList();

            List<SalesView> invList = new List<SalesView>();

            viewModel.invoiceList = GetInvoiceList(status, customer, viewModel.dateFrom, viewModel.dateTo, isShowAll);

            viewModel.isShowAll = isShowAll;
            viewModel.customerList = customerList;
            viewModel.emailList = emailList;
            viewModel.stringStatus = status;
            viewModel.stringCustomer = customer;

            //_logger.WriteTestLog("TCC Invoice List, From : " + viewModel.dateFrom.ToString("dd/MM/yyyy") + " to " + viewModel.dateTo.ToString("dd/MM/yyyy"));

            return View(viewModel);
        }

        // POST: Selected Invoice List
        [HttpPost]
        public IActionResult SelectedSalesList(ViewSales selectedModel)
        {
            ViewSales viewModel = new ViewSales();

            viewModel.dateFrom = DateTime.ParseExact(selectedModel.stringDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            viewModel.dateTo = DateTime.ParseExact(selectedModel.stringDateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            List<EmailGroup> emailList = new List<EmailGroup>();
            emailList = _context.GetMPSContext().EmailGroup.ToList();

            string status = selectedModel.stringStatus;
            string customer = selectedModel.stringCustomer;
            string isShowAll = selectedModel.isShowAll;

            List<Company> customerList = new List<Company>();
            customerList = _context.GetMPSContext().Company.Where(w => w.Type.Equals("Customer") || w.Type.Equals("CustSup")).ToList();

            List<SalesView> invList = new List<SalesView>();

            viewModel.invoiceList = GetInvoiceList(status, customer, viewModel.dateFrom, viewModel.dateTo, isShowAll);

            viewModel.isShowAll = isShowAll;
            viewModel.customerList = customerList;
            viewModel.emailList = emailList;
            viewModel.stringStatus = status;
            viewModel.stringCustomer = customer;

            //_logger.WriteTestLog("TCC Invoice List, From : " + viewModel.dateFrom.ToString("dd/MM/yyyy") + " to " + viewModel.dateTo.ToString("dd/MM/yyyy"));


            return View("SalesList", viewModel);
        }

        public IActionResult SalesDetails(string id)
        {
            ViewSales viewModel = new ViewSales();

            List<EmailGroup> emailList = new List<EmailGroup>();
            emailList = _context.GetMPSContext().EmailGroup.ToList();
            viewModel.emailList = emailList;

            SalesView saleView = new SalesView();
            List<SaleItemView> invItemList = new List<SaleItemView>();
            List<Company> custList = _api.GetCompanyList("Customer", "All");
            List<CustomisedProductItemModel> productList = _api.GetCustomisedProductList("Active", "Sales");
            List<Address> shipList = new List<Address>();
            List<SaleType> typeList = new List<SaleType>();
            List<SaleDispatch> dispatchList = new List<SaleDispatch>();
            List<ClaimRefItem> claimRefList = new List<ClaimRefItem>();
            typeList = _context.GetMPSContext().SaleType.ToList();

            if (id.Equals("New"))
            {
                viewModel.isNew = true;
                saleView.Status = "New";
                saleView.invTotal = _api.GetInvoiceTotal(0);
                viewModel.isClosed = false;
            }
            else
            {
                viewModel.isNew = false;

                saleView = _api.GetInvoice(int.Parse(id), "Shipping");
                invItemList = _api.GetInvoiceItemList(saleView.InvoiceId);
                shipList = _context.GetMPSContext().Address.Where(w => w.Company.Equals(saleView.Customer) && w.Type.Equals("Shipping")).ToList();

                if (saleView.Type.Contains("Claim"))
                {
                    claimRefList = _api.GetClaimRefList("Sales");
                }

                viewModel.isClosed = _api.IsClosedPeriodByDate(saleView.ArrivalDate);
            }

            viewModel.claimRefList = claimRefList;
            viewModel.dispatchList = dispatchList;
            viewModel.typeList = typeList;
            viewModel.productList = productList;
            viewModel.shipList = shipList;
            viewModel.customerList = custList;
            viewModel.saleView = saleView;
            viewModel.invoiceItemList = invItemList;

            return View("SalesDetails", viewModel);
        }

        [HttpPost]
        public IActionResult AddSaleItem(ViewSales viewModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            DateTime timeSync = DateTime.Now;
            var invId = viewModel.itemView.InvoiceId;

            SaleItem newItem = new SaleItem();
            Product product = new Product();

            if (viewModel.itemView.ProductId.HasValue)
            {
                product = _context.GetMPSContext().Product.Where(w => w.Id.Equals(viewModel.itemView.ProductId)).First();
            }
            else
            {
                product = _context.GetMPSContext().Product.Where(w => w.Id.Equals(viewModel.itemView.CustProductId)).First();
            }

            if (_context.GetMPSContext().ProductMapping.Any(a => a.MercCode.Equals(product.Code)))
            {
                var mapping = _context.GetMPSContext().ProductMapping.First(a => a.MercCode.Equals(product.Code));

                newItem.CompanyCode = mapping.CompanyCode;
                newItem.CompanyDesc = mapping.CompanyDesc;
            }
            else
            {
                newItem.CompanyCode = "N/A";
                newItem.CompanyDesc = "N/A";
            }

            newItem.InvoiceId = invId;
            newItem.SortId = viewModel.itemView.SortID;

            newItem.ProductCode = product.Code;
            newItem.ProductDesc = product.Desc;

            newItem.InvoicedQty = viewModel.itemView.InvoicedQty;
            newItem.Price = viewModel.itemView.Price;
            newItem.Tax = viewModel.itemView.Tax;
            newItem.Job = viewModel.itemView.Job;
            newItem.FreightProportion = viewModel.itemView.FreightProportion;

            newItem.UpdatedOn = timeSync;
            newItem.UpdatedBy = userName;

            _context.GetMPSContext().SaleItem.Add(newItem);

            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                newItem = _context.GetMPSContext().SaleItem.Where(w => w.UpdatedOn.Equals(timeSync)).First();
                _logger.WriteEvents(userName, "Sale", "Added Item", "Sale Item [" + newItem.ProductCode + "] [" + newItem.Price + " @ " + newItem.InvoicedQty + "] Added");
            }

            invId = newItem.InvoiceId.Value;

            return RedirectToAction("SalesDetails", new { id = invId });
        }

        public IActionResult DuplicateSales(string id)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            DateTime timeSync = DateTime.Now;

            if (!string.IsNullOrEmpty(id))
            {
                var invId = int.Parse(id);

                var baseInv = _context.GetMPSContext().Sale.Where(w => w.InvoiceId.Equals(invId)).First();

                Sale newSale = new Sale();

                newSale.Company = baseInv.Company;

                if (baseInv.Revision.HasValue)
                {
                    newSale.Revision = baseInv.Revision.Value;
                }

                newSale.ShippingAddress = baseInv.ShippingAddress;

                newSale.Status = "Order";
                newSale.Type = baseInv.Type;

                if (baseInv.ClaimReference.HasValue)
                {
                    newSale.ClaimReference = baseInv.ClaimReference.Value;
                }

                newSale.UpdatedOn = timeSync;
                newSale.UpdatedBy = userName;
                newSale.ArrivalDate = baseInv.ArrivalDate;
                newSale.DeliveryDate = baseInv.DeliveryDate;

                _context.GetMPSContext().Sale.Add(newSale);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    newSale = _context.GetMPSContext().Sale.Where(w => w.UpdatedOn.Equals(timeSync)).First();
                    _logger.WriteEvents(userName, "Sale", "Added", "Sale ID#" + newSale.InvoiceId + " Added Copy From ID#" + baseInv.InvoiceId);
                }

                if (_context.GetMPSContext().SaleItem.Any(x => x.InvoiceId.HasValue && x.InvoiceId.Value.Equals(invId)))
                {
                    List<SaleItem> itemList = new List<SaleItem>();
                    itemList = _context.GetMPSContext().SaleItem.Where(x => x.InvoiceId.HasValue && x.InvoiceId.Value.Equals(invId)).ToList();

                    int sortNo = 1;

                    foreach (var item in itemList)
                    {
                        SaleItem newItem = new SaleItem();
                        newItem.InvoiceId = newSale.InvoiceId;
                        newItem.ProductCode = item.ProductCode;
                        newItem.ProductDesc = item.ProductDesc;
                        newItem.SortId = sortNo++;
                        newItem.Job = item.Job;
                        newItem.Price = item.Price;
                        newItem.InvoicedQty = 0;
                        newItem.Tax = item.Tax;

                        newItem.CompanyCode = item.CompanyCode;
                        newItem.CompanyDesc = item.CompanyDesc;

                        newItem.UpdatedOn = DateTime.Now;
                        newItem.UpdatedBy = userName;

                        _context.GetMPSContext().SaleItem.Add(newItem);
                        if (_context.GetMPSContext().SaveChanges() > 0)
                        {
                            _logger.WriteEvents(userName, "Sale", "Added Item", "Sale Item [" + newItem.ProductCode + "] [" + newItem.Price + " @ " + newItem.InvoicedQty + "] Copied from Item ID#" + item.Id);
                        }
                    }
                }

                return RedirectToAction("SalesDetails", new { id = newSale.InvoiceId });
            }
            else
            {
                return RedirectToAction("SalesList");
            }

        }

        public List<SalesView> GetInvoiceList(string status, string customer, DateTime dateFrom, DateTime dateTo, string isShowAll)
        {
            List<SalesView> returnList = new List<SalesView>();
            List<Sale> saleList = new List<Sale>();

            if (status.Equals("All") && customer.Equals("All"))
            {
                saleList = _context.GetMPSContext().Sale.Where(w => w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0).OrderByDescending(o => o.DeliveryDate.Value.Date).ToList();

                foreach (var sale in saleList)
                {
                    if (isShowAll.Equals("Yes"))
                    {
                        SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                        returnList.Add(invoice);
                    }
                    else
                    {
                        if (!sale.Status.Equals("Deleted"))
                        {
                            SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                            returnList.Add(invoice);
                        }
                    }
                }
            }
            else if (status.Equals("All") && (!customer.Equals("All")))
            {
                saleList = _context.GetMPSContext().Sale.Where(w => w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0 && w.Company.Equals(customer)).OrderByDescending(o => o.DeliveryDate.Value.Date).ToList();

                foreach (var sale in saleList)
                {
                    if (isShowAll.Equals("Yes"))
                    {
                        SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                        returnList.Add(invoice);
                    }
                    else
                    {
                        if (!sale.Status.Equals("Deleted"))
                        {
                            SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                            returnList.Add(invoice);
                        }
                    }
                }
            }
            else if ((!status.Equals("All")) && customer.Equals("All"))
            {
                saleList = _context.GetMPSContext().Sale.Where(w => w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0 && w.Status.Equals(status)).OrderByDescending(o => o.DeliveryDate.Value.Date).ToList();

                foreach (var sale in saleList)
                {
                    if (isShowAll.Equals("Yes"))
                    {
                        SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                        returnList.Add(invoice);
                    }
                    else
                    {
                        if (!sale.Status.Equals("Deleted"))
                        {
                            SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                            returnList.Add(invoice);
                        }
                    }
                }
            }
            else
            {
                saleList = _context.GetMPSContext().Sale.Where(w => w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0 && w.Status.Equals(status) && w.Company.Equals(customer)).OrderByDescending(o => o.DeliveryDate.Value.Date).ToList();

                foreach (var sale in saleList)
                {
                    if (isShowAll.Equals("Yes"))
                    {
                        SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                        returnList.Add(invoice);
                    }
                    else
                    {
                        if (!sale.Status.Equals("Deleted"))
                        {
                            SalesView invoice = _api.GetInvoice(sale.InvoiceId, "Shipping");
                            returnList.Add(invoice);
                        }
                    }
                }
            }

            return returnList;
        }

        [HttpPost]
        public IActionResult EditSales(ViewSales viewModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            var status = viewModel.saleView.Status;
            var type = viewModel.saleView.Type;

            _logger.WriteTestLog("Cust : " + viewModel.saleView.Customer);

            var company = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(int.Parse(viewModel.saleView.Customer))).First();
            var address = viewModel.saleView.AddId;
            var custpo = viewModel.saleView.CustPO;
            var invId = 0;
            var revision = 1;
            var deliveryDate = DateTime.ParseExact(viewModel.saleView.strDelDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var arrivalDate = DateTime.ParseExact(viewModel.saleView.strArrDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var claimRef = "";

            if (type.Contains("Claim"))
            {
                claimRef = viewModel.saleView.ClaimRef;
            }

            var updateOn = DateTime.Now;
            var updateBy = userName;

            if (status.Equals("New"))
            {
                Sale newSale = new Sale();

                newSale.Status = "Invoice";
                newSale.Type = type;
                newSale.Company = company.CompanyName;
                newSale.CompanyNo = custpo;
                newSale.ShippingAddress = address;
                newSale.DeliveryDate = deliveryDate;
                newSale.ArrivalDate = arrivalDate;
                newSale.Revision = revision;

                newSale.UpdatedOn = updateOn;
                newSale.UpdatedBy = updateBy;

                if (!string.IsNullOrEmpty(claimRef))
                {
                    newSale.ClaimReference = int.Parse(claimRef);
                }

                if (!string.IsNullOrEmpty(viewModel.saleView.Comment))
                {
                    newSale.Comment = viewModel.saleView.Comment;
                }

                _context.GetMPSContext().Sale.Add(newSale);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    invId = _context.GetMPSContext().Sale.Where(w => w.UpdatedOn.HasValue && w.UpdatedOn.Equals(updateOn)).First().InvoiceId;
                    _logger.WriteEvents(userName, "Sale", "Added", "Sale ID#" + newSale.InvoiceId + " Added");
                }

            }
            else
            {
                invId = viewModel.saleView.InvoiceId;

                Sale editSale = _context.GetMPSContext().Sale.Where(w => w.InvoiceId.Equals(invId)).First();

                editSale.Type = type;
                editSale.Company = company.CompanyName;
                editSale.CompanyNo = custpo;
                editSale.ShippingAddress = address;
                editSale.DeliveryDate = deliveryDate;
                editSale.ArrivalDate = arrivalDate;
                editSale.Revision = revision;

                editSale.UpdatedOn = updateOn;
                editSale.UpdatedBy = updateBy;

                if (string.IsNullOrEmpty(viewModel.saleView.Comment))
                {
                    editSale.Comment = "";
                }
                else
                {
                    editSale.Comment = viewModel.saleView.Comment;
                }

                if (!string.IsNullOrEmpty(claimRef))
                {
                    editSale.ClaimReference = int.Parse(claimRef);
                }

                _context.GetMPSContext().Sale.Update(editSale);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    invId = _context.GetMPSContext().Sale.Where(w => w.UpdatedOn.HasValue && w.UpdatedOn.Equals(updateOn)).First().InvoiceId;
                    _logger.WriteEvents(userName, "Sale", "Adjustment", "Sale ID#" + editSale.InvoiceId + " Updated");
                }

            }

            return RedirectToAction("SalesDetails", new { id = invId });
        }

        [HttpPost]
        public IActionResult ChangeItemTotal(string total)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            var itemId = int.Parse(total.Split('_')[0]);
            var qty = total.Split('_')[1];
            var price = total.Split('_')[2];

            var eventBy = _api.GetFullName(HttpContext.User.Identity.Name);
            var item = _context.GetMPSContext().SaleItem.Where(w => w.Id.Equals(itemId)).First();

            var originalQty = item.InvoicedQty;
            var originalPrice = item.Price.Value;

            item.InvoicedQty = double.Parse(qty);
            item.Price = double.Parse(price);

            _context.GetMPSContext().SaleItem.Update(item);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Sale", "Adjustment", "Sale Item ID#" + itemId + "[" + originalPrice + " @ " + originalQty + "] -> [" + item.Price + " @ " + item.InvoicedQty + "] Updated");
            }

            return new JsonResult("Success");
        }

        //DispatchAction
        [HttpPost]
        public IActionResult DispatchAction(ViewSales selectedModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            var distpatchActionType = selectedModel.dispatchActionType;
            var distpatchId = selectedModel.dispatchId;
            var title = " - The Carrot Company Pty Ltd";

            if (distpatchActionType.Equals("PickingList"))
            {
                return RedirectToAction("PrintPickingList", "Sales", new { distpatchId = distpatchId });
                //return RedirectToAction("PickingList", "Sales", new { dispatchId = distpatchId });
                //title = "Picking List" + title;
                //var pdfFile = _pdfService.GeneratePDF(title, distpatchActionType, distpatchId);

                //return new FileStreamResult(new MemoryStream(pdfFile), "application/pdf");
            }
            else if (distpatchActionType.Equals("LoadSheet"))
            {
                return RedirectToAction("LoadSheet", "Sales", new { dispatchId = distpatchId });
                //title = "Load Sheet" + title;
                //var pdfFile = _pdfService.GeneratePDF(title, distpatchActionType, distpatchId);
                //return new FileStreamResult(new MemoryStream(pdfFile), "application/pdf");
            }
            else if (distpatchActionType.Equals("DeliveryAdvice"))
            {
                return RedirectToAction("GetDeliveryAdvice", "Sales", new { dispatchId = distpatchId });
                //title = "Delivery Advice" + title;
                //var pdfFile = _pdfService.GeneratePDF(title, distpatchActionType, distpatchId);

                //return new FileStreamResult(new MemoryStream(pdfFile), "application/pdf");
            }
            else
            {
                return RedirectToAction("SalesDetails", new { id = selectedModel.invoiceId });
            }
        }

        [HttpGet]
        public IActionResult PrintPickingList(int distpatchId)
        {
            var dispatchInformation = _api.GetDispatchInformation(distpatchId);
            var dispatchList = _api.GetDispatchList(distpatchId);
            var result = new CarrotSystem.Models.ViewModels.PickingListViewModel()
            {
                DispatchView = dispatchInformation,
                DispatchViewList = dispatchList
            };

            return View(result);
        }

        [AllowAnonymous]
        public IActionResult ViewSales(int invoiceid)
        {
            var title = "MPS";
            var pdfFile = _pdfService.GeneratePDF(title, "Invoice", invoiceid);

            return new FileStreamResult(new MemoryStream(pdfFile), "application/pdf");
        }

        public IActionResult NewDispatch(int invId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            DateTime timeSync = DateTime.Now;

            DispatchViewModel viewModel = new DispatchViewModel();
            List<DispatchItemView> disItemList = new List<DispatchItemView>();
            List<SaleItem> saleItemList = new List<SaleItem>();

            saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Equals(invId)).ToList();
            var saleInv = _context.GetMPSContext().Sale.Where(x => x.InvoiceId.Equals(invId)).First();

            if (saleItemList.Count > 0)
            {
                SaleDispatch newDispatch = new SaleDispatch();

                newDispatch.SaleInvoiceId = invId;
                newDispatch.UpdatedBy = userName;
                newDispatch.UpdatedOn = timeSync;
                newDispatch.DispatchDate = saleInv.DeliveryDate.Value;

                _context.GetMPSContext().SaleDispatch.Add(newDispatch);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    newDispatch = _context.GetMPSContext().SaleDispatch.Where(w => w.UpdatedOn.Equals(timeSync)).First();
                    _logger.WriteEvents(userName, "Dispatch", "Added", "Dispatch ID#" + newDispatch.DispatchId + ", Sale ID# " + newDispatch.SaleInvoiceId + " Added");
                }

                foreach (var item in saleItemList)
                {
                    List<SaleDispatchItem> prvDisItem = new List<SaleDispatchItem>();
                    int preFilled = 0;

                    if (_context.GetMPSContext().SaleDispatchItem.Any(n => n.SaleItemId.Equals(item.Id)))
                    {
                        prvDisItem = _context.GetMPSContext().SaleDispatchItem.Where(w => w.SaleItemId.Equals(item.Id)).OrderBy(o => o.UpdatedOn).ToList();

                        foreach (var filled in prvDisItem)
                        {
                            preFilled = preFilled + filled.Qty.Value;
                        }
                    }
                    else
                    {
                        preFilled = 0;
                    }

                    var saleItem = _context.GetMPSContext().SaleItem.Where(w => w.Id.Equals(item.Id)).First();

                    DateTime itemSync = DateTime.Now;

                    SaleDispatchItem newDisItem = new SaleDispatchItem();
                    newDisItem.DispatchId = newDispatch.DispatchId;
                    newDisItem.SaleItemId = saleItem.Id;
                    newDisItem.Qty = 0;
                    newDisItem.UpdatedBy = userName;
                    newDisItem.UpdatedOn = itemSync;

                    _context.GetMPSContext().SaleDispatchItem.Add(newDisItem);
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        newDisItem = _context.GetMPSContext().SaleDispatchItem.Where(w => w.UpdatedOn.Equals(itemSync)).First();
                        _logger.WriteEvents(userName, "Dispatch", "Added Item", "Dispatch ID#" + newDisItem.DispatchId + " [" + saleItem.ProductCode + " @ " + newDisItem.Qty + " Added");
                    }

                    DispatchItemView newItem = new DispatchItemView();

                    newItem.DispatchId = newDispatch.DispatchId;
                    newItem.SaleItemId = item.Id;
                    newItem.SortId = (int)item.SortId.Value;
                    newItem.InvoiceId = item.InvoiceId.Value;
                    newItem.DispatchItemId = newDisItem.Id;
                    newItem.ProductCode = item.ProductCode;
                    newItem.ProductDesc = item.ProductDesc;
                    newItem.CustomerCode = item.CompanyCode;
                    newItem.CustomerDesc = item.CompanyDesc;
                    newItem.Ordered = (int)item.InvoicedQty.Value;
                    newItem.Unfilled = (newItem.Ordered - (float)preFilled);

                    disItemList.Add(newItem);
                }
            }

            viewModel.dispatchItemList = disItemList;

            return PartialView("_DispatchList", viewModel);
        }

        public IActionResult DispatchAll(int invId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            DateTime timeSync = DateTime.Now;

            DispatchViewModel viewModel = new DispatchViewModel();
            List<DispatchItemView> disItemList = new List<DispatchItemView>();
            List<SaleItem> saleItemList = new List<SaleItem>();

            saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Equals(invId)).ToList();
            var saleInv = _context.GetMPSContext().Sale.Where(x => x.InvoiceId.Equals(invId)).First();

            if (saleItemList.Count > 0)
            {
                SaleDispatch newDispatch = new SaleDispatch();

                newDispatch.SaleInvoiceId = invId;
                newDispatch.UpdatedBy = userName;
                newDispatch.UpdatedOn = timeSync;
                newDispatch.DispatchDate = saleInv.DeliveryDate.Value;

                _context.GetMPSContext().SaleDispatch.Add(newDispatch);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    newDispatch = _context.GetMPSContext().SaleDispatch.Where(w => w.UpdatedOn.Equals(timeSync)).First();
                    _logger.WriteEvents(userName, "Dispatch", "Added", "Dispatch ID#" + newDispatch.DispatchId + ", Sale ID# " + newDispatch.SaleInvoiceId + " Added");
                }

                foreach (var item in saleItemList)
                {
                    List<SaleDispatchItem> prvDisItem = new List<SaleDispatchItem>();
                    int preFilled = 0;

                    if (_context.GetMPSContext().SaleDispatchItem.Any(n => n.SaleItemId.Equals(item.Id)))
                    {
                        prvDisItem = _context.GetMPSContext().SaleDispatchItem.Where(w => w.SaleItemId.Equals(item.Id)).OrderBy(o => o.UpdatedOn).ToList();

                        foreach (var filled in prvDisItem)
                        {
                            preFilled = preFilled + filled.Qty.Value;
                        }
                    }
                    else
                    {
                        preFilled = 0;
                    }

                    var saleItem = _context.GetMPSContext().SaleItem.Where(w => w.Id.Equals(item.Id)).First();

                    DateTime itemSync = DateTime.Now;

                    SaleDispatchItem newDisItem = new SaleDispatchItem();
                    newDisItem.DispatchId = newDispatch.DispatchId;
                    newDisItem.SaleItemId = saleItem.Id;
                    newDisItem.Qty = (int)item.InvoicedQty.Value - preFilled;
                    newDisItem.UpdatedBy = userName;
                    newDisItem.UpdatedOn = itemSync;

                    _context.GetMPSContext().SaleDispatchItem.Add(newDisItem);
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        newDisItem = _context.GetMPSContext().SaleDispatchItem.Where(w => w.UpdatedOn.Equals(itemSync)).First();
                        _logger.WriteEvents(userName, "Dispatch", "Added Item", "Dispatch ID#" + newDisItem.DispatchId + " [" + saleItem.ProductCode + " @ " + newDisItem.Qty + " Added");
                    }

                    DispatchItemView newItem = new DispatchItemView();

                    newItem.DispatchId = newDispatch.DispatchId;
                    newItem.SaleItemId = item.Id;
                    newItem.SortId = (int)item.SortId.Value;
                    newItem.InvoiceId = item.InvoiceId.Value;
                    newItem.DispatchItemId = newDisItem.Id;
                    newItem.ProductCode = item.ProductCode;
                    newItem.ProductDesc = item.ProductDesc;
                    newItem.CustomerCode = item.CompanyCode;
                    newItem.CustomerDesc = item.CompanyDesc;
                    newItem.Ordered = (int)item.InvoicedQty.Value;
                    newItem.Unfilled = 0;

                    disItemList.Add(newItem);
                }
            }

            viewModel.dispatchItemList = disItemList;

            return PartialView("_DispatchList", viewModel);
        }

        public IActionResult GetDispatchList(int invId)
        {
            var isClosed = true;

            var sale = _context.GetMPSContext().Sale.Where(w => w.InvoiceId.Equals(invId)).First();

            List<SaleDispatch> dispatchList = new List<SaleDispatch>();

            dispatchList = _context.GetMPSContext().SaleDispatch.Where(w => w.SaleInvoiceId.HasValue && w.SaleInvoiceId.Value.Equals(invId)).ToList();

            if (dispatchList.Count > 0)
            {
                var dispatch = dispatchList.First();

                if (sale.DeliveryDate.HasValue)
                {
                    isClosed = _api.IsClosedPeriodByDate(sale.DeliveryDate.Value);
                }
                else if (sale.ArrivalDate.HasValue)
                {
                    isClosed = _api.IsClosedPeriodByDate(sale.ArrivalDate.Value);
                }
                else
                {
                    _logger.WriteTestLog("No Date Found");
                }
            }

            DispatchViewModel dispatchViewModel = new DispatchViewModel();

            dispatchViewModel.dispatchList = dispatchList;
            dispatchViewModel.isClosed = isClosed;

            return PartialView("_GetDispatchList", dispatchViewModel);
        }

        [HttpPost]
        public IActionResult ChangeDispatchItem(string type, int dataId, string chgVal)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (_context.GetMPSContext().SaleDispatchItem.Any(a => a.Id.Equals(dataId)))
            {
                var target = _context.GetMPSContext().SaleDispatchItem.Where(w => w.Id.Equals(dataId)).First();

                var originalValue = "";

                if (type.Equals("Qty"))
                {
                    originalValue = target.Qty.ToString();
                    target.Qty = int.Parse(chgVal);
                }

                target.UpdatedBy = userName;
                target.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().SaleDispatchItem.Update(target);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Dispatch", "Adjustment", "Dispatch Item ID#" + dataId + " [" + type + "] [" + originalValue + " -> " + chgVal + " Updated");
                }

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }
        }

        public IActionResult GetDispatchDetails(int dispatchId)
        {
            var isClosed = true;

            DispatchViewModel viewModel = new DispatchViewModel();

            if (_context.GetMPSContext().SaleDispatch.Any(w => w.DispatchId.Equals(dispatchId)))
            {
                var dispatch = _context.GetMPSContext().SaleDispatch.Where(w => w.DispatchId.Equals(dispatchId)).First();

                if (dispatch.DispatchDate.HasValue)
                {
                    isClosed = _api.IsClosedPeriodByDate(dispatch.DispatchDate.Value);
                }

                if (dispatch.ArrivalDate.HasValue)
                {
                    isClosed = _api.IsClosedPeriodByDate(dispatch.ArrivalDate.Value);
                }
            }

            List<DispatchItemView> disItemList = new List<DispatchItemView>();
            disItemList = _api.GetDispatchList(dispatchId);

            viewModel.isClosed = isClosed;
            viewModel.dispatchItemList = disItemList;

            return PartialView("_DispatchList", viewModel);
        }

        public IActionResult CopySales(int invId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var baseInv = _context.GetMPSContext().Sale.Where(w => w.InvoiceId.Equals(invId)).First();

            Sale newSale = new Sale();

            newSale.Company = baseInv.Company;
            newSale.CompanyNo = baseInv.CompanyNo;
            if (baseInv.Revision.HasValue)
            {
                newSale.Revision = baseInv.Revision.Value;
            }

            newSale.ShippingAddress = baseInv.ShippingAddress;

            newSale.ArrivalDate = baseInv.ArrivalDate.Value;
            newSale.DeliveryDate = baseInv.DeliveryDate.Value;

            newSale.Status = "Invoice";
            newSale.Type = baseInv.Type;

            if (baseInv.ClaimReference.HasValue)
            {
                newSale.ClaimReference = baseInv.ClaimReference.Value;
            }

            if (!string.IsNullOrEmpty(baseInv.Comment))
            {
                newSale.Comment = baseInv.Comment;
            }

            var timeSync = DateTime.Now;

            newSale.UpdatedOn = timeSync;
            newSale.UpdatedBy = userName;

            _context.GetMPSContext().Sale.Add(newSale);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                newSale = _context.GetMPSContext().Sale.Where(w => w.UpdatedOn.Equals(timeSync)).First();
                _logger.WriteEvents(userName, "Sale", "Added", "Sale ID#" + newSale.InvoiceId + " Copied From ID#" + baseInv.InvoiceId);
            }

            return RedirectToAction("SalesDetails", new { id = newSale.InvoiceId });
        }

        [HttpPost]
        public IActionResult DeleteSale(string type, int dataId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (type.Equals("Sales"))
            {
                var target = _context.GetMPSContext().Sale.Where(w => w.InvoiceId.Equals(dataId)).First();

                target.Status = "Deleted";
                target.UpdatedBy = userName;
                target.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().Sale.Update(target);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Sale", "Deleted", "Sale ID#" + dataId + " Deleted");
                }
            }
            else if (type.Equals("SaleItem"))
            {
                var target = _context.GetMPSContext().SaleItem.Where(w => w.Id.Equals(dataId)).First();

                _context.GetMPSContext().SaleItem.Remove(target);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Sale", "Delete Item", "Sale Item ID#" + dataId + " Deleted");
                }
            }
            else if (type.Equals("Dispatch"))
            {
                var target = _context.GetMPSContext().SaleDispatch.Where(w => w.DispatchId.Equals(dataId)).First();
                _context.GetMPSContext().SaleDispatch.Remove(target);

                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Dispatch", "Deleted", "Dispatch ID#" + dataId + " Deleted");
                }

                var targetList = _context.GetMPSContext().SaleDispatchItem.Where(w => w.DispatchId.Equals(target.DispatchId)).ToList();

                if (targetList.Count > 0)
                {
                    _context.GetMPSContext().SaleDispatchItem.RemoveRange(targetList);
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "Dispatch", "Delete Item", "Dispatch Item ID#" + dataId + " Deleted");
                    }
                }

            }

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult GetFirstDispatchId(int invId)
        {
            if (_context.GetMPSContext().SaleDispatch.Any(w => w.SaleInvoiceId.HasValue && w.SaleInvoiceId.Value.Equals(invId)))
            {
                var firstDispatchId = _context.GetMPSContext().SaleDispatch.Where(w => w.SaleInvoiceId.HasValue && w.SaleInvoiceId.Value.Equals(invId)).OrderBy(o => o.UpdatedOn).First().DispatchId;

                return new JsonResult(firstDispatchId);
            }
            else
            {
                return new JsonResult(0);
            }
        }

        [HttpPost]
        public IActionResult GetClaimRefList()
        {
            List<ClaimRefItem> claimRefList = new List<ClaimRefItem>();

            claimRefList = _api.GetClaimRefList("Sales");

            return new JsonResult(claimRefList);
        }

        [HttpPost]
        public IActionResult GetClaimDetails(int claimId)
        {
            ClaimRefItem claimRefItem = new ClaimRefItem();

            claimRefItem = _api.GetClaimDetails("Sales", claimId);

            return new JsonResult(claimRefItem);
        }

        [HttpPost]
        public IActionResult UpdateData(int itemId, string dataType, string chgVal)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var saleItem = _context.GetMPSContext().SaleItem.Where(w => w.Id.Equals(itemId)).First();
            var originalValue = "";

            if (dataType.Equals("Sort"))
            {
                originalValue = saleItem.SortId.ToString();
                saleItem.SortId = int.Parse(chgVal);
            }
            else if (dataType.Equals("ProductCode"))
            {
                originalValue = saleItem.ProductCode;

                var product = _context.GetMPSContext().Product.Where(w => w.Id.Equals(int.Parse(chgVal))).First();

                saleItem.ProductCode = product.Code;
                saleItem.ProductDesc = product.Desc;

                if (_context.GetMPSContext().ProductMapping.Any(x => x.MercCode.Equals(saleItem.ProductCode)))
                {
                    var mapping = _context.GetMPSContext().ProductMapping.Where(w => w.MercCode.Equals(saleItem.ProductCode)).First();

                    saleItem.CompanyCode = mapping.CompanyCode;
                    saleItem.CompanyDesc = mapping.CompanyDesc;
                }
                else
                {
                    saleItem.CompanyCode = "";
                    saleItem.CompanyDesc = "";
                }

                chgVal = saleItem.ProductCode;
            }

            saleItem.UpdatedBy = userName;
            saleItem.UpdatedOn = DateTime.Now;

            _context.GetMPSContext().SaleItem.Update(saleItem);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Sale", "Adjustment", "Sale Item ID# " + itemId + ", " + dataType + " [ " + originalValue + " -> " + chgVal + " ] Updated");
            }

            return new JsonResult("Success");
        }

        [HttpGet]
        public IActionResult PickingList(int dispatchId)
        {
            var dispatchInformation = _api.GetDispatchInformation(dispatchId);
            var dispatchList = _api.GetDispatchList(dispatchId);
            var result = new CarrotSystem.Models.ViewModels.PickingListViewModel()
            {
                DispatchView = dispatchInformation,
                DispatchViewList = dispatchList
            };

            return View(result);
        }

        [HttpGet]
        public IActionResult LoadSheet(int dispatchId)
        {
            var dispatchInformation = _api.GetDispatchInformation(dispatchId);
            var dispatchList = _api.GetDispatchList(dispatchId);
            var result = new CarrotSystem.Models.ViewModels.PickingListViewModel()
            {
                DispatchView = dispatchInformation,
                DispatchViewList = dispatchList
            };
            return View(result);
        }

        [HttpGet]
        public IActionResult GetDeliveryAdvice(int dispatchId)
        {
            var dispatchInformation = _api.GetDispatchInformation(dispatchId);
            var dispatchList = _api.GetDispatchList(dispatchId);
            var result = new CarrotSystem.Models.ViewModels.PickingListViewModel()
            {
                DispatchView = dispatchInformation,
                DispatchViewList = dispatchList
            };
            return View(result);
        }

        [HttpGet]
        public IActionResult SendPickingListEmail(int dispatchId)
        {
            var dispatchInformation = _api.GetDispatchInformation(dispatchId);
            var dispatchList = _api.GetDispatchList(dispatchId);
            

            return null;
        }

        //Method

        // End Methods
    }
}
