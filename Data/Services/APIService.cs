
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using Microsoft.Extensions.Logging;
using NLog.Targets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CarrotSystem.Services
{
    public interface IAPIService
    {
        string GetFullName(string loginId);
        decimal GetTaxRate(string taxCode);

        SalesTotalView GetInvoiceTotal(int invId);
        SalesView GetInvoice(int invId, string addressType);
        Period GetLatePeriod(DateTime baseDate);
        
        bool IsClosedPeriodByDate(DateTime baseDate);
        string IsClosedPeriodById(int periodId);
        void ClosePeriodByEndDate(DateTime baseDate, string updateBy);
        void OpenPeriodByEndDate(DateTime baseDate, string updateBy);
        
        Product GetProductById(int productId);
        Period GetPeriodById(int periodId);

        string GetExpenseTotal(int periodId);
        int GetProductIdByCode(string ProductCode);
        int GetLatePeriodId(string type);

        string GetStockGroupMinorName(int stockId);
        string GetStockGroupMainName(string productCode);
        string GetStockGroupSubName(int stockId);

        string GetStockGroupMainNameByMinorId(int minorId);
        string GetStockGroupSubNameByMinorId(int minorId);

        string GetLateUpdateDate(string type);
        string GetSupplierNameById(int supplierId);
        string GetLabourCodeByProductCode(string code);
        
        void CheckAndGeneratePeriod();
        void GeneratePeriods();
        bool IsStockCountable(string productCode);

        ClaimRefItem GetClaimDetails(string type, int claimId);
        List<ClaimRefItem> GetClaimRefList(string type);
        List<WasteReason> GetWasteReason();
        List<ProductJsonView> GetProductJsonViewList(string category);
        List<CustomisedProductItemModel> GetCustomisedProductList(string show, string category);
        List<Product> GetProductList(string show, string category);
        List<Company> GetCompanyList(string show, string status);

        List<StockCountView> GetStockCountList(string periodId);
        List<ExpenseDetailView> GetExpenseList(string periodId);
        List<SaleItemView> GetInvoiceItemList(int invId);
        List<DispatchItemView> GetDispatchList(int dispatchId);
        List<PeriodicView> GetPeriodicList(DateTime basis, string targetData);

        List<StockCount> GetStockCountActiveList(List<StockCount> baseCountList);

        DispatchView GetDispatchInformation(int dispatchId);

        PurchaseTotalView CalcPurchaseTotal(List<PurchaseItemView> purchaseItemList);
        PurchaseView GetPurchase(int id);
        List<PurchaseItemView> GetPurchaseItemList(int id, string company);

        bool IsMainGroup(string groupName, int minorId);
        bool IsSubGroup(string groupName, int minorId);
        bool IsMinorGroup(string groupName, int minorId);
    }

    public class APIService : IAPIService
    {
        private IContextService _context;
        private readonly IEventWriter _logger;
        public string userName = "";
        public string loginId = "";

        public APIService(IEventWriter logger, IContextService context)
        {
            _logger = logger;
            _context = context;
        }

        public APIService()
        {
        }

        public string GetFullName(string loginId)
        {
            var user = _context.GetMPSContext().Users.Where(w => w.LoginId.Equals(loginId)).First();
            var fullName = user.FirstName + " " + user.LastName;
            
            return fullName;
        }

        public PurchaseTotalView CalcPurchaseTotal(List<PurchaseItemView> purchaseItemList)
        {
            PurchaseTotalView total = new PurchaseTotalView();

            total.InvoiceTotal = 0;
            total.TaxTotal = 0;
            total.SubTotal = 0;

            foreach (var item in purchaseItemList)
            {
                if (item.InvoicedQty < 0)
                {
                    total.SubTotal = total.SubTotal + item.InvoiceTotal;
                    total.TaxTotal = total.TaxTotal - item.TaxTotal;
                    total.InvoiceTotal = total.InvoiceTotal + (item.InvoiceTotal - item.TaxTotal);
                }
                else
                {
                    total.SubTotal = total.SubTotal + item.InvoiceTotal;
                    total.TaxTotal = total.TaxTotal + item.TaxTotal;
                    total.InvoiceTotal = total.InvoiceTotal + (item.InvoiceTotal + item.TaxTotal);
                }

                //_logger.WriteTestLog("Inv Total : " + total.InvoiceTotal + ", Sub : " + total.SubTotal + ", Tax : " + total.TaxTotal);
            }

            return total;
        }

        public List<PurchaseItemView> GetPurchaseItemList(int id, string company)
        {
            List<PurchaseItemView> purchaseItemViewList = new List<PurchaseItemView>();
            List<PurchaseItem> purchaseItemList = new List<PurchaseItem>();

            purchaseItemList = _context.GetMPSContext().PurchaseItem.Where(w => w.InvoiceId.Equals(id)).ToList();

            foreach (var item in purchaseItemList)
            {
                PurchaseItemView purchaseItem = new PurchaseItemView();

                purchaseItem.ItemId = item.Id;
                purchaseItem.InvoiceId = item.InvoiceId.Value;

                var product = _context.GetMPSContext().Product.Where(w => w.Code.Equals(item.ProductCode)).First();

                purchaseItem.ProductId = product.Id;
                purchaseItem.ProductCode = product.Code;
                purchaseItem.ProductDesc = product.Desc;

                ProductMapping mappingProduct = new ProductMapping();

                if (_context.GetMPSContext().ProductMapping.Any(w => w.Company.Equals(company)))
                {
                    mappingProduct = _context.GetMPSContext().ProductMapping.Where(w => w.Company.Equals(company)).First();

                    purchaseItem.CompanyCode = mappingProduct.CompanyCode;
                    purchaseItem.CompanyDesc = mappingProduct.CompanyDesc;
                }
                else
                {
                    purchaseItem.CompanyCode = "";
                    purchaseItem.CompanyDesc = "";
                }

                if (item.InvoicedQty.HasValue)
                {
                    purchaseItem.InvoicedQty = item.InvoicedQty.Value;
                }
                else
                {
                    purchaseItem.InvoicedQty = 0;
                }

                if (item.Price.HasValue)
                {
                    purchaseItem.Price = (decimal)item.Price.Value;
                }
                else
                {
                    purchaseItem.Price = 0;
                }

                purchaseItem.Job = item.Job;
                purchaseItem.Tax = item.Tax;

                double taxRate = _context.GetMPSContext().Tax.Where(w => w.Code.Equals(item.Tax)).First().Rate.Value;

                //decimal taxedRate = (decimal) (2.0 - taxRate);
                decimal taxedRate = (decimal)(taxRate - 1.0);

                //purchaseItem.InvoiceTotal = (purchaseItem.Price * (decimal)purchaseItem.InvoicedQty) * taxedRate;
                purchaseItem.InvoiceTotal = purchaseItem.Price * (decimal)purchaseItem.InvoicedQty;

                //_logger.WriteTestLog("Total : " + purchaseItem.InvoiceTotal);

                if (purchaseItem.InvoicedQty < 0)
                {
                    purchaseItem.TaxTotal = purchaseItem.InvoiceTotal * (decimal)taxedRate * -1;
                }
                else
                {
                    purchaseItem.TaxTotal = purchaseItem.InvoiceTotal * (decimal)taxedRate;
                }

                //_logger.WriteTestLog("Taxed rate : " + taxedRate + ", " + purchaseItem.Tax + ", Tax Total : " + purchaseItem.TaxTotal);


                purchaseItemViewList.Add(purchaseItem);
            }

            return purchaseItemViewList;
        }

        public PurchaseView GetPurchase(int id)
        {
            PurchaseView purchaseView = new PurchaseView();

            Purchase purchase = new Purchase();
            purchase = _context.GetMPSContext().Purchase.Where(w => w.InvoiceId.Equals(id)).First();

            purchaseView.InvoiceId = purchase.InvoiceId;

            if (string.IsNullOrEmpty(purchase.Company))
            {
                purchaseView.CompanyId = 0;
                purchaseView.Company = "";
                purchaseView.SupplierName = "";
                purchaseView.InvoiceBN = "";
                purchaseView.BillingAddress = "";
                purchaseView.Type = purchase.Type;

                purchaseView.IsStockReturn = purchase.Returned.Value;
            }
            else
            {
                purchaseView.CompanyId = _context.GetMPSContext().Company.Where(w => w.CompanyName.Equals(purchase.Company)).First().Pk;
                purchaseView.Company = purchase.Company;
                purchaseView.SupplierName = purchase.Company;
                purchaseView.InvoiceBN = purchase.CompanyNo;
                purchaseView.Type = purchase.Type;

                purchaseView.IsStockReturn = purchase.Returned.Value;

                if (!string.IsNullOrEmpty(purchase.Comment))
                {
                    purchaseView.Comment = purchase.Comment;
                }

                if (purchase.ClaimReference > 0)
                {
                    purchaseView.ClaimRef = purchase.ClaimReference.ToString();
                }

                var billAddress = _context.GetMPSContext().Address.Where(w => w.Company.Equals(purchaseView.Company) && w.Type.Equals("Billing")).FirstOrDefault();
                if(billAddress != null)
                {
                    purchaseView.BillingAddress = billAddress.Street + " " + billAddress.City + " " + billAddress.State + " " + billAddress.Postcode;
                }
                else
                {
                    purchaseView.BillingAddress = "NA";
                }

            }

            purchaseView.Status = purchase.Status;
            purchaseView.DeliveryDate = purchase.DeliveryDate.Value;

            return purchaseView;
        }

        public void CheckAndGeneratePeriod()
        {
            DateTime today = DateTime.Now;
            
            if (_context.GetMPSContext().Period.OrderByDescending(o => o.EndDate.Value).First().EndDate.Value.Date.Equals(today.Date))
            {
                GeneratePeriods();
            }
            else if (!_context.GetMPSContext().Period.Any(w => today.Date.CompareTo(w.StartDate.Value.Date) >= 0 && today.Date.CompareTo(w.EndDate.Value.Date) <= 0))
            {
                GeneratePeriods();
            }
        }

        public string GetLabourCodeByProductCode(string code)
        { 
            var receiptList = _context.GetMPSContext().ProductRecipe.Where(w=>w.ProductCode.Equals(code)).ToList();

            if(receiptList.Count > 0)
            {
                foreach(var receipt in receiptList)
                {
                    var array = receipt.Component.ToArray();

                    if(array[0] == 'E' && array[1] == 'L')
                    {
                        return receipt.Component;
                    }
                }
            }

            return "N/A";
        }

        public Period GetPeriodById(int periodId)
        {
            return _context.GetMPSContext().Period.Where(w=>w.Id.Equals(periodId)).First();
        }

        public string GetSupplierNameById(int supplierId)
        {
            return _context.GetMPSContext().Company.Where(w=>w.Pk.Equals(supplierId)).First().CompanyName;
        }

        public Product GetProductById(int productId)
        {
            return _context.GetMPSContext().Product.Where(w=>w.Id.Equals(productId)).First();
        }

        public int GetProductIdByCode(string productCode)
        {
            return _context.GetMPSContext().Product.Where(w => w.Code.Equals(productCode)).First().Id;
        }

        public List<Company> GetCompanyList(string show, string status)
        {
            List<Company> companyList = new List<Company>();

            if (show.Equals("All") && status.Equals("All"))
            {
                companyList = _context.GetMPSContext().Company.Where(w=>w.Pk > 0).ToList();
            }
            else if (show.Equals("All") && (!status.Equals("All")))
            {
                if (status.Equals("Active"))
                {
                    companyList = _context.GetMPSContext().Company.Where(w => w.Inactive.Value.Equals(false)).ToList();
                }
                else if (status.Equals("Inactive"))
                {
                    companyList = _context.GetMPSContext().Company.Where(w => w.Inactive.Value.Equals(true)).ToList();
                }
            }
            else if ((!show.Equals("All")))
            {
                if (status.Equals("All"))
                {
                    companyList = _context.GetMPSContext().Company.Where(w => w.Type.Equals(show) || w.Type.Equals("CustSup")).ToList();
                }
                else if (status.Equals("Active"))
                {
                    companyList = _context.GetMPSContext().Company.Where(w => (w.Type.Equals(show) || w.Type.Equals("CustSup")) && w.Inactive.Value.Equals(false)).ToList();
                }
                else if (status.Equals("Inactive"))
                {
                    companyList = _context.GetMPSContext().Company.Where(w => (w.Type.Equals(show) || w.Type.Equals("CustSup")) && w.Inactive.Value.Equals(true)).ToList();
                }
            }

            return companyList;
        }

        public List<ProductJsonView> GetProductJsonViewList(string category)
        {
            List<ProductJsonView> returnList = new List<ProductJsonView>();

            List<Product> productList = new List<Product>();

            if(category.Equals("All"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Inactive.HasValue && w.Inactive.Value.Equals(false)).ToList();
            }
            else if(category.Equals("IF"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Inactive.HasValue && w.Inactive.Value.Equals(false) && w.Code.Contains("IF")).ToList();
            }
            else if (category.Equals("Sales"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Inactive.HasValue && w.Inactive.Value.Equals(false) && w.Code.Contains("IF")).ToList();
            }

            foreach (var product in productList)
            {
                ProductJsonView listItem = new ProductJsonView();

                listItem.ProductId = product.Id;
                listItem.ProductCode = product.Code;
                listItem.ProductDesc = product.Desc;

                if (_context.GetMPSContext().ProductMapping.Any(a => a.MercCode.Equals(listItem.ProductCode)))
                {
                    var mapping = _context.GetMPSContext().ProductMapping.First(a => a.MercCode.Equals(listItem.ProductCode));

                    listItem.CustomerCode = mapping.CompanyCode;
                    listItem.CustomerDesc = mapping.CompanyDesc;
                }
                else
                {
                    listItem.CustomerCode = "N/A";
                    listItem.CustomerDesc = "N/A";
                }

                returnList.Add(listItem);
            }

            return returnList;
        }

        public Period GetLatePeriod(DateTime baseDate)
        {
            //_logger.WriteTestLog("Data Here Base Date : " + baseDate.Date.ToString("dd/MM/yyyy"));
            
            Period returnPeriod = new Period();

            if(_context.GetMPSContext().Period.Any(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0))
            {
                returnPeriod = _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();
            }
            else
            {
                GeneratePeriods();
                returnPeriod = _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();
            }

            return returnPeriod;
        }

        public void GeneratePeriods()
        {
            var lastPeriod = _context.GetMPSContext().Period.Where(w=>w.Id>0).OrderByDescending(o=>o.Id).First();

            for(int cntPeriods = 1; cntPeriods<5; cntPeriods++)
            {
                Period period = new Period();

                //period.Id = lastPeriod.Id + cntPeriods;
                period.StartDate = lastPeriod.StartDate.Value.Date.AddDays(7 * cntPeriods);
                period.EndDate = lastPeriod.EndDate.Value.Date.AddDays(7 * cntPeriods);
                
                period.Calculated = false;
                period.Status = "Open";
                period.UpdatedBy = "System";
                period.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().Period.Add(period);
                _context.GetMPSContext().SaveChanges();
            }
        }

        public void ClosePeriodByEndDate(DateTime baseDate, string updateBy)
        {
            Period period = new Period();
            period = _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();

            period.Status = "Closed";
            period.UpdatedBy = updateBy;
            period.UpdatedOn = DateTime.Now;

            _context.GetMPSContext().Period.Update(period);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Period", "Adjustment", "Period ID#" + period.Id + " Closed");
            }
        }

        public void OpenPeriodByEndDate(DateTime baseDate, string updateBy)
        {
            Period period = new Period();
            period = _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();

            period.Status = "Open";
            period.UpdatedBy = updateBy;
            period.UpdatedOn = DateTime.Now;

            _context.GetMPSContext().Period.Update(period);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Period", "Adjustment", "Period ID#" + period.Id + " Opened");
            }
        }

        public bool IsClosedPeriodByDate (DateTime baseDate)
        {
            Period period = GetLatePeriod(baseDate);

            if (period.Status.Equals("Closed"))
            {
                return true;
            }
            else
            { 
                return false;
            }
        }

        public List<Product> GetProductList(string show, string category)
        {
            //Show : All Active Inactive

            List<Product> productList = new List<Product>();

            if (show.Equals("All"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Id > 0).ToList();
            }
            else if (show.Equals("Active"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Inactive.Value.Equals(false)).ToList();
            }
            else if (show.Equals("Inactive"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Inactive.Value.Equals(true)).ToList();
            }

            if(category.Equals("IF"))
            {
                productList = productList.Where(w => w.Code.Contains("IF")).ToList();
            }
            else if (category.Equals("Packing"))
            {
                productList = productList.Where(w => IsMainGroup("RAW", w.MinorGroupId.Value) || IsMainGroup("FINISHED", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("Transfer"))
            {
                productList = productList.Where(w => IsMainGroup("RAW", w.MinorGroupId.Value) || IsMainGroup("FINISHED", w.MinorGroupId.Value) || IsMainGroup("WHOLESALE", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("Waste"))
            {
                productList = productList.Where(w => IsMainGroup("RAW", w.MinorGroupId.Value) || IsMainGroup("FINISHED", w.MinorGroupId.Value) || IsMainGroup("WHOLESALE", w.MinorGroupId.Value) || IsMainGroup("PACKING", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("StockCount"))
            {
                //productList = productList.Where(w => w.Code.Contains("IF")).ToList();
            }
            else if (category.Equals("Expenses"))
            {
                productList = productList.Where(w => IsMainGroup("EXPENSE", w.MinorGroupId.Value)).ToList();
            }

            return productList;
        }

        public List<CustomisedProductItemModel> GetCustomisedProductList(string show, string category)
        {
            //Show : All Active Inactive

            List<Product> productList = new List<Product>();
            List<CustomisedProductItemModel> customisedProductList = new List<CustomisedProductItemModel>();

            if (show.Equals("All"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Id > 0).ToList();
            }
            else if (show.Equals("Active"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Inactive.HasValue && w.Inactive.Value.Equals(false)).ToList();
            }
            else if (show.Equals("Inactive"))
            {
                productList = _context.GetMPSContext().Product.Where(w => w.Inactive.HasValue && w.Inactive.Value.Equals(true)).ToList();
            }

            if (category.Equals("IF"))
            {
                productList = productList.Where(w => w.Code.Contains("IF")).ToList();
            }
            else if (category.Equals("Purchase"))
            {
                productList = productList.Where(w => IsSubGroup("PURCHASE", w.MinorGroupId.Value) || IsMainGroup("RAW", w.MinorGroupId.Value) || IsMainGroup("WHOLESALE", w.MinorGroupId.Value) || IsMainGroup("FREIGHT", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("Sales"))
            {
                productList = productList.Where(w => IsSubGroup("SALE", w.MinorGroupId.Value) || IsMainGroup("FINISHED", w.MinorGroupId.Value) || IsMainGroup("WHOLESALE", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("Packing"))
            {
                productList = productList.Where(w => IsMainGroup("RAW", w.MinorGroupId.Value) || IsMainGroup("FINISHED", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("Transfer"))
            {
                productList = productList.Where(w => IsMainGroup("RAW", w.MinorGroupId.Value) || IsMainGroup("FINISHED", w.MinorGroupId.Value) || IsMainGroup("WHOLESALE", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("Waste"))
            {
                productList = productList.Where(w => IsMainGroup("RAW", w.MinorGroupId.Value) || IsMainGroup("FINISHED", w.MinorGroupId.Value) || IsMainGroup("WHOLESALE", w.MinorGroupId.Value) || IsMainGroup("PACKING", w.MinorGroupId.Value)).ToList();
            }
            else if (category.Equals("StockCount"))
            {
                //productList = GetStockCountActiveList(productList);
            }
            else if (category.Equals("Expenses"))
            {
                productList = productList.Where(w => IsMainGroup("EXPENSE", w.MinorGroupId.Value)).ToList();
            }

            int maxProductCodeNameLenght = GetProductCodeMaxLenght(productList);

            if(productList.Count > 0)
            {
                foreach(var product in productList.OrderByDescending(o=>o.MinorGroupId.Value))
                {
                    CustomisedProductItemModel custItem = new CustomisedProductItemModel();

                    custItem.Id = product.Id;
                    custItem.Code = product.Code;
                    custItem.Desc = product.Desc;
                    
                    int blankAmount = maxProductCodeNameLenght - custItem.Code.Length;
                    var blank = "&nbsp;";

                    if(blankAmount > 0)
                    {
                        blank = "&nbsp;&emsp;";
                    }

                    custItem.DisplayProductName = custItem.Code + blank + "&emsp;&nbsp;&nbsp;&emsp;" + custItem.Desc;

                    customisedProductList.Add(custItem);

                }
            }

            //_logger.LogError("cstPd List : " + customisedProductList.Count);

            return customisedProductList.OrderBy(o=>o.Code).ToList();
        }

        public SalesTotalView GetInvoiceTotal(int invId)
        {
            
            decimal invoiceSubTotal = 0, invoiceTaxTotal = 0, invoiceTotal = 0;
            decimal grandTotal = 0;

            if (invId > 0)
            {
                List<SaleItem> saleItemList = new List<SaleItem>();
                saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Equals(invId)).ToList();

                if (saleItemList.Count > 0)
                {
                    foreach (var sale in saleItemList)
                    {
                        decimal invoiceSubItem = 0, invoiceTaxItem = 0, invoiceItemTotal = 0;
                        
                        if (sale.InvoicedQty.HasValue && sale.Price.HasValue)
                        {
                            invoiceSubItem = ((decimal)sale.InvoicedQty.Value * (decimal)sale.Price.Value);
                        }

                        decimal taxRate = GetTaxRate(sale.Tax);

                        invoiceTaxItem = (((decimal)sale.InvoicedQty.Value * (decimal)sale.Price.Value) * (1 - taxRate));

                        invoiceItemTotal = invoiceSubItem + invoiceTaxItem;

                        //Grand Total
                        invoiceSubTotal = invoiceSubTotal + invoiceSubItem;
                        invoiceTaxTotal = invoiceTaxTotal + invoiceTaxItem;
                        invoiceTotal = invoiceTotal + invoiceItemTotal;

                        grandTotal = grandTotal + invoiceItemTotal;
                    }
                }
            }

            SalesTotalView returnTotal = new SalesTotalView();

            returnTotal.InvoiceSubTotal = invoiceSubTotal;
            returnTotal.InvoiceTaxTotal = invoiceTaxTotal;
            returnTotal.InvoiceTotal = invoiceTotal;
            returnTotal.Total = grandTotal;

            return returnTotal;
        }

        public string GetExpenseTotal(int periodId)
        {
            List<ExpenseDetailView> expenseViewList = new List<ExpenseDetailView>();

            expenseViewList = GetExpenseList(periodId.ToString());

            double total = 0;

            foreach (var exp in expenseViewList)
            {
                total = total + exp.Price;
            }

            return String.Format("{0:#,0.00}", total);
        }

        public SalesView GetInvoice(int invId, string addressType)
        {
            SalesView invoice = new SalesView();

            var sale = _context.GetMPSContext().Sale.First(f => f.InvoiceId.Equals(invId));

            invoice.InvoiceId = sale.InvoiceId;
            invoice.ShippingDate = sale.ArrivalDate.Value;
            invoice.DeliveryDate = sale.DeliveryDate.Value;
            invoice.ArrivalDate = sale.ArrivalDate.Value;

            //Company Information
            Address address = new Address();

            if (addressType.Equals("Billing"))
            {
                if(_context.GetMPSContext().Address.Any(w => w.Type.Equals("Billing") && w.Company.Equals(sale.Company)))
                {
                    address = _context.GetMPSContext().Address.Where(w => w.Type.Equals("Billing") && w.Company.Equals(sale.Company)).First();
                }
            }
            else
            {
                if(_context.GetMPSContext().Address.Any(w => w.Id.Equals(sale.ShippingAddress.Value)))
                {
                    address = _context.GetMPSContext().Address.Where(w => w.Id.Equals(sale.ShippingAddress.Value)).First();
                }
            }

            invoice.AddId = address.Id;
            invoice.State = address.State;
            invoice.Address = address.Street + ", " + address.City + ", " + address.State + ", " + address.Country + " " + address.Postcode;
            invoice.Customer = sale.Company;

            var company = _context.GetMPSContext().Company.Where(w => w.CompanyName.Equals(sale.Company)).First();
            invoice.CustId = company.Pk;

            if (string.IsNullOrEmpty(company.Abn))
            {
                invoice.ABN = " ";
            }
            else
            {
                invoice.ABN = company.Abn;
            }

            if (string.IsNullOrEmpty(company.VendorNumber))
            {
                invoice.SupplierCode = " ";
            }
            else
            {
                invoice.SupplierCode = company.VendorNumber;
            }

            if (sale.Revision.HasValue)
            {
                invoice.Revision = sale.Revision.Value;
            }

            invoice.CustPO = sale.CompanyNo;
            invoice.Status = sale.Status;
            invoice.Type = sale.Type;

            if (sale.ClaimReference.HasValue)
            {
                invoice.ClaimRef = sale.ClaimReference.Value.ToString();
            }

            if (!string.IsNullOrEmpty(sale.Comment))
            {
                invoice.Comment = sale.Comment;
            }

            //Invoice Item List
            invoice.invTotal = GetInvoiceTotal(invoice.InvoiceId);
            invoice.cratesTotal = GetCratesTotal(GetInvoiceItemList(invoice.InvoiceId));

            return invoice;
        }

        public List<SaleItemView> GetInvoiceItemList(int invId)
        {
            List<SaleItemView> invItemList = new List<SaleItemView>();

            List<SaleItem> saleItemList = new List<SaleItem>();
            saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Equals(invId)).ToList();

            if (saleItemList.Count > 0)
            {
                foreach (var item in saleItemList)
                {
                    SaleItemView invItem = new SaleItemView();

                    invItem.ItemId = item.Id;
                    invItem.InvoiceId = item.InvoiceId.Value;
                    invItem.ProductId = _context.GetMPSContext().Product.Where(w => w.Code.Equals(item.ProductCode)).First().Id;

                    invItem.ProductCode = item.ProductCode;
                    invItem.ProductDesc = item.ProductDesc;
                    invItem.Price = item.Price.Value;

                    if (_context.GetMPSContext().ProductMapping.Any(a => a.MercCode.Equals(invItem.ProductCode)))
                    {
                        var productMapping = _context.GetMPSContext().ProductMapping.First(f => f.MercCode.Equals(invItem.ProductCode));

                        invItem.CustomerCode = productMapping.CompanyCode;
                        invItem.CustomerDesc = productMapping.CompanyDesc;
                    }

                    invItem.Job = item.Job;
                    invItem.Tax = item.Tax;
                    invItem.CratesType = GetCratesType(item.ProductCode);

                    decimal taxRate = GetTaxRate(invItem.Tax);

                    if (item.OrderedQty.HasValue)
                    {
                        invItem.OrderedQty = item.OrderedQty.Value;
                        invItem.OrderedTotal = ((decimal)invItem.OrderedQty * (decimal)item.Price.Value) * taxRate;
                    }
                    else
                    {
                        invItem.OrderedTotal = 0;
                    }

                    if (item.InvoicedQty.HasValue)
                    {
                        invItem.InvoicedQty = item.InvoicedQty.Value;
                        invItem.InvoiceTotal = ((decimal)invItem.InvoicedQty * (decimal)item.Price.Value) * taxRate;
                        invItem.Gst = (invItem.InvoicedQty * item.Price.Value) * (double)(1 - taxRate);
                    }
                    else
                    {
                        invItem.Gst = 0;
                    }

                    if (item.FreightProportion.HasValue)
                    {
                        invItem.FreightProportion = item.FreightProportion.Value;
                    }

                    if (item.SortId.HasValue)
                    {
                        invItem.SortID = item.SortId.Value;
                    }

                    invItem.UpdatedBy = item.UpdatedBy;

                    if (item.UpdatedOn.HasValue)
                    {
                        invItem.UpdatedOn = item.UpdatedOn.Value;
                    }

                    invItemList.Add(invItem);
                }
            }

            return invItemList;
        }

        public decimal GetTaxRate(string taxCode)
        {
            return (decimal)_context.GetMPSContext().Tax.First(f => f.Code.Equals(taxCode)).Rate.Value;
        }

        public string GetCratesType(string productCode)
        {
            string returnType = "N/A";
            //IPCRATEB
            if(_context.GetMPSContext().ProductRecipe.Any(w => w.ProductCode.Equals(productCode) && w.Component.Contains("IPCRATE")))
            {
                if(_context.GetMPSContext().ProductRecipe.Any(w => w.ProductCode.Equals(productCode) && w.Component.Contains("IPCRATEA")))
                {
                    returnType = "CratesA";
                }
                else if (_context.GetMPSContext().ProductRecipe.Any(w => w.ProductCode.Equals(productCode) && w.Component.Contains("IPCRATEB")))
                {
                    returnType = "CratesB";
                }
                else if (_context.GetMPSContext().ProductRecipe.Any(w => w.ProductCode.Equals(productCode) && w.Component.Contains("IPCRATEC")))
                {
                    returnType = "CratesC";
                }
                else if (_context.GetMPSContext().ProductRecipe.Any(w => w.ProductCode.Equals(productCode) && w.Component.Contains("IPCRATED")))
                {
                    returnType = "CratesD";
                }
                else if (_context.GetMPSContext().ProductRecipe.Any(w => w.ProductCode.Equals(productCode) && w.Component.Contains("IPCRATEE")))
                {
                    returnType = "CratesE";
                }
                else if (_context.GetMPSContext().ProductRecipe.Any(w => w.ProductCode.Equals(productCode) && w.Component.Contains("IPCRATEF")))
                {
                    returnType = "CratesF";
                }
            }

            return returnType;
        }

        public SalesCratesTotalView GetCratesTotal(List<SaleItemView> itemList)
        {
            SalesCratesTotalView total = new SalesCratesTotalView();

            total.CratesA = 0;
            total.CratesB = 0;
            total.CratesC = 0;
            total.CratesD = 0;
            total.CratesE = 0;
            total.CratesF = 0;

            var totalList = itemList.GroupBy(g => new { Type = g.CratesType })
                             .Select(s => new {
                                 qty = s.Sum(b => b.InvoicedQty),
                                 type = s.Key.Type,
                             }).ToList();

            if(totalList.Count > 0)
            {
                if(totalList.Any(w=>w.type.Equals("CratesA")))
                {
                    total.CratesA = (int)totalList.Where(w => w.type.Equals("CratesA")).First().qty;
                }

                if (totalList.Any(w => w.type.Equals("CratesB")))
                {
                    total.CratesB = (int)totalList.Where(w => w.type.Equals("CratesB")).First().qty;
                }

                if (totalList.Any(w => w.type.Equals("CratesC")))
                {
                    total.CratesC = (int)totalList.Where(w => w.type.Equals("CratesC")).First().qty;
                }

                if (totalList.Any(w => w.type.Equals("CratesD")))
                {
                    total.CratesD = (int)totalList.Where(w => w.type.Equals("CratesD")).First().qty;
                }

                if (totalList.Any(w => w.type.Equals("CratesE")))
                {
                    total.CratesE = (int)totalList.Where(w => w.type.Equals("CratesE")).First().qty;
                }

                if (totalList.Any(w => w.type.Equals("CratesF")))
                {
                    total.CratesF = (int)totalList.Where(w => w.type.Equals("CratesF")).First().qty;
                }

            }

            return total;
        }

        public List<DispatchItemView> GetDispatchList(int dispatchId)
        {
            var dispatchItemList = _context.GetMPSContext().SaleDispatchItem.Where(w=>w.DispatchId.Equals(dispatchId)).ToList();

            List<DispatchItemView> disItemList = new List<DispatchItemView>();
            //Sort MercCode MercDesc Ordered Unfilled Dispatch Temp Grower BestBefore

            foreach (var item in dispatchItemList)
            {
                List<SaleDispatchItem> prvDisItem = new List<SaleDispatchItem>();
                int preFilled = 0;

                if (_context.GetMPSContext().SaleDispatchItem.Any(n => n.SaleItemId.Equals(item.SaleItemId)))
                {
                    prvDisItem = _context.GetMPSContext().SaleDispatchItem.Where(w => w.SaleItemId.Equals(item.SaleItemId)).OrderBy(o => o.UpdatedOn).ToList();

                    foreach (var filled in prvDisItem)
                    {
                        preFilled = preFilled + filled.Qty.Value;
                    }
                }
                else
                {
                    preFilled = 0;
                }

                //_logger.LogError("Dispatch PRV Item Qty: " + preFilled + ", item ID : " + item.Id);

                if(_context.GetMPSContext().SaleItem.Any(w => w.Id.Equals(item.SaleItemId)))
                {
                    var saleItem = _context.GetMPSContext().SaleItem.Where(w => w.Id.Equals(item.SaleItemId)).First();

                    DispatchItemView disItem = new DispatchItemView();

                    disItem.DispatchItemId = item.Id;
                    disItem.DispatchId = item.DispatchId.Value;
                    disItem.SaleItemId = item.SaleItemId.Value;
                    if (item.SortId.HasValue)
                    {
                        disItem.SortId = item.SortId.Value;
                    }
                    else
                    {
                        disItem.SortId = 0;
                    }

                    disItem.ProductCode = saleItem.ProductCode;
                    disItem.CustomerCode = saleItem.CompanyCode;
                    disItem.ProductDesc = saleItem.ProductDesc;
                    disItem.CustomerDesc = saleItem.CompanyDesc;

                    disItem.Ordered = (float)saleItem.InvoicedQty.Value;
                    disItem.Unfilled = (disItem.Ordered - (float)preFilled);
                    disItem.Dispatch = item.Qty.Value;

                    if (string.IsNullOrEmpty(item.Grower))
                    {
                        disItem.Grower = "N/A";
                    }
                    else
                    {
                        disItem.Grower = item.Grower;
                    }

                    if (item.BestBefore.HasValue)
                    {
                        disItem.BestBefore = item.BestBefore.Value;
                        disItem.StringBestBefore = item.BestBefore.Value.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        disItem.StringBestBefore = "N/A";
                    }

                    disItemList.Add(disItem);
                }
                else
                {

                }
            }

            return disItemList;
        }

        public DispatchView GetDispatchInformation(int dispatchId)
        {
            DispatchView information = new DispatchView();
            information.DispatchId = dispatchId;

            var dispatch = _context.GetMPSContext().SaleDispatch.Where(w=>w.DispatchId.Equals(information.DispatchId)).First();
            var sale = _context.GetMPSContext().Sale.Where(w => w.InvoiceId.Equals(dispatch.SaleInvoiceId)).First();
            var company = _context.GetMPSContext().Company.Where(w=>w.CompanyName.Equals(sale.Company)).First();
            if(sale.ShippingAddress != 0)
            {
                var address = _context.GetMPSContext().Address.Where(w=>w.Id.Equals(sale.ShippingAddress.Value)).First();
                information.ShippingAddress = address.Street + " " + address.City + " " + address.State + " " + address.Postcode;
            }

            information.InvoiceId = sale.InvoiceId;
            information.CustPO = sale.CompanyNo;
            information.ShippingAddress = "NA";
            
            if(!string.IsNullOrEmpty(company.VendorNumber))
            {
                information.VendorNumber = company.VendorNumber;
            }

            information.CustomerName = sale.Company;
            
            if(dispatch.DispatchDate.HasValue)
            {
                information.DispatchDate = dispatch.DispatchDate.Value;
            }
            else
            {
                information.DispatchDate = sale.DeliveryDate.Value;
            }
            information.Comment = sale.Comment;

            //_logger.WriteTestLog("Dispatch Information 5: " + dispatchId);

            return information;
        }

        public int GetLatePeriodId(string type)
        {
            //Transfer Waste

            if (type.Equals("Packing"))
            {
                var target = _context.GetMPSContext().ProductPacking.Where(w=>w.UpdatedOn.HasValue).OrderByDescending(d=>d.UpdatedOn).FirstOrDefault();

                if(target != null)
                {
                    return _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && target.PackingDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && target.PackingDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).FirstOrDefault().Id;
                }
                else
                {
                    return 0;
                }
            }
            else if (type.Equals("Transfer"))
            {
                var target = _context.GetMPSContext().ProductTransfer.Where(w => w.UpdatedOn.HasValue).OrderByDescending(d => d.UpdatedOn).FirstOrDefault();

                if (target != null)
                {
                    return _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && target.TransferDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && target.TransferDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).FirstOrDefault().Id;
                }
                else
                {
                    return 0;
                }
            }
            else if (type.Equals("Waste"))
            {
                var target = _context.GetMPSContext().Waste.Where(w => w.UpdatedOn.HasValue).OrderByDescending(d => d.UpdatedOn).FirstOrDefault();

                if (target != null)
                {
                    return _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && target.WasteDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && target.WasteDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).FirstOrDefault().Id;
                }
                else
                {
                    return 0;
                }
            }
            else if (type.Equals("Expenses"))
            {
                var target = _context.GetMPSContext().Expense.Where(w => w.PeriodId.HasValue).OrderByDescending(d => d.PeriodId.Value).FirstOrDefault();

                if (target != null)
                {
                    return target.PeriodId.Value;
                }
                else
                {
                    return 0;
                }
            }
            else if (type.Equals("Export"))
            {
                var target = _context.GetMPSContext().Myoblog.Where(w => w.ExportedOn.HasValue).OrderByDescending(d => d.ExportedOn).FirstOrDefault();

                if (target != null)
                {
                    return target.PeriodId.Value;
                }
                else
                {
                    return 0;
                }
            }
            else if (type.Equals("ExpenseNew"))
            {
                var target = _context.GetMPSContext().Expense.Where(w => w.PeriodId.HasValue).OrderByDescending(d => d.PeriodId.Value).FirstOrDefault();

                if(target != null)
                {
                    if (_context.GetMPSContext().Period.Any(w => w.Id.Equals(target.PeriodId.Value + 1)))
                    {
                        return target.PeriodId.Value + 1;
                    }
                    else
                    {
                        var currentLast = _context.GetMPSContext().Period.OrderByDescending(o => o.Id).FirstOrDefault();

                        Period newPeriod = new Period();
                        newPeriod.Calculated = false;
                        newPeriod.StartDate = currentLast.StartDate.Value.AddDays(7);
                        newPeriod.EndDate = currentLast.EndDate.Value.AddDays(7);
                        newPeriod.Status = "Open";
                        newPeriod.UpdatedBy = "System";
                        newPeriod.UpdatedOn = DateTime.Now;

                        _context.GetMPSContext().Period.Add(newPeriod);
                        _context.GetMPSContext().SaveChanges();

                        return target.PeriodId.Value + 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }

        }

        public string GetLateUpdateDate(string type)
        {
            if (type.Equals("Packing"))
            {
                var target = _context.GetMPSContext().ProductPacking.Where(w => w.UpdatedOn.HasValue).OrderByDescending(d => d.UpdatedOn).FirstOrDefault();

                if (target != null)
                {
                    return target.PackingDate.Value.ToString("ddMMyyyy");
                }
                else
                {
                    return DateTime.Now.ToString("ddMMyyyy");
                }
            }
            else if (type.Equals("Transfer"))
            {
                var target = _context.GetMPSContext().ProductTransfer.Where(w => w.UpdatedOn.HasValue).OrderByDescending(d => d.UpdatedOn).FirstOrDefault();

                if (target != null)
                {
                    return target.TransferDate.Value.ToString("ddMMyyyy");
                }
                else
                {
                    return DateTime.Now.ToString("ddMMyyyy");
                }
            }
            else if (type.Equals("Waste"))
            {
                var target = _context.GetMPSContext().Waste.Where(w => w.UpdatedOn.HasValue).OrderByDescending(d => d.UpdatedOn).FirstOrDefault();

                if (target != null)
                {
                    return target.WasteDate.Value.ToString("ddMMyyyy"); ;
                }
                else
                {
                    return DateTime.Now.ToString("ddMMyyyy");
                }
            }
            else if (type.Equals("Export"))
            {
                var target = _context.GetMPSContext().Myoblog.Where(w => w.ExportedOn.HasValue).OrderByDescending(d => d.ExportedOn).FirstOrDefault();

                if (target != null)
                {
                    return GetPeriodById(target.PeriodId.Value).EndDate.Value.ToString("ddMMyyyy");
                }
                else
                {
                    return DateTime.Now.ToString("ddMMyyyy");
                }
            }
            else
            {
                return "None";
            }
        }

        public List<PeriodicView> GetPeriodicList(DateTime basis, string targetData)
        {
            List<PeriodicView> periodList = new List<PeriodicView>();

            //DateTime dateFrom = DateTime.ParseExact("01/07/" + basis.AddYears(-1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //DateTime dateTo = DateTime.ParseExact("30/06/" + basis.AddYears(1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            DateTime dateFrom = basis.AddMonths(-1);
            DateTime dateTo = basis;

            if (targetData.Equals("Packing"))
            {
                List<ProductPacking> packingList = new List<ProductPacking>();
                packingList = _context.GetMPSContext().ProductPacking.Where(w => w.PackingDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.PackingDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                if (packingList.Count > 0)
                {
                    foreach (var packing in packingList)
                    {
                        Period period = new Period();

                        if (_context.GetMPSContext().Period.Any(w => packing.PackingDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && packing.PackingDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0))
                        {
                            period = _context.GetMPSContext().Period.Where(w => packing.PackingDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && packing.PackingDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();

                            if(periodList.Any(a=>a.TargetDate.Date.Equals(packing.PackingDate.Value.Date) && a.PeriodId.Equals(period.Id)))
                            {

                            }
                            else
                            {
                                PeriodicView periodicView = new PeriodicView();
                                periodicView.TargetId = packing.Id;
                                
                                //periodicView.TargetDate = period.EndDate.Value;
                                // Showing only End of period Date - 12/07/22
                                periodicView.TargetDate = packing.PackingDate.Value.Date;
                                periodicView.PeriodId = period.Id;
                                periodicView.Status = period.Status;

                                periodList.Add(periodicView);
                            }
                        }
                    }
                }
            }
            else if (targetData.Equals("Transfer"))
            {
                List<ProductTransfer> transferList = new List<ProductTransfer>();
                transferList = _context.GetMPSContext().ProductTransfer.Where(w => w.TransferDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.TransferDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                if (transferList.Count > 0)
                {
                    foreach (var transfer in transferList)
                    {
                        Period period = new Period();

                        if (_context.GetMPSContext().Period.Any(w => transfer.TransferDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && transfer.TransferDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0))
                        {
                            period = _context.GetMPSContext().Period.Where(w => transfer.TransferDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && transfer.TransferDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();

                            if (periodList.Any(a => a.TargetDate.Date.Equals(transfer.TransferDate.Value.Date) && a.PeriodId.Equals(period.Id)))
                            {

                            }
                            else
                            {
                                PeriodicView periodicView = new PeriodicView();
                                periodicView.TargetId = transfer.Id;
                                periodicView.TargetDate = transfer.TransferDate.Value.Date;
                                periodicView.PeriodId = period.Id;
                                periodicView.Status = period.Status;

                                periodList.Add(periodicView);
                            }
                        }
                    }
                }
            }
            else if (targetData.Equals("Waste"))
            {
                List<Waste> wastelist = new List<Waste>();
                wastelist = _context.GetMPSContext().Waste.Where(w => w.WasteDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.WasteDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                if (wastelist.Count > 0)
                {
                    foreach (var waste in wastelist)
                    {
                        Period period = new Period();

                        if (_context.GetMPSContext().Period.Any(w => waste.WasteDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && waste.WasteDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0))
                        {
                            period = _context.GetMPSContext().Period.Where(w => waste.WasteDate.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && waste.WasteDate.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();

                            if (periodList.Any(a => a.TargetDate.Date.Equals(waste.WasteDate.Value.Date) && a.PeriodId.Equals(period.Id)))
                            {

                            }
                            else
                            {
                                PeriodicView periodicView = new PeriodicView();
                                periodicView.TargetId = waste.Id;
                                periodicView.TargetDate = waste.WasteDate.Value.Date;
                                periodicView.PeriodId = period.Id;
                                periodicView.Status = period.Status;

                                periodList.Add(periodicView);
                            }
                        }
                    }
                }
            }
            else if (targetData.Equals("StockCount"))
            {
                List<StockCount> stockCountList = new List<StockCount>();
                stockCountList = _context.GetMPSContext().StockCount.Where(w => w.UpdatedOn.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.UpdatedOn.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                if (stockCountList.Count > 0)
                {
                    foreach (var stockCount in stockCountList)
                    {
                        Period period = new Period();

                        if (_context.GetMPSContext().Period.Any(w => stockCount.UpdatedOn.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && stockCount.UpdatedOn.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0))
                        {
                            period = _context.GetMPSContext().Period.Where(w => stockCount.UpdatedOn.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && stockCount.UpdatedOn.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();

                            if (periodList.Any(a => a.TargetDate.Date.Equals(period.EndDate.Value.Date) && a.PeriodId.Equals(period.Id)))
                            {

                            }
                            else
                            {
                                PeriodicView periodicView = new PeriodicView();
                                periodicView.TargetId = stockCount.Id;
                                periodicView.TargetDate = period.EndDate.Value.Date;
                                //periodicView.TargetDate = stockCount.UpdatedOn.Value.Date;
                                periodicView.PeriodId = period.Id;
                                periodicView.Status = period.Status;

                                periodList.Add(periodicView);
                            }
                        }
                    }
                }
            }
            else if (targetData.Equals("Expenses"))
            {
                List<Expense> expenseList = new List<Expense>();
                expenseList = _context.GetMPSContext().Expense.Where(w => w.UpdatedOn.Value.Date.CompareTo(dateFrom.Date) >= 0).ToList();

                if (expenseList.Count > 0)
                {
                    foreach (var expense in expenseList)
                    {
                        Period period = new Period();

                        if (_context.GetMPSContext().Period.Any(w => w.Id.Equals(expense.PeriodId.Value)))
                        {
                            period = _context.GetMPSContext().Period.Where(w => w.Id.Equals(expense.PeriodId.Value)).First();

                            if (periodList.Any(a => a.TargetDate.Date.Equals(period.EndDate.Value) && a.PeriodId.Equals(period.Id)))
                            {

                            }
                            else
                            {
                                PeriodicView periodicView = new PeriodicView();
                                periodicView.TargetId = expense.Id;
                                periodicView.TargetDate = period.EndDate.Value;
                                periodicView.PeriodId = period.Id;
                                periodicView.Status = period.Status;

                                periodList.Add(periodicView);
                            }
                        }
                    }
                }
            }
            else if (targetData.Equals("Export"))
            {
                dateTo = dateTo.AddDays(14);

                var pdList = _context.GetMPSContext().Period.Where(w => w.EndDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.EndDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                foreach (var period in pdList)
                {
                    PeriodicView periodicView = new PeriodicView();
                    periodicView.TargetId = 0;
                    periodicView.TargetDate = period.EndDate.Value;
                    periodicView.PeriodId = period.Id;
                    periodicView.Status = period.Status;

                    periodList.Add(periodicView);
                }
                /*
                List<Myoblog> logList = new List<Myoblog>();
                logList = _context.GetMPSContext().Myoblog.Where(w => w.ExportedOn.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.ExportedOn.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                if (logList.Count > 0)
                {
                    foreach (var myobLog in logList)
                    {
                        Period period = new Period();

                        if (_context.GetMPSContext().Period.Any(w => myobLog.ExportedOn.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && myobLog.ExportedOn.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0))
                        {
                            period = _context.GetMPSContext().Period.Where(w => myobLog.ExportedOn.Value.Date.CompareTo(w.StartDate.Value.Date) >= 0 && myobLog.ExportedOn.Value.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();

                            if (periodList.Any(a => a.TargetDate.Date.Equals(myobLog.ExportedOn.Value.Date) && a.PeriodId.Equals(period.Id)))
                            {

                            }
                            else
                            {
                                PeriodicView periodicView = new PeriodicView();
                                periodicView.TargetId = myobLog.InvoiceId.Value;
                                periodicView.TargetDate = myobLog.ExportedOn.Value.Date;
                                periodicView.PeriodId = period.Id;
                                periodicView.Status = period.Status;

                                periodList.Add(periodicView);
                            }
                        }
                    }
                }
                */


            }

            //var returnPeriodList = (from period in periodList select period).ToList().Distinct();
            /*    
                taskList.GroupBy(g => new { Division = g.Division, Category = g.Category })
                            .Select(s => new {
                                totalTimeMinutes = s.Sum(b => b.Duration.Value.TotalMinutes),
                                category = _context.GetTaskContext().Category.Where(w => w.Code.Equals(s.Key.Category)).First().Rate,
                                totalAmount = Math.Round(_context.GetTaskContext().Category.Where(w => w.Code.Equals(s.Key.Category)).First().Rate.Value * (decimal)(s.Sum(b => b.Duration.Value.TotalMinutes) / 60), 2),
                                divisionCode = s.Key.Division
                            }).ToList();
            */
                //PeriodId TargetId TargetDate Status

                return periodList;
        }

        public bool IsMainGroup(string groupName, int minorId)
        {
            bool result = false;

            ProductMainGroup mainGroup = new ProductMainGroup();
            ProductSubGroup subGroup = new ProductSubGroup();
            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(minorId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(minorId)).First();

                if (_context.GetMPSContext().ProductSubGroup.Any(w => w.Id.Equals(minorGroup.SubGroupId)))
                {
                    subGroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();

                    if (_context.GetMPSContext().ProductMainGroup.Any(w => w.Id.Equals(subGroup.MainGroupId)))
                    {
                        mainGroup = _context.GetMPSContext().ProductMainGroup.Where(w => w.Id.Equals(subGroup.MainGroupId)).First();

                        result = mainGroup.Type.Equals(groupName);
                        
                        return result;
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return result;
            }
        }

        public bool IsSubGroup(string groupName, int minorId)
        {
            bool result = false;

            ProductMainGroup mainGroup = new ProductMainGroup();
            ProductSubGroup subGroup = new ProductSubGroup();
            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(minorId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(minorId)).First();

                if (_context.GetMPSContext().ProductSubGroup.Any(w => w.Id.Equals(minorGroup.SubGroupId)))
                {
                    subGroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();

                    return subGroup.Type.Equals(groupName);
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return result;
            }
        }

        public bool IsMinorGroup(string groupName, int minorId)
        {
            bool result = false;

            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(minorId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(minorId)).First();

                return minorGroup.Type.Equals(groupName);
            }
            else
            {
                return result;
            }
        }

        public string IsClosedPeriodById(int periodId)
        {
            var target = _context.GetMPSContext().Period.Where(w => w.Id.Equals(periodId)).First();

            if (target.Status.Equals("Closed"))
            {
                return "Closed";
            }
            else
            {
                return "Open";
            }
        }

        public string GetStockGroupMainName(string productCode)
        {
            var stock = _context.GetMPSContext().Product.Where(w=>w.Code.Equals(productCode)).First();

            ProductMainGroup mainGroup = new ProductMainGroup();
            ProductSubGroup subGroup = new ProductSubGroup();
            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(stock.MinorGroupId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(stock.MinorGroupId)).First();

                if (_context.GetMPSContext().ProductSubGroup.Any(w => w.Id.Equals(minorGroup.SubGroupId)))
                {
                    subGroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();

                    if (_context.GetMPSContext().ProductMainGroup.Any(w => w.Id.Equals(subGroup.MainGroupId)))
                    {
                        mainGroup = _context.GetMPSContext().ProductMainGroup.Where(w => w.Id.Equals(subGroup.MainGroupId)).First();
                        
                        return mainGroup.Type;
                    }
                    else
                    {
                        return subGroup.Type;
                    }
                }
                else
                {
                    return minorGroup.Type;
                }
            }
            else
            {
                return "None";
            }
        }

        public string GetStockGroupSubName(int stockId)
        {
            var stock = _context.GetMPSContext().Product.Where(w => w.Id.Equals(stockId)).First();

            ProductMainGroup mainGroup = new ProductMainGroup();
            ProductSubGroup subGroup = new ProductSubGroup();
            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(stock.MinorGroupId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(stock.MinorGroupId)).First();

                if (_context.GetMPSContext().ProductSubGroup.Any(w => w.Id.Equals(minorGroup.SubGroupId)))
                {
                    subGroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();

                    return subGroup.Type;
                }
                else
                {
                    return minorGroup.Type;
                }
            }
            else
            {
                return "None";
            }
        }

        public string GetStockGroupMinorName(int stockId)
        {
            var stock = _context.GetMPSContext().Product.Where(w => w.Id.Equals(stockId)).First();

            ProductMainGroup mainGroup = new ProductMainGroup();
            ProductSubGroup subGroup = new ProductSubGroup();
            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(stock.MinorGroupId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(stock.MinorGroupId)).First();

                return minorGroup.Type;
            }
            else
            {
                return "None";
            }
        }

        public string GetStockGroupMainNameByMinorId(int minorId)
        {
            ProductMainGroup mainGroup = new ProductMainGroup();
            ProductSubGroup subGroup = new ProductSubGroup();
            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(minorId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(minorId)).First();

                if (_context.GetMPSContext().ProductSubGroup.Any(w => w.Id.Equals(minorGroup.SubGroupId)))
                {
                    subGroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();

                    if (_context.GetMPSContext().ProductMainGroup.Any(w => w.Id.Equals(subGroup.MainGroupId)))
                    {
                        mainGroup = _context.GetMPSContext().ProductMainGroup.Where(w => w.Id.Equals(subGroup.MainGroupId)).First();

                        return mainGroup.Type;
                    }
                    else
                    {
                        return subGroup.Type;
                    }
                }
                else
                {
                    return minorGroup.Type;
                }
            }
            else
            {
                return "None";
            }
        }

        public string GetStockGroupSubNameByMinorId(int minorId)
        {
            ProductMainGroup mainGroup = new ProductMainGroup();
            ProductSubGroup subGroup = new ProductSubGroup();
            ProductMinorGroup minorGroup = new ProductMinorGroup();

            if (_context.GetMPSContext().ProductMinorGroup.Any(w => w.Id.Equals(minorId)))
            {
                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(minorId)).First();

                if (_context.GetMPSContext().ProductSubGroup.Any(w => w.Id.Equals(minorGroup.SubGroupId)))
                {
                    subGroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();

                    return subGroup.Type;
                }
                else
                {
                    return minorGroup.Type;
                }
            }
            else
            {
                return "None";
            }
        }

        public List<StockCountView> GetStockCountList(string periodId)
        {
            Period period = new Period();
            period = _context.GetMPSContext().Period.Where(w => w.Id.Equals(int.Parse(periodId))).First();

            //_logger.WriteTestLog("Period ID : " + periodId);

            List<StockCount> stockCountList = new List<StockCount>();

            if (_context.GetMPSContext().StockCount.Any(w => w.PeriodId.Equals(period.Id)))
            {
                stockCountList = _context.GetMPSContext().StockCount.Where(w => w.PeriodId.Equals(period.Id)).ToList();
            }

            List<StockCountView> stockCountViewList = new List<StockCountView>();

            foreach (var stockCout in stockCountList)
            {
                if(_context.GetMPSContext().Product.Any(x => x.Code.Equals(stockCout.ProductCode)))
                {
                    if (!_context.GetMPSContext().Product.Where(x => x.Code.Equals(stockCout.ProductCode)).First().Inactive.Value)
                    {
                        StockCountView stockCountView = new StockCountView();

                        stockCountView.StockCountId = stockCout.Id;

                        if (period.Status.Equals("Closed"))
                        {
                            stockCountView.IsClosed = true;
                        }
                        else
                        {
                            stockCountView.IsClosed = false;
                        }

                        stockCountView.StockDate = period.EndDate.Value.Date;
                        stockCountView.StockDateString = period.EndDate.Value.ToString("dd/MM/yy");
                        stockCountView.ProductCode = stockCout.ProductCode;
                        stockCountView.ProductDesc = _context.GetMPSContext().Product.Where(w => w.Code.Equals(stockCountView.ProductCode)).First().Desc;
                        stockCountView.MainGroup = GetStockGroupMainName(stockCountView.ProductCode);

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
                }
            }

            return stockCountViewList;
        }

        public List<ExpenseDetailView> GetExpenseList(string periodId)
        {
            Period period = new Period();

            if (periodId.Equals("New"))
            {
                var latestPeriodId = GetLatePeriodId("ExpenseNew");
                period = _context.GetMPSContext().Period.Where(w => w.Id.Equals(latestPeriodId)).First();
            }
            else
            {
                period = _context.GetMPSContext().Period.Where(w => w.Id.Equals(int.Parse(periodId))).First();
            }

            List<Expense> expensList = new List<Expense>();
            if (_context.GetMPSContext().Expense.Any(w => w.PeriodId.Equals(period.Id)))
            {
                expensList = _context.GetMPSContext().Expense.Where(w => w.PeriodId.Equals(period.Id)).ToList();
            }

            List<ExpenseDetailView> expensViewList = new List<ExpenseDetailView>();
            if (expensList.Count() > 0)
            {
                foreach (var expense in expensList)
                {
                    ExpenseDetailView expenseView = new ExpenseDetailView();

                    expenseView.ExpenseId = expense.Id;
                    expenseView.PeriodId = expense.PeriodId.Value;

                    if (period.Status.Equals("Closed"))
                    {
                        expenseView.IsClosed = true;
                    }
                    else
                    {
                        expenseView.IsClosed = false;
                    }

                    expenseView.ExpenseCode = expense.ExpenseCode;
                    expenseView.ExpenseDesc = _context.GetMPSContext().Product.Where(w => w.Code.Equals(expenseView.ExpenseCode)).First().Desc;

                    if (expense.Price.HasValue)
                    {
                        expenseView.Price = expense.Price.Value;
                    }
                    else
                    {
                        expenseView.Price = 0;
                    }

                    expensViewList.Add(expenseView);
                }
            }
            else
            {
                List<CustomisedProductItemModel> itemList = new List<CustomisedProductItemModel>();
                itemList = GetCustomisedProductList("Active", "Expenses");

                foreach(var item in itemList)
                {
                    DateTime timeSync = DateTime.Now;

                    Expense newExpense = new Expense();

                    newExpense.ExpenseCode = item.Code;
                    newExpense.PeriodId = period.Id;
                    newExpense.Price = 0;
                    newExpense.UpdatedBy = "System";
                    newExpense.UpdatedOn = timeSync;

                    _context.GetMPSContext().Expense.Add(newExpense);
                    if(_context.GetMPSContext().SaveChanges() > 0)
                    {
                        newExpense = _context.GetMPSContext().Expense.Where(w => w.UpdatedOn.Value.Equals(timeSync)).First();
                    }

                    ExpenseDetailView expenseView = new ExpenseDetailView();

                    expenseView.ExpenseId = newExpense.Id;
                    expenseView.PeriodId = newExpense.PeriodId.Value;

                    if (period.Status.Equals("Closed"))
                    {
                        expenseView.IsClosed = true;
                    }
                    else
                    {
                        expenseView.IsClosed = false;
                    }

                    expenseView.ExpenseCode = newExpense.ExpenseCode;
                    expenseView.ExpenseDesc = _context.GetMPSContext().Product.Where(w => w.Code.Equals(expenseView.ExpenseCode)).First().Desc;

                    if (newExpense.Price.HasValue)
                    {
                        expenseView.Price = newExpense.Price.Value;
                    }
                    else
                    {
                        expenseView.Price = 0;
                    }

                    expensViewList.Add(expenseView);
                }

            }

            return expensViewList;
        }

        public List<WasteReason> GetWasteReason()
        {
            return _context.GetMPSContext().WasteReason.Where(w => w.SortId < 3).ToList();
        }

        public int GetProductCodeMaxLenght(List<Product> productList)
        {
            int maxLenght = 0;
            
            foreach(var product in productList)
            {
                if(product.Code.Length > maxLenght)
                {
                    maxLenght = product.Code.Length;
                }
            }

            return maxLenght;
        }

        public List<ClaimRefItem> GetClaimRefList(string type)
        {
            DateTime dateFrom = DateTime.Now.AddYears(-1);
            DateTime dateTo = DateTime.Now;

            List<ClaimRefItem> claimRefList = new List<ClaimRefItem>();

            if(type.Equals("Sales"))
            {
                var claimSaleList = new List<Sale>();

                claimSaleList = _context.GetMPSContext().Sale.Where(w => (w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0) && (!w.Status.Equals("New")) && ((!w.Type.Equals("PriceClaim")) || (!w.Type.Equals("QualityClaim")) || (!w.Type.Equals("QuantityClaim")))).OrderByDescending(o => o.InvoiceId).ToList();

                foreach (var claim in claimSaleList)
                {
                    ClaimRefItem item = new ClaimRefItem();

                    item.InvoiceId = claim.InvoiceId;
                    item.CompanyName = claim.Company;
                    item.DisplayDate = claim.DeliveryDate.Value.ToString("yyyy/MM/dd");

                    item.DisplayOption = claim.InvoiceId + "&emsp;|&emsp;" + claim.Company + "&emsp;|&emsp;" + item.DisplayDate;

                    claimRefList.Add(item);
                }
            }
            else if(type.Equals("Purchases"))
            {
                var claimPurchaseList = new List<Purchase>();

                claimPurchaseList = _context.GetMPSContext().Purchase.Where(w => (w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0) && (!w.Status.Equals("New")) && w.Type.Equals("Invoice")).OrderByDescending(o => o.InvoiceId).ToList();

                foreach(var claim in claimPurchaseList)
                {
                    ClaimRefItem item = new ClaimRefItem();

                    item.InvoiceId = claim.InvoiceId;
                    item.CompanyName = claim.Company;
                    item.DisplayDate = claim.DeliveryDate.Value.ToString("yyyy/MM/dd");

                    item.DisplayOption = claim.InvoiceId + "&emsp;|&emsp;" + claim.Company + "&emsp;|&emsp;" + item.DisplayDate;

                    claimRefList.Add(item);
                }
            }

            return claimRefList;
        }

        public ClaimRefItem GetClaimDetails(string type, int claimId)
        {
            ClaimRefItem item = new ClaimRefItem();

            if(type.Equals("Sales"))
            {
                var sale = _context.GetMPSContext().Sale.Where(w=>w.InvoiceId.Equals(claimId)).First();

                if(!string.IsNullOrEmpty(sale.CompanyNo))
                {
                    item.CustPo = sale.CompanyNo;
                }
                else
                {
                    item.CustPo = " ";
                }

                if (sale.Revision.HasValue)
                {
                    item.Revision = (int)sale.Revision.Value;
                }
                else
                {
                    item.Revision = 0;
                }

                if (sale.ShippingAddress.HasValue)
                {
                    var address = _context.GetMPSContext().Address.Where(w=>w.Id.Equals(sale.ShippingAddress.Value)).First();
                    
                    item.AddressId = address.Id;
                    item.Address = address.Street + " " + address.City + " " + address.Street + " " + address.Postcode;
                }
                else
                {
                    item.AddressId = 0;
                    item.Address = " ";
                }

                item.CompanyName = sale.Company;
                item.CustPk = _context.GetMPSContext().Company.Where(w => w.CompanyName.Equals(item.CompanyName)).First().Pk;
            }
            else if (type.Equals("Purchases"))
            {
                var purchase = _context.GetMPSContext().Purchase.Where(w => w.InvoiceId.Equals(claimId)).First();

                if (!string.IsNullOrEmpty(purchase.CompanyNo))
                {
                    item.InvoiceBn = purchase.CompanyNo;
                }
                else
                {
                    item.InvoiceBn = " ";
                }

                if(purchase.Returned.HasValue)
                {
                    item.IsReturned = purchase.Returned.Value;
                }
                else
                {
                    item.IsReturned = false;
                }

                item.CompanyName = purchase.Company;
                item.CustPk = _context.GetMPSContext().Company.Where(w=>w.CompanyName.Equals(item.CompanyName)).First().Pk;

                var address = _context.GetMPSContext().Address.Where(w => w.Company.Equals(item.CompanyName) && w.Type.Equals("Billing")).First();

                item.AddressId = address.Id;
                item.Address = address.Street + " " + address.City + " " + address.Street + " " + address.Postcode;
                
            }

            return item;
        }

        public List<StockCount> GetStockCountActiveList(List<StockCount> baseCountList)
        {
            List<StockCount> returnList = new List<StockCount>();

            foreach(var stockCount in baseCountList)
            {
                if(_context.GetMPSContext().ProductSettings.Any(x=>x.ProductCode.Equals(stockCount.ProductCode.Trim()) && (!x.IsStockCountable)))
                {
                    //
                }
                else
                {
                    returnList.Add(stockCount);
                }
            }

            return returnList;
        }

        public bool IsStockCountable(string productCode)
        {
            var returnValue = true;
            
            if(_context.GetMPSContext().ProductSettings.Any(w => w.ProductCode.Equals(productCode)))
            {
                returnValue = _context.GetMPSContext().ProductSettings.Where(w => w.ProductCode.Equals(productCode)).First().IsStockCountable;
            }

            return returnValue;
        }

        /*

        rnrals wjdtjrk whgdmfflrk dlTsi


        */

        //End
    }
}
