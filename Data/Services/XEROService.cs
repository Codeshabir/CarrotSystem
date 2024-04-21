
using MYOB.AccountRight.SDK.Contracts;
using MYOB.AccountRight.SDK.Services;
using MYOB.AccountRight.SDK;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using MYOB.AccountRight.SDK.Extensions;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Models.MPS;
using CarrotSystem.Controllers;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Api;

namespace CarrotSystem.Services
{
    public class XEROService : ApiAccessorController<AccountingApi>
    {
        private IContextService _context;
        private readonly IAPIService _apiService;
        private readonly IEventWriter _logger;
        private readonly string myobLibraryPath = "";

        public XEROService(IOptions<XeroConfiguration> xeroConfig, IContextService context, IAPIService apiService, IEventWriter logger) : base(xeroConfig) 
        {
            _context = context;
            _apiService = apiService;
            _logger = logger;
        }

        public void ClearRecords(string type, string exportDate)
        {
            var targetDate = DateTime.ParseExact(exportDate, "ddMMyyyy", CultureInfo.InvariantCulture);
            var period = _apiService.GetLatePeriod(targetDate);

            if (type.Equals("All"))
            {
                ClearRecords("Sales", exportDate);
                ClearRecords("Purchases", exportDate);
            }
            else if (type.Equals("Sales"))
            {
                var salesList = _context.GetMPSContext().Sale.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();
                
                foreach(var sale in salesList)
                {
                    sale.Status = "Invoice";

                    _context.GetMPSContext().Sale.Update(sale);
                    _context.GetMPSContext().SaveChanges();
                }

                var logList = _context.GetMPSContext().Sale.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

                foreach(var log in logList)
                {
                    if(_context.GetMPSContext().Myoblog.Any(x=>x.InvoiceId.Equals(log.InvoiceId)))
                    {
                        var deleteList = _context.GetMPSContext().Myoblog.Where(x => x.InvoiceId.Equals(log.InvoiceId)).ToList();

                        //_logger.WriteTestLog("Deleted Sale Log Items : " + deleteList.Count());

                        _context.GetMPSContext().Myoblog.RemoveRange(deleteList);
                        _context.GetMPSContext().SaveChanges();
                    }
                }
            }
            else if (type.Equals("Purchases"))
            {
                var purchaseList = _context.GetMPSContext().Purchase.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

                foreach (var purchase in purchaseList)
                {
                    purchase.Status = "Invoice";

                    _context.GetMPSContext().Purchase.Update(purchase);
                    _context.GetMPSContext().SaveChanges();
                }

                var logList = _context.GetMPSContext().Purchase.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

                foreach (var log in logList)
                {
                    if (_context.GetMPSContext().Myoblog.Any(x => x.InvoiceId.Equals(log.InvoiceId)))
                    {
                        var deleteList = _context.GetMPSContext().Myoblog.Where(x => x.InvoiceId.Equals(log.InvoiceId)).ToList();

                        //_logger.WriteTestLog("Deleted Purchase Items : " + deleteList.Count());

                        _context.GetMPSContext().Myoblog.RemoveRange(deleteList);
                        _context.GetMPSContext().SaveChanges();
                    }
                }
            }
        }

        public int GetExportCount(string type, string exportDate)
        {
            var maxWidth = 1000;

            var targetDate = DateTime.ParseExact(exportDate, "ddMMyyyy", CultureInfo.InvariantCulture);
            var period = _apiService.GetLatePeriod(targetDate);

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

            return maxWidth;
        }

        public string ExportToXero(string type, string exportDate, string userName)
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            CompanyFile companyFile = new CompanyFile();

            if (companyFiles.Any(any => any.LibraryPath.Equals(myobLibraryPath)))
            {
                companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

                var itemURI = "";
                
                //_logger.WriteTestLog("Data Here : " + type + ", exportDate : " + exportDate);
                var exportedCount = 0;
                var errorCount = 0;

                if (type.Equals("Sales"))
                {
                    var exportType = type;

                    var targetDate = DateTime.ParseExact(exportDate, "ddMMyyyy", CultureInfo.InvariantCulture);
                    var period = _apiService.GetLatePeriod(targetDate);
                    var isClosed = _apiService.IsClosedPeriodByDate(targetDate);

                    itemURI = "/Sale/Invoice/Item";
                    var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                    List<ExportDetailView> expSaleList = new List<ExportDetailView>();

                    var salesList = _context.GetMPSContext().Sale.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();
                    //var salesList = _context.GetMPSContext().Sale.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

                    if (salesList.Count > 0)
                    {
                        foreach (var sale in salesList)
                        {
                            List<string> errorList = new List<string>();
                            List<SaleItem> saleItemList = new List<SaleItem>();
                            saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Value.Equals(sale.InvoiceId)).ToList();

                            var json = new StringBuilder();

                            json.Append("{");
                            json.Append("'Date':'" + sale.DeliveryDate.Value.ToString("yyyy-MM-dd") + "T" + sale.DeliveryDate.Value.ToString("HH:mm:ss") + ".000',");
                            json.Append("'Number': " + sale.InvoiceId + ",");
                            json.Append("'Customer':{'UID':'" + GetUIDfromDB("Customer", sale.Company) + "'},");
                            json.Append("'Lines': [");

                            foreach (var saleItem in saleItemList)
                            {
                                var qty = saleItem.InvoicedQty.Value;
                                var price = saleItem.Price.Value;
                                var total = qty * price;

                                if (saleItem.Tax.Equals("GST"))
                                {
                                    total = total * 1.1;
                                }

                                json.Append("{");
                                json.Append("'ShipQuantity': " + qty + ",");
                                json.Append("'UnitPrice': " + price + ",");
                                json.Append("'Total': " + Math.Round(total, 2) + ",");
                                json.Append("'Item': { 'UID':'" + GetUIDfromDB("Item", saleItem.ProductCode) + "'},");
                                json.Append("'TaxCode': { 'UID':'" + GetUIDfromDB("Taxcode", saleItem.Tax) + "'},");
                                json.Append("},");
                            }

                            json.Append("],");
                            json.Append("'Freight': 0,");
                            json.Append("'FreightTaxCode': { 'UID':'" + GetUIDfromDB("Taxcode", "GST") + "'}");
                            json.Append("}");

                            errorList.Add(PostData(requestAdd, json.ToString()));

                            foreach (var errorMsg in errorList)
                            {
                                var errDesc = "";

                                if(errorMsg.Split("_").Length > 1)
                                {
                                    errDesc = errorMsg.Split("_")[1];
                                }

                                if (errorMsg.Contains("Created"))
                                {
                                    exportedCount++;
                                    Myoblog newMyobLog = new Myoblog();

                                    newMyobLog.Target = "TCC";
                                    newMyobLog.PeriodId = period.Id;
                                    newMyobLog.InvoiceId = sale.InvoiceId;
                                    newMyobLog.Type = "Sale";
                                    newMyobLog.Result = "Success";
                                    newMyobLog.ErrorNumber = "";
                                    newMyobLog.ErrorDescription = errDesc;
                                    newMyobLog.ExportedOn = DateTime.Now;
                                    newMyobLog.ExportedBy = userName;

                                    _context.GetMPSContext().Myoblog.Add(newMyobLog);
                                    _context.GetMPSContext().SaveChanges();

                                    sale.Status = "Exported";
                                    sale.UpdatedBy = userName;
                                    sale.UpdatedOn = DateTime.Now;

                                    _context.GetMPSContext().Sale.Update(sale);
                                    if (_context.GetMPSContext().SaveChanges() > 0)
                                    {
                                        _logger.WriteEvents(userName, "MYOB", "Sales Exported", "Sales ID#" + sale.InvoiceId + " Exported [" + newMyobLog.Result + "]");
                                    }
                                }
                                else if (errorMsg.Contains("Failed"))
                                {
                                    errorCount++;
                                    Myoblog newMyobLog = new Myoblog();

                                    newMyobLog.Target = "TCC";
                                    newMyobLog.PeriodId = period.Id;
                                    newMyobLog.InvoiceId = sale.InvoiceId;
                                    newMyobLog.Type = "Sale";
                                    newMyobLog.Result = "Failed";
                                    newMyobLog.ErrorNumber = "";
                                    newMyobLog.ErrorDescription = errDesc;
                                    newMyobLog.ExportedOn = DateTime.Now;
                                    newMyobLog.ExportedBy = userName;

                                    _context.GetMPSContext().Myoblog.Add(newMyobLog);
                                    _context.GetMPSContext().SaveChanges();

                                    sale.Status = "Exported";
                                    sale.UpdatedBy = userName;
                                    sale.UpdatedOn = DateTime.Now;

                                    _context.GetMPSContext().Sale.Update(sale);
                                    if (_context.GetMPSContext().SaveChanges() > 0)
                                    {
                                        _logger.WriteEvents(userName, "MYOB", "Sales Exported", "Sales ID#" + sale.InvoiceId + " Exported [" + newMyobLog.Result + "]");
                                    }
                                }
                            }
                        }
                    }
                }
                else if (type.Equals("Purchases"))
                {
                    var exportType = type;

                    var targetDate = DateTime.ParseExact(exportDate, "ddMMyyyy", CultureInfo.InvariantCulture);
                    var period = _apiService.GetLatePeriod(targetDate);
                    var isClosed = _apiService.IsClosedPeriodByDate(targetDate);

                    itemURI = "/Purchase/Bill/Item";
                    var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                    List<ExportDetailView> expSaleList = new List<ExportDetailView>();

                    var purchaseList = _context.GetMPSContext().Purchase.Where(w => (!w.Status.Equals("Exported")) && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();
                    //var purchaseList = _context.GetMPSContext().Purchase.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

                    if (purchaseList.Count > 0)
                    {
                        foreach (var purchase in purchaseList)
                        {
                            List<string> errorList = new List<string>();
                            List<PurchaseItem> purchaseItemList = new List<PurchaseItem>();
                            purchaseItemList = _context.GetMPSContext().PurchaseItem.Where(w => w.InvoiceId.Value.Equals(purchase.InvoiceId)).ToList();

                            var json = new StringBuilder();

                            json.Append("{");
                            json.Append("'Date':'" + purchase.DeliveryDate.Value.ToString("yyyy-MM-dd") + "T" + purchase.DeliveryDate.Value.ToString("HH:mm:ss") + ".000',");
                            json.Append("'Number': " + purchase.InvoiceId + ",");
                            json.Append("'Supplier':{'UID':'" + GetUIDfromDB("Supplier", purchase.Company) + "'},");
                            json.Append("'Lines': [");

                            foreach (var purchaseItem in purchaseItemList)
                            {
                                var qty = purchaseItem.InvoicedQty.Value;
                                var price = purchaseItem.Price.Value;
                                var total = qty * price;

                                if(purchaseItem.Tax.Equals("GST"))
                                {
                                    total = total * 1.1;
                                }

                                _logger.WriteTestLog("TaxTotal : " + total);

                                json.Append("{");
                                json.Append("'BillQuantity': " + qty + ",");
                                json.Append("'UnitPrice': " + price + ",");
                                json.Append("'Total': " + Math.Round(total, 2) + ",");
                                json.Append("'Item': { 'UID':'" + GetUIDfromDB("Item", purchaseItem.ProductCode) + "'},");
                                json.Append("'TaxCode': { 'UID':'" + GetUIDfromDB("Taxcode", purchaseItem.Tax) + "'},");
                                json.Append("},");
                            }

                            json.Append("],");
                            json.Append("'Freight': 0,");
                            json.Append("'FreightTaxCode': { 'UID':'" + GetUIDfromDB("Taxcode", "GST") + "'}");
                            json.Append("}");

                            errorList.Add(PostData(requestAdd, json.ToString()));

                            foreach (var errorMsg in errorList)
                            {
                                var errDesc = "";

                                if (errorMsg.Split("_").Length > 1)
                                {
                                    errDesc = errorMsg.Split("_")[1];
                                }

                                if (errorMsg.Contains("Created"))
                                {
                                    exportedCount++;
                                    Myoblog newMyobLog = new Myoblog();

                                    newMyobLog.Target = "TCC";
                                    newMyobLog.PeriodId = period.Id;
                                    newMyobLog.InvoiceId = purchase.InvoiceId;
                                    newMyobLog.Type = "Purchase";
                                    newMyobLog.Result = "Success";
                                    newMyobLog.ErrorNumber = "";
                                    newMyobLog.ErrorDescription = errDesc;
                                    newMyobLog.ExportedOn = DateTime.Now;
                                    newMyobLog.ExportedBy = userName;

                                    _context.GetMPSContext().Myoblog.Add(newMyobLog);
                                    _context.GetMPSContext().SaveChanges();

                                    purchase.Status = "Exported";
                                    purchase.UpdatedBy = userName;
                                    purchase.UpdatedOn = DateTime.Now;

                                    _context.GetMPSContext().Purchase.Update(purchase);
                                    if (_context.GetMPSContext().SaveChanges() > 0)
                                    {
                                        _logger.WriteEvents(userName, "MYOB", "Purchase Exported", "Purchase ID#" + purchase.InvoiceId + " Exported [" + newMyobLog.Result + "]");
                                    }
                                }
                                else if (errorMsg.Contains("Failed"))
                                {
                                    errorCount++;
                                    Myoblog newMyobLog = new Myoblog();

                                    newMyobLog.Target = "TCC";
                                    newMyobLog.PeriodId = period.Id;
                                    newMyobLog.InvoiceId = purchase.InvoiceId;
                                    newMyobLog.Type = "Purchase";
                                    newMyobLog.Result = "Failed";
                                    newMyobLog.ErrorNumber = "";
                                    newMyobLog.ErrorDescription = errDesc;
                                    newMyobLog.ExportedOn = DateTime.Now;
                                    newMyobLog.ExportedBy = userName;

                                    _context.GetMPSContext().Myoblog.Add(newMyobLog);
                                    _context.GetMPSContext().SaveChanges();

                                    purchase.Status = "Exported";
                                    purchase.UpdatedBy = userName;
                                    purchase.UpdatedOn = DateTime.Now;

                                    _context.GetMPSContext().Purchase.Update(purchase);
                                    _context.GetMPSContext().SaveChanges();
                                }
                            }

                        }
                    }
                }
                else if (type.Equals("Journal"))
                {
                    _logger.WriteTestLog("Journal Export Start");

                    itemURI = "/GeneralLedger/GeneralJournal";
                    var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                    var exportedDate = DateTime.ParseExact(exportDate, "yyyyMMddhhmmss", CultureInfo.InvariantCulture);
                    var exportJournalList = _context.GetMPSContext().ExportTcclines.Where(w=>w.ExportedOn.Equals(exportedDate)).ToList();

                    if(exportJournalList.Count > 0)
                    {
                        var expJournalGroup = exportJournalList.GroupBy(g => new { JournalNo = g.JournalNo, DateOccurred = g.DateOccurred, Memo = g.Memo, Inclusive = g.Inclusive})
                        .Select(s => new {
                            JournalNo = s.Key.JournalNo,
                            DateOccurred = s.Key.DateOccurred,
                            Memo = s.Key.Memo,
                            Inclusive = s.Key.Inclusive
                        }).ToList();

                        if(expJournalGroup.Count > 0)
                        {
                            foreach(var expGroup in expJournalGroup)
                            {
                                _logger.WriteTestLog("Journal Export Group Occurred On " + expGroup.DateOccurred.ToString("yyyy-MM-dd"));

                                var json = new StringBuilder();

                                json.Append("{");
                                json.Append("'DateOccurred':'" + expGroup.DateOccurred.ToString("yyyy-MM-dd") + "T" + expGroup.DateOccurred.ToString("HH:mm:ss") + ".000',");
                                json.Append("'DisplayID':'" + expGroup.JournalNo + "',");
                                json.Append("'Memo':'" + expGroup.Memo + "',");
                                json.Append("'GSTReportingMethod':'Sale',");
                                json.Append("'IsTaxInclusive':'" + expGroup.Inclusive + "',");
                                json.Append("'Lines': [");

                                //Journal Lines
                                var expJnrList = exportJournalList.Where(w => w.JournalNo.Equals(expGroup.JournalNo) && w.DateOccurred.Equals(expGroup.DateOccurred) && w.Inclusive.Equals(expGroup.Inclusive) && w.Memo.Equals(expGroup.Memo)).ToList();

                                foreach(var jnr in expJnrList)
                                {
                                    decimal amount = 0;
                                    decimal taxAmount = 0;

                                    string isCredit = "false";

                                    if (jnr.DebitExTax > 0 || jnr.DebitInTax > 0)
                                    {
                                        taxAmount = jnr.DebitInTax - jnr.DebitExTax;
                                        amount = jnr.DebitExTax + taxAmount;

                                        isCredit = "false";
                                    }
                                    else if (jnr.CreditExTax > 0 || jnr.CreditInTax > 0)
                                    {
                                        taxAmount = jnr.CreditInTax - jnr.CreditExTax;
                                        amount = jnr.CreditExTax + taxAmount;

                                        isCredit = "true";
                                    }

                                    json.Append("{");
                                    json.Append("'Account':{'UID':'" + GetUIDfromDB("Account", jnr.AccountNo) + "'},");
                                    json.Append("'TaxCode':{'UID':'" + GetUIDfromDB("Taxcode", jnr.TaxCode) + "'},");
                                    json.Append("'Amount': " + amount + ",");
                                    json.Append("'TaxAmount': " + taxAmount + ",");
                                    json.Append("'IsCredit': '" + isCredit + "',");
                                    json.Append("'Job':{'UID':'" + GetUIDfromDB("Job", jnr.Job) + "'}");
                                    json.Append("},");
                                }

                                json.Append("]");
                                json.Append("}");

                                PostData(requestAdd, json.ToString());
                            }
                        }
                    }
                }

                return "Success_" + exportedCount + "_" + errorCount;
            }
            else
            {
                _logger.WriteTestLog("Can not find MYOB company file.");

                return "Can not find MYOB company file";
            }
        }

        public void ExportToDBbyLines(string type, string exportData, string exportDate, string userBy)
        {
            var exportStringArray = exportData.Split(',');
            
            if(type.Equals("Journal"))
            {
                if(!string.IsNullOrEmpty(exportStringArray[0]))
                {
                    ExportTccline exportLines = new ExportTccline();

                    exportLines.JournalNo = exportStringArray[0];
                    exportLines.DateOccurred = DateTime.ParseExact(exportStringArray[1], "dd/MM/yy", CultureInfo.InvariantCulture);
                    exportLines.Memo = exportStringArray[2];

                    if (exportStringArray[3].Equals("s"))
                    {
                        exportLines.Gstreporting = "Sale";
                    }
                    else if (exportStringArray[3].Equals("p"))
                    {
                        exportLines.Gstreporting = "Purchase";
                    }

                    var accNo = exportStringArray[5].ToArray();

                    exportLines.Inclusive = IsTaxInclusive(exportStringArray[4]).ToString();
                    exportLines.AccountNo = accNo[0] + "-" + accNo[1] + accNo[2] + accNo[3] + accNo[4];

                    exportLines.DebitExTax = decimal.Parse(exportStringArray[6]);
                    exportLines.DebitInTax = decimal.Parse(exportStringArray[7]);
                    exportLines.CreditExTax = decimal.Parse(exportStringArray[8]);
                    exportLines.CreditInTax = decimal.Parse(exportStringArray[9]);

                    exportLines.Job = exportStringArray[10];
                    exportLines.TaxCode = exportStringArray[11];

                    exportLines.ExportedOn = DateTime.ParseExact(exportDate, "yyyyMMddhhmmss", CultureInfo.InvariantCulture);
                    exportLines.ExportedBy = userBy;

                    _context.GetMPSContext().ExportTcclines.Add(exportLines);
                    _context.GetMPSContext().SaveChanges();
                }
            }
            else if (type.Equals("SaleINV"))
            {
                if(!string.IsNullOrEmpty(exportStringArray[0]))
                {
                    var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

                    var cfService = new CompanyFileService(configuration);
                    var credentials = new CompanyFileCredentials("Administrator", "yumgo");

                    List<CompanyFile> companyFiles = cfService.GetRange().ToList();

                    CompanyFile companyFile = new CompanyFile();

                    if (companyFiles.Any(any => any.LibraryPath.Equals(myobLibraryPath)))
                    {
                        companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

                        var itemURI = "/Purchase/Bill/Item";
                        var exportedDate = DateTime.ParseExact(exportDate, "yyyyMMddhhmmss", CultureInfo.InvariantCulture);

                        var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                        var json = new StringBuilder();

                        var compName = exportStringArray[0].Replace("_", "&");
                        var itemNo = exportStringArray[12];
                        var dateOccurred = DateTime.ParseExact(exportStringArray[8], "dd/MM/yy", CultureInfo.InvariantCulture);

                        var taxCode = exportStringArray[27];

                        var qty = decimal.Parse(exportStringArray[13]);
                        var price = decimal.Parse(exportStringArray[15]);
                        
                        var totalIncTax = decimal.Parse(exportStringArray[19]);
                        var total = decimal.Parse(exportStringArray[18]);
                        var totalGST = decimal.Parse(exportStringArray[29]);
                        
                        json.Append("{");
                        json.Append("'Date':'" + dateOccurred.ToString("yyyy-MM-dd") + "T" + dateOccurred.ToString("HH:mm:ss") + ".000',");
                        json.Append("'Supplier':{'UID':'" + GetUIDfromDB("Supplier", compName) + "'},");
                        json.Append("'Lines': [");
                        json.Append("{");
                        json.Append("'BillQuantity': " + qty + ",");
                        json.Append("'UnitPrice': " + price + ",");
                        json.Append("'Total': " + total + ",");
                        json.Append("'Item': { 'UID':'" + GetUIDfromDB("Item", itemNo) + "'},");
                        json.Append("'TaxCode': { 'UID':'" + GetUIDfromDB("Taxcode", taxCode) + "'},");
                        json.Append("}");
                        json.Append("],");
                        json.Append("'Freight': 0,");
                        json.Append("'FreightTaxCode': { 'UID':'" + GetUIDfromDB("Taxcode", "GST") + "'}");
                        json.Append("}");

                        PostData(requestAdd, json.ToString());
                    }

                }
            }

        }

        public string PostData(string requestAdd, string dataJson)
        {
            string isCreated = "Failed";
            _logger.WriteTestLog(requestAdd);
            _logger.WriteTestLog("JSON : " + dataJson);

            byte[] arr = System.Text.Encoding.UTF8.GetBytes(dataJson);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "POST";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.ContentLength = arr.Length;
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(arr, 0, arr.Length);
            dataStream.Close();

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        var responseText = reader.ReadToEnd();
                        _logger.WriteTestLog("Response text " + responseText);
                        _logger.WriteTestLog("Sup Status Code : " + response.StatusCode.ToString());
                        isCreated = response.StatusCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorData = ex.Data.ToJson();

                var resultMsg = "Error info:" + ex.Message + ", " + errorData.ToString() + ", ";

                _logger.WriteTestLog(resultMsg);
                _logger.WriteTestLog("Error Stack Trace:" + ex.StackTrace);

                isCreated = isCreated + "_" + ex.Message;
            }

            return isCreated;
        }

        public void ViewSupplierList()
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/Contact/Supplier?$filter=IsActive eq true";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

            _logger.WriteTestLog(requestAdd);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                Stream responseStream = response.GetResponseStream();
                using (var reader = new StreamReader(responseStream))
                {
                    // get the response as text
                    string responseText = reader.ReadToEnd();

                    // convert from text
                    _logger.WriteTestLog(responseText);
                    var results = Newtonsoft.Json.JsonConvert.DeserializeObject<DataSet>(responseText);

                    // do something with it

                    _logger.WriteTestLog("Content : " + results.Tables.Count);
                    _logger.WriteTestLog("Length : " + response.ContentLength);
                    _logger.WriteTestLog("Encoding : " + response.ContentEncoding);
                    _logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                }
            }
        }

        public void ViewCustomerList()
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/Contact/Customer?$top=2&filter=IsActive eq true";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

            _logger.WriteTestLog(requestAdd);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                Stream responseStream = response.GetResponseStream();
                using (var reader = new StreamReader(responseStream))
                {
                    // get the response as text
                    string responseText = reader.ReadToEnd();

                    // convert from text
                    _logger.WriteTestLog(responseText);
                    var results = Newtonsoft.Json.JsonConvert.DeserializeObject<DataSet>(responseText);

                    // do something with it

                    _logger.WriteTestLog("Content : " + results.Tables.Count);
                    _logger.WriteTestLog("Length : " + response.ContentLength);
                    _logger.WriteTestLog("Encoding : " + response.ContentEncoding);
                    _logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                }
            }
        }

        public string GetUIDfromDB (string dataType, string name)
        {
            if (_context.GetMPSContext().Myobsync.Any(x => x.Type.Equals(dataType) && x.Name.Equals(name)))
            {
                return _context.GetMPSContext().Myobsync.Where(x => x.Type.Equals(dataType) && x.Name.Equals(name)).First().Uid;
            }
            else
            {
                return _context.GetMPSContext().Myobsync.Where(x => x.Type.Equals(dataType)).First().Uid;
            }
        }

        public string GetItemUID(string itemNo)
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/Inventory/Item";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;
            var returnUID = "7fa2d90f-c324-4bd2-ac9b-c3d2e04a8db4";

            _logger.WriteTestLog("GetItemUID, Request Address : " + requestAdd);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        string responseText = reader.ReadToEnd();
                        //_logger.WriteTestLog(responseText);

                        // convert from text
                        var results = JsonConvert.DeserializeObject<JsonConvertItem>(responseText);

                        if (results.Items.Length > 0)
                        {
                            foreach (var item in results.Items)
                            {
                                if (item.Number.Equals(itemNo))
                                {
                                    returnUID = item.UID.ToString();
                                }
                            }
                        }

                        //_logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
            }

            return returnUID;
        }
        
        public string GetCustomerUID(string customerName)
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/Contact/Customer";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;
            var returnUID = "58efeb38-0d4d-455a-bfd8-51aac3d231a9";

            _logger.WriteTestLog("GetCustomerUID, Request Address : " + requestAdd + ", " + customerName);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        string responseText = reader.ReadToEnd();
                        //_logger.WriteTestLog(responseText);

                        // convert from text
                        var results = JsonConvert.DeserializeObject<JsonConvertCustomer>(responseText);

                        if (results.Items.Length > 0)
                        {
                            foreach (var item in results.Items)
                            {
                                if (item.CompanyName.Equals(customerName))
                                {
                                    returnUID = item.UID.ToString();
                                }
                            }
                        }
                        //_logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
            }

            return returnUID;
        }
        
        public string GetSupplierUID(string companyName)
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/Contact/Supplier/";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;
            var returnUID = "0c387348-ee87-4fab-bdd4-8eba7921c875";

            _logger.WriteTestLog("GetSupplierUID, Request Address : " + requestAdd + ", " + companyName);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        string responseText = reader.ReadToEnd();
                        //_logger.WriteTestLog(responseText);

                        // convert from text
                        var results = JsonConvert.DeserializeObject<JsonConvertSupplier>(responseText);

                        if (results.Items.Length > 0)
                        {
                            foreach (var item in results.Items)
                            {
                                if (item.CompanyName.Equals(companyName))
                                {
                                    returnUID = item.UID.ToString();
                                }
                            }
                        }

                        //_logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
            }

            return returnUID;
        }
        
        public string GetAccountUID(string accountName)
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/GeneralLedger/Account/";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;
            var returnUID = "0c387348-ee87-4fab-bdd4-8eba7921c875";

            var accountNoArray = accountName.ToArray();

            accountName = accountNoArray[0] + "-" + accountNoArray[1] + accountNoArray[2] + accountNoArray[3] + accountNoArray[4];

            //_logger.WriteTestLog("Request Address : " + requestAdd + ", " + accountName);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            try
            { 
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        string responseText = reader.ReadToEnd();
                        //_logger.WriteTestLog(responseText);

                        // convert from text
                        var results = JsonConvert.DeserializeObject<JsonConvertAccount>(responseText);

                        if (results.Items.Length > 0)
                        {
                            foreach (var item in results.Items)
                            {
                                if (item.DisplayID.Equals(accountName))
                                {
                                    returnUID = item.UID.ToString();
                                }
                            }
                        }
                        //_logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
            }

            return returnUID;
        }
        
        public string GetJobUID(string jobName)
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/GeneralLedger/Job/";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;
            var returnUID = "0c387348-ee87-4fab-bdd4-8eba7921c875";

            //_logger.WriteTestLog("Request Address : " + requestAdd + ", " + jobName);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            try 
            { 
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        string responseText = reader.ReadToEnd();
                        //_logger.WriteTestLog(responseText);

                        // convert from text
                        var results = JsonConvert.DeserializeObject<JsonConvertJob>(responseText);

                        if (results.Items.Length > 0)
                        {
                            foreach (var item in results.Items)
                            {
                                if (item.Number.Equals(jobName))
                                {
                                    returnUID = item.UID.ToString();
                                }
                            }
                        }
                        //_logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
            }

            return returnUID;
        }
        
        public string GetTaxCodeUID(string taxCode)
        {
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();

            var itemURI = "/GeneralLedger/TaxCode/";

            var requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;
            var returnUID = "0c387348-ee87-4fab-bdd4-8eba7921c875";

            //_logger.WriteTestLog("Request Address : " + requestAdd + ", " + taxCode);

            var compUid = companyFile.Id;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
            request.Method = "GET";
            request.Headers["x-myobapi-version"] = "v2";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential("administrator", "yumgo");

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        string responseText = reader.ReadToEnd();
                        //_logger.WriteTestLog(responseText);

                        // convert from text
                        var results = JsonConvert.DeserializeObject<JsonConvertTaxCode>(responseText);

                        if (results.Items.Length > 0)
                        {
                            foreach (var item in results.Items)
                            {
                                if (item.Code.Equals(taxCode))
                                {
                                    returnUID = item.UID.ToString();
                                }
                            }
                        }
                        //_logger.WriteTestLog("Status Code : " + response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
            }

            return returnUID;
        }

        public bool IsTaxInclusive (string isTaxInc)
        {
            if(isTaxInc.Equals("x"))
            {
                return false;
            }
            else if(isTaxInc.Equals("o"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SyncXEROtoDB(string dataType)
        {
            //Item Customer Supplier Account Job Taxcode
            var configuration = new ApiConfiguration("http://192.168.1.181:8080/accountright");

            var cfService = new CompanyFileService(configuration);
            var credentials = new CompanyFileCredentials("Administrator", "yumgo");

            List<CompanyFile> companyFiles = cfService.GetRange().ToList();

            var companyFile = companyFiles.Where(w => w.LibraryPath.Equals(myobLibraryPath)).First();
            var compUid = companyFile.Id;

            var itemURI = "";
            var requestAdd = "";

            int updateCount = 0;
            int addedCount = 0;
            
            if (dataType.Equals("All"))
            {
                SyncXEROtoDB("Item");
                SyncXEROtoDB("Customer");
                SyncXEROtoDB("Supplier");
                SyncXEROtoDB("Account");
                SyncXEROtoDB("Job");
                SyncXEROtoDB("Taxcode");
            }
            else if (dataType.Equals("Item"))
            {
                itemURI = "/Inventory/Item";
                requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
                request.Method = "GET";
                request.Headers["x-myobapi-version"] = "v2";
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential("administrator", "yumgo");

                try
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        Stream responseStream = response.GetResponseStream();
                        using (var reader = new StreamReader(responseStream))
                        {
                            // get the response as text
                            string responseText = reader.ReadToEnd();
                            //_logger.WriteTestLog(responseText);

                            // convert from text
                            var results = JsonConvert.DeserializeObject<JsonConvertItem>(responseText);

                            if (results.Items.Length > 0)
                            {
                                foreach (var item in results.Items)
                                {
                                    Myobsync newItem = new Myobsync();

                                    if (_context.GetMPSContext().Myobsync.Any(x=>x.Name.Equals(item.Number) && x.Type.Equals(dataType)))
                                    {
                                        newItem = _context.GetMPSContext().Myobsync.Where(x => x.Name.Equals(item.Number) && x.Type.Equals(dataType)).First();

                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Update(newItem);
                                        _context.GetMPSContext().SaveChanges();
                                        updateCount++;
                                    }
                                    else
                                    {
                                        newItem.Name = item.Number;
                                        
                                        newItem.Type = dataType;
                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Add(newItem);
                                        _context.GetMPSContext().SaveChanges();
                                        addedCount++;
                                    }
                                }

                                _logger.WriteTestLog("Sync : [" + dataType + "] " + updateCount + " Updated, " + addedCount + " Added.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
                }
            }
            else if (dataType.Equals("Customer"))
            {
                itemURI = "/Contact/Customer";
                requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
                request.Method = "GET";
                request.Headers["x-myobapi-version"] = "v2";
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential("administrator", "yumgo");

                try
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        Stream responseStream = response.GetResponseStream();
                        using (var reader = new StreamReader(responseStream))
                        {
                            // get the response as text
                            string responseText = reader.ReadToEnd();
                            //_logger.WriteTestLog(responseText);

                            // convert from text
                            var results = JsonConvert.DeserializeObject<JsonConvertCustomer>(responseText);

                            if (results.Items.Length > 0)
                            {
                                foreach (var item in results.Items)
                                {
                                    
                                    Myobsync newItem = new Myobsync();

                                    if (_context.GetMPSContext().Myobsync.Any(x => x.Name.Equals(item.CompanyName) && x.Type.Equals(dataType)))
                                    {
                                        newItem = _context.GetMPSContext().Myobsync.Where(x => x.Name.Equals(item.CompanyName) && x.Type.Equals(dataType)).First();

                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Update(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        updateCount++;
                                    }
                                    else
                                    {
                                        newItem.Name = item.CompanyName;

                                        newItem.Type = dataType;
                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Add(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        addedCount++;
                                    }
                                }
                                _logger.WriteTestLog("Sync : [" + dataType + "] " + updateCount + " Updated, " + addedCount + " Added.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
                }
            }
            else if (dataType.Equals("Supplier"))
            {
                itemURI = "/Contact/Supplier/";
                requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
                request.Method = "GET";
                request.Headers["x-myobapi-version"] = "v2";
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential("administrator", "yumgo");

                try
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        Stream responseStream = response.GetResponseStream();
                        using (var reader = new StreamReader(responseStream))
                        {
                            // get the response as text
                            string responseText = reader.ReadToEnd();
                            //_logger.WriteTestLog(responseText);

                            // convert from text
                            var results = JsonConvert.DeserializeObject<JsonConvertSupplier>(responseText);

                            if (results.Items.Length > 0)
                            {
                                foreach (var item in results.Items)
                                {

                                    Myobsync newItem = new Myobsync();

                                    if (_context.GetMPSContext().Myobsync.Any(x => x.Name.Equals(item.CompanyName) && x.Type.Equals(dataType)))
                                    {
                                        newItem = _context.GetMPSContext().Myobsync.Where(x => x.Name.Equals(item.CompanyName) && x.Type.Equals(dataType)).First();

                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Update(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        updateCount++;
                                    }
                                    else
                                    {
                                        newItem.Name = item.CompanyName;

                                        newItem.Type = dataType;
                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Add(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        addedCount++;
                                    }
                                }
                                _logger.WriteTestLog("Sync : [" + dataType + "] " + updateCount + " Updated, " + addedCount + " Added.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
                }
            }
            else if (dataType.Equals("Account"))
            {
                itemURI = "/GeneralLedger/Account/";
                requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
                request.Method = "GET";
                request.Headers["x-myobapi-version"] = "v2";
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential("administrator", "yumgo");

                try
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        Stream responseStream = response.GetResponseStream();
                        using (var reader = new StreamReader(responseStream))
                        {
                            // get the response as text
                            string responseText = reader.ReadToEnd();
                            //_logger.WriteTestLog(responseText);

                            // convert from text
                            var results = JsonConvert.DeserializeObject<JsonConvertAccount>(responseText);

                            if (results.Items.Length > 0)
                            {
                                foreach (var item in results.Items)
                                {

                                    Myobsync newItem = new Myobsync();

                                    if (_context.GetMPSContext().Myobsync.Any(x => x.Name.Equals(item.DisplayID) && x.Type.Equals(dataType)))
                                    {
                                        newItem = _context.GetMPSContext().Myobsync.Where(x => x.Name.Equals(item.DisplayID) && x.Type.Equals(dataType)).First();

                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Update(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        updateCount++;
                                    }
                                    else
                                    {
                                        newItem.Name = item.DisplayID;

                                        newItem.Type = dataType;
                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Add(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        addedCount++;
                                    }
                                }
                                _logger.WriteTestLog("Sync : [" + dataType + "] " + updateCount + " Updated, " + addedCount + " Added.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
                }
            }
            else if (dataType.Equals("Job"))
            {
                itemURI = "/GeneralLedger/Job/";
                requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
                request.Method = "GET";
                request.Headers["x-myobapi-version"] = "v2";
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential("administrator", "yumgo");

                try
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        Stream responseStream = response.GetResponseStream();
                        using (var reader = new StreamReader(responseStream))
                        {
                            // get the response as text
                            string responseText = reader.ReadToEnd();
                            //_logger.WriteTestLog(responseText);

                            // convert from text
                            var results = JsonConvert.DeserializeObject<JsonConvertJob>(responseText);

                            if (results.Items.Length > 0)
                            {
                                foreach (var item in results.Items)
                                {

                                    Myobsync newItem = new Myobsync();

                                    if (_context.GetMPSContext().Myobsync.Any(x => x.Name.Equals(item.Number) && x.Type.Equals(dataType)))
                                    {
                                        newItem = _context.GetMPSContext().Myobsync.Where(x => x.Name.Equals(item.Number) && x.Type.Equals(dataType)).First();

                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Update(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        updateCount++;
                                    }
                                    else
                                    {
                                        newItem.Name = item.Number;

                                        newItem.Type = dataType;
                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Add(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        addedCount++;
                                    }
                                }
                                _logger.WriteTestLog("Sync : [" + dataType + "] " + updateCount + " Updated, " + addedCount + " Added.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
                }
            }
            else if (dataType.Equals("Taxcode"))
            {
                itemURI = "/GeneralLedger/TaxCode/";
                requestAdd = companyFile.Uri.ToString().Replace("localhost", "192.168.1.181") + itemURI;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestAdd);
                request.Method = "GET";
                request.Headers["x-myobapi-version"] = "v2";
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential("administrator", "yumgo");

                try
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        Stream responseStream = response.GetResponseStream();
                        using (var reader = new StreamReader(responseStream))
                        {
                            // get the response as text
                            string responseText = reader.ReadToEnd();
                            //_logger.WriteTestLog(responseText);

                            // convert from text
                            var results = JsonConvert.DeserializeObject<JsonConvertTaxCode>(responseText);

                            if (results.Items.Length > 0)
                            {
                                foreach (var item in results.Items)
                                {

                                    Myobsync newItem = new Myobsync();

                                    if (_context.GetMPSContext().Myobsync.Any(x => x.Name.Equals(item.Code) && x.Type.Equals(dataType)))
                                    {
                                        newItem = _context.GetMPSContext().Myobsync.Where(x => x.Name.Equals(item.Code) && x.Type.Equals(dataType)).First();

                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Update(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        updateCount++;
                                    }
                                    else
                                    {
                                        newItem.Name = item.Code;

                                        newItem.Type = dataType;
                                        newItem.Uid = item.UID.ToString();

                                        _context.GetMPSContext().Myobsync.Add(newItem);
                                        _context.GetMPSContext().SaveChanges();

                                        addedCount++;
                                    }
                                }
                                _logger.WriteTestLog("Sync : [" + dataType + "] " + updateCount + " Updated, " + addedCount + " Added.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.WriteTestLog("Error Message : [" + ex + "] " + ex.Message);
                }
            }
        }

        //End
    }
}
