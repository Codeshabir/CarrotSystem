
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarrotSystem.Controllers
{
   // [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class OperationController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly IEventWriter _logger;
        private readonly IGenPDFService _pdfService;
        public string eventType = "Operation";

        public OperationController(IEventWriter logger, IContextService context, IAPIService api, IGenPDFService pdfService)
        {
            _logger = logger;
            _context = context;
            _pdfService = pdfService;
            _api = api;
        }

        [AllowAnonymous]
        public IActionResult Packing()
        {
            ViewOperation viewModel = new ViewOperation();

            DateTime baseDate = DateTime.Now.AddDays(7);
            
            List<PeriodicView> periodList = new List<PeriodicView>();
            periodList = _api.GetPeriodicList(baseDate, "Packing");
            
            viewModel.periodicList = periodList;
            viewModel.focusId = _api.GetLatePeriodId("Packing");
            viewModel.focusDate = _api.GetLateUpdateDate("Packing");

            List<CustomisedProductItemModel> productList = new List<CustomisedProductItemModel>();
            List<Company> supplierList = new List<Company>();
            
            productList = _api.GetCustomisedProductList("Active", "Packing");
            supplierList = _api.GetCompanyList("Supplier", "Active");

            viewModel.productList = productList;
            viewModel.supplierList = supplierList;

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult ProductTransfer()
        {
            ViewOperation viewModel = new ViewOperation();

            DateTime baseDate = DateTime.Now.AddDays(7);
            List<PeriodicView> periodList = new List<PeriodicView>();
            periodList = _api.GetPeriodicList(baseDate, "Transfer");
            
            List<CustomisedProductItemModel> productList = new List<CustomisedProductItemModel>();
            List<Company> supplierList = new List<Company>();

            productList = _api.GetCustomisedProductList("Active", "Transfer");
            supplierList = _api.GetCompanyList("Supplier", "Active");
            viewModel.productList = productList;
            viewModel.supplierList = supplierList;

            viewModel.periodicList = periodList;
            viewModel.focusId = _api.GetLatePeriodId("Transfer");
            viewModel.focusDate = _api.GetLateUpdateDate("Transfer");

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Waste()
        {
            ViewOperation viewModel = new ViewOperation();

            DateTime baseDate = DateTime.Now.AddDays(7);
            List<PeriodicView> periodList = new List<PeriodicView>();
            periodList = _api.GetPeriodicList(baseDate, "Waste");

            List<CustomisedProductItemModel> productList = new List<CustomisedProductItemModel>();
            List<Company> supplierList = new List<Company>();

            productList = _api.GetCustomisedProductList("Active", "Waste");
            supplierList = _api.GetCompanyList("Supplier", "Active");
            viewModel.productList = productList;
            viewModel.supplierList = supplierList;
            viewModel.reasonList = _api.GetWasteReason();

            viewModel.periodicList = periodList;
            viewModel.focusId = _api.GetLatePeriodId("Waste");
            viewModel.focusDate = _api.GetLateUpdateDate("Waste");

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult StockCount()
        {
            ViewOperation viewModel = new ViewOperation();

            DateTime baseDate = DateTime.Now.AddDays(7);
            List<PeriodicView> periodList = new List<PeriodicView>();
            
            periodList = _api.GetPeriodicList(baseDate, "StockCount");

            viewModel.periodicList = periodList;

            if (periodList.Count > 0)
            {
                viewModel.dataId = periodList.OrderByDescending(d => d.TargetDate).First().PeriodId.ToString();
            }
            else
            {
                viewModel.dataId = "0";
            }

            List<CustomisedProductItemModel> productList = new List<CustomisedProductItemModel>();
            List<Company> supplierList = new List<Company>();

            productList = _api.GetCustomisedProductList("Active", "StockCount");
            supplierList = _api.GetCompanyList("Supplier", "Active");

            viewModel.productList = productList;
            viewModel.supplierList = supplierList;

            var lastTarget = _context.GetMPSContext().Waste.Where(w => w.Id > 0).OrderByDescending(d => d.Id).FirstOrDefault();

            if(lastTarget != null)
            {
                viewModel.focusId = lastTarget.Id;
            }
            else
            {
                viewModel.focusId = 0;
            }
            
            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult GetPackingList(string target)
        {
            var targetDate = DateTime.ParseExact(target, "ddMMyyyy", CultureInfo.InvariantCulture);
            var isClosed = _api.IsClosedPeriodByDate(targetDate);

            List<ProductPacking> packingList = new List<ProductPacking>();
            DetailViewPacking detailView = new DetailViewPacking();

            if(_context.GetMPSContext().ProductPacking.Any(w => w.PackingDate.Value.Date.Equals(targetDate.Date)))
            {
                packingList = _context.GetMPSContext().ProductPacking.Where(w => w.PackingDate.Value.Date.Equals(targetDate.Date)).ToList();
            }

            if(packingList.Count > 0)
            {
                List<PackingView> packingViewList = new List<PackingView>();

                foreach(var packing in packingList)
                {
                    PackingView packingView = new PackingView(); 

                    packingView.ItemPK = packing.Pk;
                    packingView.IsClosed = isClosed;
                    packingView.PackingId = packing.Id;
                    packingView.PackingDate = packing.PackingDate.Value;
                    packingView.PackingDateString = packing.PackingDate.Value.ToString("dd/MM/yy");
                    packingView.ProductCode = packing.ProductCode;
                    packingView.ProductId = _context.GetMPSContext().Product.Where(w => w.Code.Equals(packing.ProductCode)).First().Id;
                    packingView.ProductDesc = _context.GetMPSContext().Product.Where(w=>w.Code.Equals(packing.ProductCode)).First().Desc;

                    if (packing.ProductQty.HasValue)
                    {
                        packingView.ProductQty = packing.ProductQty.Value;
                    }

                    if(packing.BestBefore.HasValue)
                    {
                        packingView.BestBefore = packing.BestBefore.Value;
                        packingView.BestBeforeString = packing.BestBefore.Value.ToString("dd/MM/yy");
                    }

                    packingView.Supplier = packing.Supplier;
                    packingView.SupplierPK = _context.GetMPSContext().Company.Where(w=>w.CompanyName.Equals(packingView.Supplier)).First().Pk;
                    packingView.PackingDesc = packing.Description;

                    packingViewList.Add(packingView);
                }

                List<CustomisedProductItemModel> productList = new List<CustomisedProductItemModel>();
                List<Company> supplierList = new List<Company>();
                //Finished, Wholesale
                productList = _api.GetCustomisedProductList("Active", "Packing");
                supplierList = _api.GetCompanyList("Supplier", "Active");

                detailView.productList = productList;
                detailView.supplierList = supplierList;
                detailView.detailList = packingViewList;

                return PartialView("_PackingDetails", detailView);
            }
            else
            {
                return new JsonResult("None");
            }
            
        }

        [AllowAnonymous]
        public IActionResult GetWasteList(string target)
        {
            var targetDate = DateTime.ParseExact(target, "ddMMyyyy", CultureInfo.InvariantCulture);
            var isClosed = _api.IsClosedPeriodByDate(targetDate);
            
            DetailViewWaste detailView = new DetailViewWaste();
            List<Waste> wasteList = new List<Waste>();

            if (_context.GetMPSContext().Waste.Any(w => w.WasteDate.Value.Date.Equals(targetDate.Date)))
            {
                wasteList = _context.GetMPSContext().Waste.Where(w => w.WasteDate.Value.Date.Equals(targetDate.Date)).ToList();
            }

            if (wasteList.Count > 0)
            {
                List<WasteView> wasteViewList = new List<WasteView>();

                foreach (var waste in wasteList)
                {
                    WasteView wasteView = new WasteView();
                    wasteView.IsClosed = isClosed;
                    wasteView.WasteId = waste.Id;
                    wasteView.WasteDate = waste.WasteDate.Value;
                    wasteView.WasteDateString = waste.WasteDate.Value.ToString("dd/MM/yy"); ;
                    wasteView.ProductCode = waste.ProductCode;
                    wasteView.ProductId = _context.GetMPSContext().Product.Where(w => w.Code.Equals(wasteView.ProductCode)).First().Id;
                    wasteView.ProductDesc = _context.GetMPSContext().Product.Where(w => w.Code.Equals(wasteView.ProductCode)).First().Desc;
                    wasteView.ProductQty = waste.Qty.Value;
                    wasteView.Supplier = waste.Supplier;
                    wasteView.SupplierPK = _context.GetMPSContext().Company.Where(w=>w.CompanyName.Equals(waste.Supplier)).First().Pk;
                    wasteView.Reason = waste.Reason;

                    wasteViewList.Add(wasteView);
                }

                List<CustomisedProductItemModel> productList = new List<CustomisedProductItemModel>();
                List<Company> supplierList = new List<Company>();
                //Raw, Finished, Wholesale, Packing
                productList = _api.GetCustomisedProductList("Active", "Waste");
                supplierList = _api.GetCompanyList("Supplier", "Active");

                detailView.reasonList = _api.GetWasteReason();
                detailView.detailList = wasteViewList;
                detailView.productList = productList;
                detailView.supplierList = supplierList;

                return PartialView("_WasteDetails", detailView);
            }
            else
            {
                return new JsonResult("None");
            }

        }

        [AllowAnonymous]
        public IActionResult GetStockCountList(string periodId)
        {
            List<StockCountView> stockCountViewList = new List<StockCountView>();

            stockCountViewList = _api.GetStockCountList(periodId);

            if (stockCountViewList.Count > 0)
            {
                return PartialView("_CountDetails", stockCountViewList);
            }
            else
            {
                return new JsonResult("No records for the selected period.");
            }

        }

        [AllowAnonymous]
        public IActionResult GetTransferList(string target)
        {
            var targetDate = DateTime.ParseExact(target, "ddMMyyyy", CultureInfo.InvariantCulture);
            var isClosed = _api.IsClosedPeriodByDate(targetDate);

            List<ProductTransfer> transferList = new List<ProductTransfer>();
            List<TransferJsonView> transferJsonList = new List<TransferJsonView>();

            if (_context.GetMPSContext().ProductTransfer.Any(w => w.TransferDate.Value.Date.Equals(targetDate.Date)))
            {
                transferList = _context.GetMPSContext().ProductTransfer.Where(w => w.TransferDate.Value.Date.Equals(targetDate.Date)).ToList();

                foreach(var transfer in transferList)
                {
                    TransferJsonView item = new TransferJsonView();

                    item.IsClosed = isClosed;
                    item.DataId = transfer.Id;
                    item.FromProduct = transfer.FromProduct;
                    item.FromProductId = _api.GetProductIdByCode(transfer.FromProduct).ToString();
                    item.FromQty = transfer.FromQty.Value;
                    item.ToProduct = transfer.ToProduct;
                    item.ToProductId = _api.GetProductIdByCode(transfer.ToProduct).ToString();
                    item.ToQty = transfer.ToQty.Value;
                    item.Price = transfer.Price.Value;
                    item.TransferDate = transfer.TransferDate.Value;

                    transferJsonList.Add(item);
                }
            }
            
            return PartialView("_TransferDetails", transferJsonList);
        }

        //Methods
        [AllowAnonymous]
        public List<DateTime> GetSelectDateList(DateTime basis, string targetData)
        {
            List<DateTime> dateList = new List<DateTime>();

            DateTime dateFrom = DateTime.ParseExact("01/07/" + basis.AddYears(-1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateTo = DateTime.ParseExact("30/06/" + basis.AddYears(1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            if (targetData.Equals("Packing"))
            {
                List<ProductPacking> packingList = new List<ProductPacking>();
                packingList = _context.GetMPSContext().ProductPacking.Where(w => w.PackingDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.PackingDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                if (packingList.Count > 0)
                {
                    foreach (var packing in packingList)
                    {
                        if(packing.PackingDate.HasValue)
                        {
                            dateList.Add(packing.PackingDate.Value);
                        }
                    }
                }
            }

            return dateList;
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult NewPacking(ViewOperation selectedModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            ProductPacking newPacking = new ProductPacking();
                
            var product = _api.GetProductById(int.Parse(selectedModel.newPacking.ProductId));

            newPacking.ProductCode = product.Code;
            newPacking.Description = selectedModel.newPacking.PackingDesc;
            newPacking.LabourCode = _api.GetLabourCodeByProductCode(newPacking.ProductCode);
            newPacking.Supplier = _api.GetSupplierNameById(int.Parse(selectedModel.newPacking.SupplierId));
            newPacking.ProductQty = selectedModel.newPacking.ProductQty;
            newPacking.BestBefore = DateTime.ParseExact(selectedModel.newPacking.BestBeforeString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            newPacking.PackingDate = DateTime.ParseExact(selectedModel.newPacking.PackingDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            newPacking.UpdatedOn = DateTime.Now;
            newPacking.UpdatedBy = userName;

            _context.GetMPSContext().ProductPacking.Add(newPacking);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Packing", "Added", "Product Packing Item [" + newPacking.ProductCode + " @ " + newPacking.ProductQty + "] Added");
            }

            return RedirectToAction("Packing");
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult ChangePacking(int itemId, string type, string chgValue)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            ProductPacking item = new ProductPacking();

            if(itemId > 0)
            {
                item = _context.GetMPSContext().ProductPacking.Where(w=>w.Pk.Equals(itemId)).First();

                var originalValue = "";

                if(type.Equals("Qty"))
                {
                    originalValue = item.ProductQty.ToString();
                    item.ProductQty = int.Parse(chgValue);
                }
                else if (type.Equals("PackDate"))
                {
                    var packDate = DateTime.ParseExact(chgValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    originalValue = item.PackingDate.Value.ToString("dd/MM/yyyy");
                    item.PackingDate = packDate;
                }
                else if (type.Equals("ProductCode"))
                {
                    originalValue = item.ProductCode;
                    item.ProductCode = _context.GetMPSContext().Product.Where(w => w.Id.Equals(int.Parse(chgValue))).First().Code;
                }
                else if (type.Equals("BeforeDate"))
                {
                    var bestbefore = DateTime.ParseExact(chgValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    originalValue = item.BestBefore.Value.ToString("dd/MM/yyyy");
                    item.BestBefore = bestbefore;
                }
                else if (type.Equals("Supplier"))
                {
                    var supplier = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(int.Parse(chgValue))).First().CompanyName;
                    originalValue = item.Supplier;
                    item.Supplier = supplier;
                }
                if (type.Equals("PackDesc"))
                {
                    originalValue = item.Description;
                    item.Description = chgValue;
                }

                item.UpdatedBy = userName;
                item.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().ProductPacking.Update(item);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Packing", "Adjustment", "Product Packing ID#" + item.Id + " Updated [" + type + "] " + originalValue + " -> " + chgValue);
                }

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }
           
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult NewTransfer(ViewOperation selectedModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            ProductTransfer newTransfer = new ProductTransfer();

            var fromProduct = _api.GetProductById(int.Parse(selectedModel.newTransfer.FromProductId));
            var toProduct = _api.GetProductById(int.Parse(selectedModel.newTransfer.ToProductId));

            newTransfer.FromProduct = fromProduct.Code;
            newTransfer.ToProduct = toProduct.Code;
            newTransfer.FromQty = selectedModel.newTransfer.FromQty;
            newTransfer.ToQty = selectedModel.newTransfer.ToQty;
            newTransfer.Price = selectedModel.newTransfer.Price;

            newTransfer.TransferDate = DateTime.ParseExact(selectedModel.newTransfer.TransferDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            newTransfer.UpdatedBy = userName;
            newTransfer.UpdatedOn = DateTime.Now;

            _context.GetMPSContext().ProductTransfer.Add(newTransfer);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Transfer", "Added", "Product Transfer Item [" + newTransfer.FromProduct + " @ " + newTransfer.FromQty + "] to [" + newTransfer.ToProduct + " @ " + newTransfer.ToQty + "] ");
            }

            return RedirectToAction("ProductTransfer");
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult ChangeTransfer(int itemId, string type, string chgValue)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var changeVal = 0.0;
            
            ProductTransfer item = new ProductTransfer();

            if (itemId > 0)
            {
                if(!string.IsNullOrEmpty(chgValue))
                {
                    changeVal = Convert.ToDouble(chgValue);
                }

                _logger.WriteTestLog("Change Value : " + chgValue);
                
                item = _context.GetMPSContext().ProductTransfer.Where(w => w.Id.Equals(itemId)).First();

                var originalValue = "";

                if (type.Equals("FromQty"))
                {
                    originalValue = item.FromQty.ToString();
                    item.FromQty = changeVal;
                }
                else if (type.Equals("ToQty"))
                {
                    originalValue = item.ToQty.ToString();
                    item.ToQty = changeVal;
                }
                else if (type.Equals("Price"))
                {
                    originalValue = item.Price.ToString();
                    item.Price = changeVal;
                }

                item.UpdatedBy = userName;
                item.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().ProductTransfer.Update(item);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Transfer", "Adjustment", "Product Transfer ID#" + item.Id + " [" + type + "] " + originalValue + " -> " + chgValue + " Updated");
                }

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }

        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult NewWaste(ViewOperation selectedModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            Waste newWaste = new Waste();
            var product = _api.GetProductById(selectedModel.newWaste.ProductId);

            newWaste.ProductCode = product.Code;

            newWaste.Qty = selectedModel.newWaste.ProductQty;
            newWaste.Reason = selectedModel.newWaste.Reason;
            newWaste.Supplier = _api.GetSupplierNameById(selectedModel.newWaste.SupplierId);

            newWaste.WasteDate = DateTime.ParseExact(selectedModel.newWaste.WasteDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            newWaste.UpdatedOn = DateTime.Now;
            newWaste.UpdatedBy = _api.GetFullName(HttpContext.User.Identity.Name);

            _context.GetMPSContext().Waste.Add(newWaste);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Waste", "Added", "Product Waste Item [" + newWaste.ProductCode + " @ " + newWaste.Qty + "] Added");
            }

            return RedirectToAction("Waste");
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult ChangeWaste(int itemId, string type, string chgValue)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            Waste item = new Waste();

            if (itemId > 0)
            {
                item = _context.GetMPSContext().Waste.Where(w => w.Id.Equals(itemId)).First();

                var originalValue = "";

                if (type.Equals("Qty"))
                {
                    originalValue = item.Qty.ToString();
                    item.Qty = int.Parse(chgValue);
                }
                else if (type.Equals("WasteDate"))
                {
                    var wasteDate = DateTime.ParseExact(chgValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    originalValue = item.WasteDate.Value.ToString("dd/MM/yyyy");
                    item.WasteDate = wasteDate;
                }
                else if (type.Equals("ProductCode"))
                {
                    originalValue = item.ProductCode;
                    item.ProductCode = _context.GetMPSContext().Product.Where(w => w.Id.Equals(int.Parse(chgValue))).First().Code;
                }
                else if (type.Equals("Supplier"))
                {
                    originalValue = item.Supplier;
                    var supplier = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(int.Parse(chgValue))).First().CompanyName;
                    item.Supplier = supplier;
                }
                if (type.Equals("Reason"))
                {
                    originalValue = item.Reason;
                    item.Reason = chgValue;
                }

                item.UpdatedBy = userName;
                item.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().Waste.Update(item);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Waste", "Adjustment", "Product Waste ID#" + item.Id + " [" + type + "] " + originalValue + " -> " + chgValue + " Updated");
                }

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }

        }

        [AllowAnonymous]
        public IActionResult GetNewStockCountList()
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            Period period = new Period();
            period = _api.GetLatePeriod(DateTime.Now);

            List<StockCount> stockCountList = new List<StockCount>();
            List<StockCountView> stockCountViewList = new List<StockCountView>();

            if (_context.GetMPSContext().StockCount.Any(a=>a.PeriodId.Equals(period.Id)))
            {
                stockCountList = _context.GetMPSContext().StockCount.Where(w => w.PeriodId.Equals(period.Id)).ToList();
            }
            else
            {
                List<StockCount> lastCountList = new List<StockCount>();
                var lastPeriodId = _context.GetMPSContext().StockCount.OrderByDescending(o=>o.PeriodId).First().PeriodId;

                if (_context.GetMPSContext().StockCount.Any(w => w.PeriodId.Equals(lastPeriodId)))
                {
                    lastCountList = _context.GetMPSContext().StockCount.Where(w => w.PeriodId.Equals(lastPeriodId)).ToList();
                }

                foreach(var item in lastCountList)
                {
                    if(_api.IsStockCountable(item.ProductCode))
                    {
                        StockCount newCount = new StockCount();

                        newCount.PeriodId = period.Id;
                        newCount.ProductCode = item.ProductCode;
                        newCount.Qty = 0;
                        if (item.Qty.HasValue)
                        {
                            newCount.LogLastQty = item.Qty.Value;
                        }
                        else
                        {
                            newCount.LogLastQty = 0;
                        }

                        newCount.UpdatedOn = period.EndDate;
                        newCount.UpdatedBy = userName;

                        stockCountList.Add(newCount);

                        _context.GetMPSContext().StockCount.Add(newCount);
                        if (_context.GetMPSContext().SaveChanges() > 0)
                        {
                            _logger.WriteEvents(userName, "Stock Count", "Added", "Stock Count Item [" + newCount.ProductCode + " @ " + newCount.Qty + "] Counted");
                        }
                    }
                }
            }

            stockCountList = _api.GetStockCountActiveList(stockCountList);

            foreach (var stockCout in stockCountList)
            {
                StockCountView stockCountView = new StockCountView();

                stockCountView.StockCountId = stockCout.Id;
                stockCountView.StockDate = period.EndDate.Value;
                stockCountView.StockDateString = period.EndDate.Value.ToString("dd/MM/yy");
                stockCountView.ProductCode = stockCout.ProductCode;
                
                if(_context.GetMPSContext().Product.Any(w => w.Code.Equals(stockCountView.ProductCode)))
                {
                    stockCountView.ProductDesc = _context.GetMPSContext().Product.Where(w => w.Code.Equals(stockCountView.ProductCode)).First().Desc;
                }
                else
                {
                    stockCountView.ProductDesc = "";
                }
                
                if (period.Status.Equals("Open"))
                {
                    stockCountView.IsClosed = false;
                }
                else
                {
                    stockCountView.IsClosed = true;
                }

                if (stockCout.Qty.HasValue)
                {
                    stockCountView.ProductQty = stockCout.Qty.Value;
                }
                else
                {
                    stockCountView.ProductQty = 0;
                }

                if (string.IsNullOrEmpty(stockCout.BatchCode))
                {
                    stockCountView.BatchCode = "";
                }
                else
                {
                    stockCountView.BatchCode = stockCout.BatchCode;
                }

                stockCountViewList.Add(stockCountView);
            }

            return PartialView("_StockCountList", stockCountViewList);
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult ChangeCountItem(string type, int dataId, string chgVal)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (_context.GetMPSContext().StockCount.Any(a=>a.Id.Equals(dataId)))
            {
                var target = _context.GetMPSContext().StockCount.Where(w=>w.Id.Equals(dataId)).First();

                var originalValue = "";

                if(type.Equals("Qty"))
                {
                    originalValue = target.Qty.ToString();
                    target.Qty = double.Parse(chgVal);
                }
                else if (type.Equals("BatchCode"))
                {
                    originalValue = target.BatchCode;
                    target.BatchCode = chgVal;
                }

                target.UpdatedBy = userName;
                target.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().StockCount.Update(target);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Stock Count", "Adjustment", "Stock Count Item ID#" + target.Id + " [" + type + "] " + originalValue + " -> " + chgVal + " Updated");
                }

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }
        }

        [HttpPost]
        public IActionResult GetStockCountForm(ViewOperation selectedModel)
        {
            var periodId = selectedModel.targetId;
            
            if(periodId > 0)
            {
                var title = "Stock Count Form - MPS";

                var itemList = _api.GetStockCountList(periodId.ToString());

                return View(itemList);

                var pdfFile = _pdfService.GeneratePDF(title, "StockCountForm", periodId);

                return new FileStreamResult(new MemoryStream(pdfFile), "application/pdf");
            
            }
            else
            {
                return RedirectToAction("StockCount");
            }
        }

        [HttpPost]
        public IActionResult GetStockCountReport(ViewOperation selectedModel)
        {
            var periodId = selectedModel.targetId;

            if(periodId > 0)
            {
                var itemList = _api.GetStockCountList(periodId.ToString());

                return View(itemList);

                //var title = "Stock Count Report - MPS";
                //var pdfFile = _pdfService.GeneratePDF(title, "StockCountReport", periodId);
                //return new FileStreamResult(new MemoryStream(pdfFile), "application/pdf");
            }
            else
            {
                return RedirectToAction("StockCount");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult DeleteItem (string type, int itemId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (type.Equals("Packing"))
            {
                var deleteModel = _context.GetMPSContext().ProductPacking.Where(w=>w.Id.Equals(itemId)).First();

                _context.GetMPSContext().ProductPacking.Remove(deleteModel);
                _context.GetMPSContext().SaveChanges();
            }
            else if (type.Equals("Transfer"))
            {
                var deleteModel = _context.GetMPSContext().ProductTransfer.Where(w => w.Id.Equals(itemId)).First();

                _context.GetMPSContext().ProductTransfer.Remove(deleteModel);
                _context.GetMPSContext().SaveChanges();
            }
            else if (type.Equals("Waste"))
            {
                var deleteModel = _context.GetMPSContext().Waste.Where(w => w.Id.Equals(itemId)).First();

                _context.GetMPSContext().Waste.Remove(deleteModel);
                _context.GetMPSContext().SaveChanges();
            }

            _logger.WriteEvents(userName, type, "Deleted", type + " Item ID#" + itemId + " Deleted");

            return new JsonResult("Success");
        }

        //End Methods

    }
}
