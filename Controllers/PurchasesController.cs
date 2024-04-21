
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarrotSystem.Controllers
{
    //[Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class PurchasesController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly ISystemService _system;
        private readonly IEventWriter _logger;

        public string eventBy = "Purchase";

        public PurchasesController(IEventWriter logger, IContextService context, IAPIService api, ISystemService system)
        {
            _logger = logger;
            _context = context;
            _system = system;
            _api = api;
        }

        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult PurchaseList()
        {
            ViewPurchase viewModel = new ViewPurchase();

            viewModel.dateFrom = _context.GetDateByNow(-15);
            viewModel.dateTo = DateTime.Now.AddDays(7).Date;

            string strSupplier = "All";
            string strType = "All";
            string strStatus = "All";
            string isShowAll = "No";

            List<Purchase> purchaseList = new List<Purchase>();
            purchaseList = GetPurchaseList(viewModel.dateFrom, viewModel.dateTo);

            List<PurchaseView> purchaseViewList = new List<PurchaseView>();
            purchaseViewList = GetPurchaseViewList(purchaseList, strSupplier, strStatus, strType, isShowAll);

            viewModel.isShowAll = isShowAll;
            viewModel.purchaseViewList = purchaseViewList;
            viewModel.supplierList = GetSupplierList(purchaseList);
            viewModel.typeList = GetTypeList(purchaseList);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult SelectedPurchaseList(ViewPurchase selectedModel)
        {
            ViewPurchase viewModel = new ViewPurchase();

            DateTime dateFrom = DateTime.ParseExact(selectedModel.strDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateTo = DateTime.ParseExact(selectedModel.strDateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            viewModel.dateFrom = dateFrom;
            viewModel.dateTo = dateTo;

            string strSupplier = selectedModel.supplier;
            string strType = selectedModel.type;
            string strStatus = selectedModel.status;
            string isShowAll = selectedModel.isShowAll;

            List<Purchase> purchaseList = new List<Purchase>();
            purchaseList = GetPurchaseList(dateFrom, dateTo);

            List<PurchaseView> purchaseViewList = new List<PurchaseView>();
            purchaseViewList = GetPurchaseViewList(purchaseList, strSupplier, strStatus, strType, isShowAll);

            viewModel.isShowAll = isShowAll;
            viewModel.purchaseViewList = purchaseViewList;
            viewModel.supplierList = GetSupplierList(purchaseList);
            viewModel.typeList = GetTypeList(purchaseList);

            return View("PurchaseList", viewModel);
        }

        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult PurchaseDetails(string id)
        {
            ViewPurchase viewModel = new ViewPurchase();
            PurchaseView purchaseView = new PurchaseView();

            if (id.Equals("New"))
            {
                DateTime timeSync = DateTime.Now;
                Purchase newPurchase = new Purchase();

                //purchaseView.Status = "Invoice";
                purchaseView.Status = "New";
                purchaseView.DeliveryDate = timeSync.Date;

                viewModel.isNew = true;
            }
            else
            {
                purchaseView = _api.GetPurchase(int.Parse(id));
                viewModel.isNew = false;
            }

            viewModel.purchaseView = purchaseView;

            viewModel.isClosed = _api.IsClosedPeriodByDate(purchaseView.DeliveryDate);

            List<PurchaseItemView> purchaseItemViewList = new List<PurchaseItemView>();
            purchaseItemViewList = _api.GetPurchaseItemList(purchaseView.InvoiceId, purchaseView.Company);

            viewModel.purchaseItemViewList = purchaseItemViewList;
            PurchaseTotalView totalView = new PurchaseTotalView();
            totalView = _api.CalcPurchaseTotal(purchaseItemViewList);
            viewModel.totalView = totalView;

            List<PurchaseClaimView> claimRefList = new List<PurchaseClaimView>();
            if (!string.IsNullOrEmpty(purchaseView.ClaimRef))
            {
                claimRefList = GetClaimPurchase(purchaseView.ClaimRef);
            }
            viewModel.claimList = claimRefList;

            var custSuppList = new List<Company>();
            custSuppList = _context.GetMPSContext().Company.Where(w => (w.Type.Equals("Supplier") || w.Type.Equals("CustSup")) && w.Inactive.Equals(false)).ToList();

            var typeList = new List<string>();
            typeList = _context.GetMPSContext().PurchaseType.OrderBy(o => o.SortId).Select(s => s.Type).ToList();

            viewModel.detailSupplierList = custSuppList;
            viewModel.typeList = typeList;
            viewModel.productList = _api.GetCustomisedProductList("Active", "Purchase");

            return View("PurchaseDetails", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult DeletePurchase(string type, int dataId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (type == "Purchases")
            {
                var delModel = _context.GetMPSContext().Purchase.Where(w => w.InvoiceId.Equals(dataId)).First();

                delModel.Status = "Deleted";
                delModel.UpdatedBy = userName;
                delModel.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().Purchase.Update(delModel);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Purchase", "Deleted", "Purchase ID#" + dataId + " Deleted");
                }
            }
            else if (type == "PurchaseItem")
            {
                var delModel = _context.GetMPSContext().PurchaseItem.Where(w => w.Id.Equals(dataId)).First();

                _context.GetMPSContext().PurchaseItem.Remove(delModel);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Purchase", "Deleted", "Purchase Item ID#" + dataId + " Deleted");
                }
            }

            return new JsonResult("Success");
        }

        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult DuplicatePurchase(string id)
        {
            DateTime timeSync = DateTime.Now;
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (!string.IsNullOrEmpty(id))
            {
                var invoiceId = int.Parse(id);

                var purchase = _context.GetMPSContext().Purchase.Where(x => x.InvoiceId.Equals(invoiceId)).First();

                Purchase newPurchase = new Purchase();

                newPurchase.Status = "New";
                newPurchase.DeliveryDate = timeSync.Date;
                newPurchase.UpdatedOn = timeSync;
                newPurchase.UpdatedBy = userName;

                //newPurchase.CompanyNo = purchase.CompanyNo;
                newPurchase.Company = purchase.Company;
                newPurchase.Type = purchase.Type;

                if (string.IsNullOrEmpty(purchase.Comment))
                {
                    newPurchase.Comment = "";
                }
                else
                {
                    newPurchase.Comment = purchase.Comment;
                }

                if (purchase.Type.Contains("Claim"))
                {
                    purchase.ClaimReference = purchase.ClaimReference;

                    purchase.Returned = purchase.Returned;
                }
                else
                {
                    purchase.Returned = false;
                }

                _context.GetMPSContext().Purchase.Add(newPurchase);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    newPurchase = _context.GetMPSContext().Purchase.Where(x => x.UpdatedOn.HasValue && x.UpdatedOn.Equals(timeSync)).First();

                    _logger.WriteEvents(userName, "Purchase", "Add Purchase", "Purchase ID#" + newPurchase.InvoiceId + " Added");
                }

                if (_context.GetMPSContext().PurchaseItem.Any(x => x.InvoiceId.Equals(purchase.InvoiceId)))
                {
                    List<PurchaseItem> purchaseItems = new List<PurchaseItem>();

                    purchaseItems = _context.GetMPSContext().PurchaseItem.Where(x => x.InvoiceId.Equals(purchase.InvoiceId)).ToList();

                    foreach (var pItem in purchaseItems)
                    {
                        PurchaseItem newItem = new PurchaseItem();

                        newItem.SortId = pItem.SortId;
                        newItem.InvoiceId = newPurchase.InvoiceId;
                        newItem.InvoicedQty = pItem.InvoicedQty;
                        newItem.ProductCode = pItem.ProductCode;
                        newItem.Job = pItem.Job;
                        newItem.Price = pItem.Price;
                        newItem.Tax = pItem.Tax;

                        newItem.UpdatedBy = userName;
                        newItem.UpdatedOn = DateTime.Now;

                        _context.GetMPSContext().PurchaseItem.Add(newItem);
                        _context.GetMPSContext().SaveChanges();
                    }

                }

                return RedirectToAction("PurchaseDetails", new { id = purchase.InvoiceId.ToString() });
            }
            else
            {
                return RedirectToAction("PurchaseList");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult SaveComment(int invId, string comment)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var purchase = _context.GetMPSContext().Purchase.Where(w=>w.InvoiceId.Equals(invId)).First();

            if(!string.IsNullOrEmpty(comment))
            {
                purchase.Comment = comment;
            }
            else if(comment.Equals("None"))
            {
                purchase.Comment = "";
            }

            _context.GetMPSContext().Purchase.Update(purchase);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Purchase", "Adjustment", "Purchase ID#" + invId + "[Comment] Updated");
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult ChangeItemTotal(string total)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            
            var itemId = int.Parse(total.Split('_')[0]);
            var qty = total.Split('_')[1];
            var price = total.Split('_')[2];

            var item = _context.GetMPSContext().PurchaseItem.Where(w => w.Id.Equals(itemId)).First();
            
            var originalQty = item.InvoicedQty;
            var originalPrice = item.Price.Value;

            item.InvoicedQty = double.Parse(qty);
            item.Price = double.Parse(price);

            _context.GetMPSContext().PurchaseItem.Update(item);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Purchase", "Adjustment", "Purchase Item ID#" + itemId + "[" + originalPrice  + " @ " + originalQty + "] -> [" + item.Price + " @ "+ item.InvoicedQty + "] Updated");
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult ChangeProductonItem(int itemId, string dataType, string dataValue)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            PurchaseItem targetItem = new PurchaseItem();
            targetItem = _context.GetMPSContext().PurchaseItem.Where(w=>w.Id.Equals(itemId)).First();

            var originalValue = "";

            if(dataType.Equals("Product"))
            {
                var newProduct = _context.GetMPSContext().Product.Where(w => w.Id.Equals(int.Parse(dataValue))).First();
                originalValue = targetItem.ProductCode;
                targetItem.ProductCode = newProduct.Code;
                targetItem.Tax = newProduct.Tax;
            }
            else if (dataType.Equals("Qty"))
            {
                originalValue = targetItem.InvoicedQty.ToString();
                targetItem.InvoicedQty = double.Parse(dataValue);
            }
            else if (dataType.Equals("Price"))
            {
                originalValue = targetItem.Price.ToString();
                targetItem.Price = double.Parse(dataValue);
            }

            targetItem.UpdatedOn = DateTime.Now;
            targetItem.UpdatedBy = userName;

            _context.GetMPSContext().PurchaseItem.Update(targetItem);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Purchase", "Adjustment", "Purchase Item ID#" + targetItem.Id + "[" + dataType + "] " + originalValue + " -> " + dataValue + " Updated");
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult EditPurchase(ViewPurchase viewPurchase)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            int invId = viewPurchase.newPurchase.InvoiceId;
            var status = viewPurchase.newPurchase.Status;
            
            DateTime timeSync = DateTime.Now;
            NewPurchaseJson newPurchaseJson = viewPurchase.newPurchase;

            if (invId > 0)
            {
                var invoiceBN = newPurchaseJson.InvoiceBN;

                Purchase purchase = new Purchase();
                purchase = _context.GetMPSContext().Purchase.Where(w => w.InvoiceId.Equals(invId)).First();
                purchase.Type = newPurchaseJson.Type;
                purchase.DeliveryDate = DateTime.ParseExact(newPurchaseJson.DeliveryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                purchase.CompanyNo = newPurchaseJson.InvoiceBN;
                
                if(newPurchaseJson.Supplier > 0)
                {
                    var company = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(newPurchaseJson.Supplier)).First();
                    purchase.Company = company.CompanyName;
                }
                
                purchase.Status = newPurchaseJson.Status;

                if (string.IsNullOrEmpty(newPurchaseJson.Comment))
                {
                    purchase.Comment = "";
                }
                else
                {
                    purchase.Comment = newPurchaseJson.Comment;
                }

                if (purchase.Type.Contains("Claim"))
                {
                    if (!string.IsNullOrEmpty(newPurchaseJson.ClaimRef))
                    {
                        purchase.ClaimReference = int.Parse(newPurchaseJson.ClaimRef);
                    }

                    purchase.Returned = newPurchaseJson.ReturnStock;
                }
                else
                {
                    purchase.Returned = false;
                }

                purchase.UpdatedOn = timeSync;
                purchase.UpdatedBy = userName;

                _context.GetMPSContext().Purchase.Update(purchase);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    //_logger.WriteEvents(userName, "Purchase", "Adjustment", "Purchase ID#" + purchase.InvoiceId + " Updated");
                }
            }
            else
            {
                Purchase purchase = new Purchase();
                
                purchase.Type = newPurchaseJson.Type;
                purchase.DeliveryDate = DateTime.ParseExact(newPurchaseJson.DeliveryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                purchase.CompanyNo = newPurchaseJson.InvoiceBN;
                
                if (newPurchaseJson.Supplier > 0)
                {
                    var company = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(newPurchaseJson.Supplier)).First();
                    purchase.Company = company.CompanyName;
                }

                purchase.Status = "Order";

                if (string.IsNullOrEmpty(newPurchaseJson.Comment))
                {
                    purchase.Comment = "";
                }
                else
                {
                    purchase.Comment = newPurchaseJson.Comment;
                }

                if (purchase.Type.Contains("Claim"))
                {
                    if (!string.IsNullOrEmpty(newPurchaseJson.ClaimRef))
                    {
                        purchase.ClaimReference = int.Parse(newPurchaseJson.ClaimRef);
                    }

                    purchase.Returned = newPurchaseJson.ReturnStock;
                }
                else
                {
                    purchase.Returned = false;
                }

                purchase.UpdatedOn = timeSync;
                purchase.UpdatedBy = userName;

                _context.GetMPSContext().Purchase.Add(purchase);

                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    purchase = _context.GetMPSContext().Purchase.Where(w => w.UpdatedOn.HasValue && w.UpdatedOn.Value.Equals(timeSync)).First();
                    //_logger.WriteEvents(userName, "Purchase", "Added", "Purchase ID#" + purchase.InvoiceId + " Added");
                }
                
                invId = purchase.InvoiceId;
            }

            return RedirectToAction("PurchaseDetails", new { id = invId.ToString()});
        }

        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult CopyPurchase(int invId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var baseItem = _context.GetMPSContext().Purchase.Where(w => w.InvoiceId.Equals(invId)).First();

            Purchase newPurchase = new Purchase();

            newPurchase.Company = baseItem.Company;
            newPurchase.DeliveryDate = DateTime.Now;
            newPurchase.Status = "Order";
            newPurchase.Type = baseItem.Type;

            if (baseItem.ClaimReference.HasValue)
            {
                newPurchase.ClaimReference = baseItem.ClaimReference.Value;

                if (baseItem.Returned.HasValue && baseItem.Returned.Value)
                {
                    baseItem.Returned = true;
                }
                else
                {
                    baseItem.Returned = false;
                }
            }

            var timeSync = DateTime.Now;

            newPurchase.UpdatedOn = timeSync;
            newPurchase.UpdatedBy = userName;

            _context.GetMPSContext().Purchase.Add(newPurchase);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                newPurchase = _context.GetMPSContext().Purchase.Where(w => w.UpdatedOn.Equals(timeSync)).First();
                _logger.WriteEvents(userName, "Purchase", "Added", "Purchase ID#" + newPurchase.InvoiceId + " Added Copy From ID#" + baseItem.InvoiceId);
            }

            if (_context.GetMPSContext().PurchaseItem.Any(x => x.InvoiceId.HasValue && x.InvoiceId.Value.Equals(invId)))
            {
                List<PurchaseItem> itemList = new List<PurchaseItem>();
                itemList = _context.GetMPSContext().PurchaseItem.Where(x => x.InvoiceId.HasValue && x.InvoiceId.Value.Equals(invId)).ToList();

                int sortNo = 1;

                foreach (var item in itemList)
                {
                    PurchaseItem newItem = new PurchaseItem();
                    newItem.InvoiceId = newPurchase.InvoiceId;
                    newItem.ProductCode = item.ProductCode;

                    newItem.SortId = sortNo++;
                    newItem.Job = item.Job;
                    newItem.Price = item.Price;
                    newItem.Tax = item.Tax;

                    newItem.InvoicedQty = 0;

                    newItem.UpdatedOn = DateTime.Now;
                    newItem.UpdatedBy = userName;

                    _context.GetMPSContext().PurchaseItem.Add(newItem);
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "Purchase", "Added Item", "Purchase Item [" + newItem.ProductCode + "] [" + newItem.Price + " @ " + newItem.InvoicedQty + "] Copied from Item ID#" + item.Id);
                    }
                }
            }

            return RedirectToAction("PurchaseDetails", new { id = newPurchase.InvoiceId });
        }

        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult Setinvoice(int purchaseId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            Purchase purchase = new Purchase();
            purchase = _context.GetMPSContext().Purchase.Where(w => w.InvoiceId.Equals(purchaseId)).First();

            purchase.Status = "Invoice";
            purchase.UpdatedOn = DateTime.Now;
            purchase.UpdatedBy = userName;

            _context.GetMPSContext().Purchase.Update(purchase);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Purchase", "Adjustment", "Purchase ID#" + purchase.InvoiceId + " [Status] to Invoice");
            }

            return new JsonResult("Success");
        }

        //Methods
        [HttpPost]
        public IActionResult GetClaimRefList()
        {
            List<ClaimRefItem> claimRefList = new List<ClaimRefItem>();

            claimRefList = _api.GetClaimRefList("Purchases");

            return new JsonResult(claimRefList);
        }

        [HttpPost]
        public IActionResult GetClaimDetails(int claimId)
        {
            ClaimRefItem claimRefItem = new ClaimRefItem();

            claimRefItem = _api.GetClaimDetails("Purchases", claimId);

            return new JsonResult(claimRefItem);
        }

        [HttpPost]
        public IActionResult GetBillingAddress(int compPk)
        {
            var company = _context.GetMPSContext().Company.Where(w=>w.Pk.Equals(compPk)).First();
            var billAddress = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName) && w.Type.Equals("Billing")).First();
            var returnAddress = billAddress.Street + " " + billAddress.City + " " + billAddress.State + " " + billAddress.Postcode;

            return new JsonResult(returnAddress);
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
        public IActionResult NewPurchaseItem(NewPurchaseItemJson newPurchaseItem)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            PurchaseItem newItem = new PurchaseItem();

            newItem.InvoicedQty = (double)newPurchaseItem.InvoiceQty;
            newItem.Price = (double)newPurchaseItem.Price;
            newItem.InvoiceId = newPurchaseItem.InvoiceId;
            newItem.Tax = newPurchaseItem.Tax;
            newItem.Job = newPurchaseItem.Job;
            
            newItem.UpdatedOn = DateTime.Now;
            newItem.UpdatedBy = userName;

            newItem.ProductCode = _context.GetMPSContext().Product.Where(w=>w.Id.Equals(newPurchaseItem.ProductId)).First().Code;

            _context.GetMPSContext().PurchaseItem.Add(newItem);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Purchase", "Added Item", "Purchase Item [" + newItem.ProductCode + "] [" + newItem.Price + " @ " + newItem.InvoicedQty + "] Added");
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult GetProduct(int productId, int compPk)
        {
            var company = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(compPk)).First();
            var product = _context.GetMPSContext().Product.Where(w=>w.Id.Equals(productId)).First();

            NewPurchaseItemView returnItem = new NewPurchaseItemView();

            returnItem.ProductCode = product.Code;
            returnItem.ProductDesc = product.Desc;

            ProductMapping mappingProduct = new ProductMapping();

            if (_context.GetMPSContext().ProductMapping.Any(w => w.Company.Equals(company.CompanyName)))
            {
                mappingProduct = _context.GetMPSContext().ProductMapping.Where(w => w.Company.Equals(company.CompanyName)).First();

                returnItem.CompanyCode = mappingProduct.CompanyCode;
                returnItem.CompanyDesc = mappingProduct.CompanyDesc;
            }
            else
            {
                returnItem.CompanyCode = "";
                returnItem.CompanyDesc = "";
            }

            returnItem.Tax = product.Tax;

            return new JsonResult(returnItem);
        }

        public List<PurchaseClaimView> GetClaimPurchase(string claimRef)
        {
            List<PurchaseClaimView> claimList = new List<PurchaseClaimView>();
            
            List<Purchase> purchaseList = new List<Purchase>();
            purchaseList = _context.GetMPSContext().Purchase.Where(w => w.InvoiceId.Equals(int.Parse(claimRef))).ToList();

            foreach(var purchase in purchaseList)
            {
                PurchaseClaimView claimView = new PurchaseClaimView();

                claimView.InvoiceId = purchase.InvoiceId;
                claimView.InvoiceBN = purchase.CompanyNo;
                claimView.Status = purchase.Status;
                claimView.Type = purchase.Type;
                claimView.ClaimDate = purchase.DeliveryDate.Value;

                claimList.Add(claimView);
            }

            return claimList;
        }

        

        public List<string> GetSupplierList(List<Purchase> purchaseList)
        {
            List<string> supplierList = new List<string>();

            foreach(var item in purchaseList)
            {
                supplierList.Add(item.Company);
            }

            return supplierList.Distinct().ToList();
        }

        public List<string> GetStatusList(List<Purchase> purchaseList)
        {
            List<string> statusList = new List<string>();

            foreach (var item in purchaseList)
            {
                statusList.Add(item.Status);
            }

            return statusList.Distinct().ToList();
        }

        public List<string> GetTypeList(List<Purchase> purchaseList)
        {
            List<string> typeList = new List<string>();

            foreach (var item in purchaseList)
            {
                typeList.Add(item.Type);
            }

            return typeList.Distinct().ToList();
        }

        public List<Purchase> GetPurchaseList(DateTime dateFrom, DateTime dateTo)
        {
            List<Purchase> purchaseList = new List<Purchase>();
            purchaseList = _context.GetMPSContext().Purchase.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

            return purchaseList;
        }

        public List<PurchaseView> GetPurchaseViewList(List<Purchase> purchaseList, string supplier, string status, string type, string isShowAll)
        {
            List<PurchaseView> purchaseViewList = new List<PurchaseView>();

            foreach(var item in purchaseList)
            {
                if(isShowAll.Equals("Yes"))
                {
                    PurchaseView purchaseView = new PurchaseView();

                    purchaseView.InvoiceId = item.InvoiceId;
                    purchaseView.DeliveryDate = item.DeliveryDate.Value;
                    purchaseView.Company = item.Company;
                    purchaseView.CompanyNo = item.CompanyNo;
                    purchaseView.Status = item.Status;
                    purchaseView.Type = item.Type;
                    purchaseView.Comment = item.Comment;
                    purchaseView.Total = GetTotalPurchases(item.InvoiceId);

                    if ((supplier.Equals("All") || supplier.Equals(item.Company)) && (status.Equals("All") || status.Equals(item.Status)) && (type.Equals("All") || type.Equals(item.Type)))
                    {
                        purchaseViewList.Add(purchaseView);
                    }
                }
                else
                {
                    if(!item.Status.Equals("Deleted"))
                    {
                        PurchaseView purchaseView = new PurchaseView();

                        purchaseView.InvoiceId = item.InvoiceId;
                        purchaseView.DeliveryDate = item.DeliveryDate.Value;
                        purchaseView.Company = item.Company;
                        purchaseView.CompanyNo = item.CompanyNo;
                        purchaseView.Status = item.Status;
                        purchaseView.Type = item.Type;
                        purchaseView.Comment = item.Comment;
                        purchaseView.Total = GetTotalPurchases(item.InvoiceId);

                        if ((supplier.Equals("All") || supplier.Equals(item.Company)) && (status.Equals("All") || status.Equals(item.Status)) && (type.Equals("All") || type.Equals(item.Type)))
                        {
                            purchaseViewList.Add(purchaseView);
                        }
                    }
                }
            }

            return purchaseViewList;
        }

        public double GetTotalPurchases(int invId)
        {
            double grandTotal = 0;

            List<PurchaseItem> purchaseItems = new List<PurchaseItem>();

            if(_context.GetMPSContext().PurchaseItem.Any(w => w.InvoiceId.Equals(invId)))
            {
                purchaseItems = _context.GetMPSContext().PurchaseItem.Where(w => w.InvoiceId.Equals(invId)).ToList();
            }
            
            if(purchaseItems.Count > 0)
            {
                foreach(var item in purchaseItems)
                {
                    //double taxRate = 2.0 - (_context.GetMPSContext().Tax.Where(w=>w.Code.Equals(item.Tax)).First().Rate.Value);

                    double taxRate = _context.GetMPSContext().Tax.Where(w => w.Code.Equals(item.Tax)).First().Rate.Value;

                    if (item.InvoicedQty.HasValue && item.Price.HasValue)
                    {
                        grandTotal = grandTotal + (item.InvoicedQty.Value * item.Price.Value * taxRate);
                    }
                }
            }

            //_logger.WriteTestLog("Returned Purchase total : $ " + grandTotal);

            return Math.Round(grandTotal, 2);
        }

        [HttpPost]
        public IActionResult GenInvoice(int invId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var purchase = _context.GetMPSContext().Purchase.Where(w=>w.InvoiceId.Equals(invId)).First();

            if(purchase.Status.Equals("New"))
            {
                purchase.Status = "Invoice";
            }

            purchase.UpdatedOn = DateTime.Now;
            purchase.UpdatedBy = userName;

            _context.GetMPSContext().Purchase.Update(purchase);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Purchase", "Adjustment", "Purchase ID#" + purchase.InvoiceId + " [Status] to Invoice");
            }

            return new JsonResult("New");
        }


        //End Methods
    }
}
