
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CarrotSystem.Services;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Models.MPS;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CarrotSystem.Controllers
{
    //[Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class APIController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly IMYOBService _myob;
        private readonly ICalcService _calc;
        private readonly ISystemService _system;
        private readonly IEventWriter _logger;

        public APIController(IEventWriter logger, IContextService context, ISystemService system, IAPIService api, ICalcService calc, IMYOBService myob)
        {
            _logger = logger;
            _context = context;
            _system = system;
            _api = api;
            _calc = calc;
            _myob = myob;
        }

        [HttpPost]
        public IActionResult GetProduct(int productId)
        {
            ProductJsonView retrunItem = new ProductJsonView();

            var product = _context.GetMPSContext().Product.Where(w => w.Id.Equals(productId)).First();

            retrunItem.ProductId = product.Id;
            retrunItem.ProductCode = product.Code;
            retrunItem.ProductDesc = product.Desc;
            retrunItem.TaxName = product.Tax;

            if (_context.GetMPSContext().ProductMapping.Any(a => a.MercCode.Equals(retrunItem.ProductCode)))
            {
                var mapping = _context.GetMPSContext().ProductMapping.First(a => a.MercCode.Equals(retrunItem.ProductCode));

                retrunItem.CustomerCode = mapping.CompanyCode;
                retrunItem.CustomerDesc = mapping.CompanyDesc;
            }
            else
            {
                retrunItem.CustomerCode = "N/A";
                retrunItem.CustomerDesc = "N/A";
            }

            return new JsonResult(retrunItem);
        }

        [HttpPost]
        public IActionResult GetShipList(int customerId)
        {
            var company = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(customerId)).First();

            var returnList = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName) && w.Type.Equals("Shipping")).ToList();

            return new JsonResult(returnList);
        }

        [HttpPost]
        public IActionResult IsClosedPeriod(string targetDate)
        {
            var target = DateTime.ParseExact(targetDate, "ddMMyyyy", CultureInfo.InvariantCulture);
            var isClosed = _api.IsClosedPeriodByDate(target);

            if(isClosed)
            {
                return new JsonResult("Closed");
            }
            else
            {
                return new JsonResult("Open");
            }
        }

        [HttpPost]
        public IActionResult IsClosedPeriodById(int periodId)
        {
            return new JsonResult(_api.IsClosedPeriodById(periodId));;
        }

        public IActionResult CalcResult(int periodId)
        {
            CalcViewModel viewModel = new CalcViewModel();
            List<ProductInventory> invenList = new List<ProductInventory>();
            invenList = _context.GetMPSContext().ProductInventory.Where(a => a.PeriodId.Equals(periodId)).OrderBy(x=>x.ProductCode).ToList();

            viewModel.invenList = invenList;

            return View("CalcResult", viewModel);
        }

        //Journal No, Date, Memo, GST [BAS] Reporting, Inclusive, Account No, Debit Ex-Tax, Debit Inc-Tax, Credit Ex-Tax, Credit Inc-Tax, Job, Tax Code, Non-GST/LCT Amount, Tax Amount, LCT Amount, Import Duty Amount
        public void ExportTCC(string type, string exportDate, string userName)
        {
            _myob.ExportToMYOB(type, exportDate, userName);
        }

        //Co./Last Name , First Name , Addr 1 - Line 1 , - Line 2 , - Line 3 , - Line 4 , Inclusive , Invoice # , Date , Customer PO , Ship Via , Delivery Status , Item Number , Quantity , Description , Price , Inc-Tax Price , Discount , Total , Inc-Tax Total , Job , Comment , Journal Memo , Salesperson Last Name , Salesperson First Name , Shipping Date , Referral Source , Tax Code , Non-GST Amount , GST Amount ,  , Filter
        public void ExportLines(string type, string exportData, string exportDate, string userBy)
        {
            _logger.WriteTestLog("Exported Line from Excel : [" + type + "] " + exportData + ", " + exportDate + " by " + userBy);

            _myob.ExportToDBbyLines(type, exportData, exportDate, userBy);
            
        }

        public void ClosePeriodByEndDate(string endDate, string updateBy)
        {
            var periodEndDate = DateTime.ParseExact(endDate, "dd/MM/yy", CultureInfo.InvariantCulture);

            _api.ClosePeriodByEndDate(periodEndDate, updateBy);

            _logger.WriteTestLog("Close Period " + periodEndDate.ToString("dd/MM/yyyy") + ", Update by " + updateBy);
        }

        public IActionResult LatestCalculation()
        {
            var updateBy = "Dashboard";
            var periodEndDate = _system.GetSaturdayByTime(DateTime.Now);
            
            var period = _context.GetMPSContext().Period.Where(w => w.EndDate.HasValue && w.EndDate.Value.Equals(periodEndDate)).First();

            period.Calculated = false;

            //_calc.UpdateInventory(period, updateBy);

            period.Calculated = true;

            _context.GetMPSContext().Period.Update(period);
            _context.GetMPSContext().SaveChanges();

            _logger.WriteTestLog("Calculation " + period.StartDate.Value.ToString("dd/MM/yyyy") + " - " + period.EndDate.Value.ToString("dd/MM/yyyy"));

            return new JsonResult("Success");
        }

        public void Calculation(string endDate, string updateBy)
        {
            var periodEndDate = DateTime.ParseExact(endDate, "dd/MM/yy", CultureInfo.InvariantCulture);

            var period = _context.GetMPSContext().Period.Where(w=>w.EndDate.HasValue && w.EndDate.Value.Equals(periodEndDate)).First();

            period.Calculated = false;

            //_calc.UpdateInventory(period, updateBy);

            period.Calculated = true;

            _context.GetMPSContext().Period.Update(period);
            _context.GetMPSContext().SaveChanges();

            _logger.WriteTestLog("Calculation " + period.StartDate.Value.ToString("dd/MM/yyyy") + " - " + period.EndDate.Value.ToString("dd/MM/yyyy"));
        }

        public void TestCalc()
        {
            _logger.WriteTestLog("Test Calculation...");

            var periodEndDate = DateTime.ParseExact("27/05/23", "dd/MM/yy", CultureInfo.InvariantCulture);

            var period = _context.GetMPSContext().Period.Where(w => w.EndDate.HasValue && w.EndDate.Value.Equals(periodEndDate)).First();

            period.Calculated = false;

            _calc.UpdateInventory(period, "Test");

            period.Calculated = true;

            //_context.GetMPSContext().Period.Update(period);
            //_context.GetMPSContext().SaveChanges();

            //return RedirectToAction("CalcResult", new { periodId = period.Id });
            
        }

        public void CalcPeriod(int periodId)
        {
            var useBy = HttpContext.User.Identity.Name;

            _logger.WriteTestLog("Test Calculation..." + useBy);

            var periodList = _context.GetMPSContext().Period.Where(w => w.Id == periodId && w.Calculated.HasValue && w.Calculated.Value.Equals(false)).OrderBy(x=>x.Id).ToList();

            _logger.WriteTestLog("Calculation..." + periodList.Count);

            //if(periodList.Count > 0)
            //{
            //    foreach (var period in periodList)
            //    {
            //        _logger.WriteTestLog(period.EndDate.Value.ToString("dd/MM/yyyy"));

            //        _calc.UpdateInventory(period, useBy);
            //    }
            //}

            var period = _context.GetMPSContext().Period.Where(x => x.Id == periodId).FirstOrDefault();
            if(period != null)
            {
                period.Calculated = true;
                period.UpdatedBy = useBy;
                period.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().Period.Update(period);
                _context.GetMPSContext().SaveChanges();
            }

            //_calc.UpdateInventory(period, "Test");
        }

        public IActionResult GetPeriodCount (int periodId)
        {
            var periodList = _context.GetMPSContext().Period.Where(w => w.Id <= periodId && w.Calculated.HasValue && w.Calculated.Value.Equals(false)).ToList();

            return new JsonResult(periodList.Count);
        }

        public IActionResult MakeUncalculated(int periodId)
        {
            if(_context.GetMPSContext().Period.Any(x=>x.Id==periodId))
            {
                var period = _context.GetMPSContext().Period.Where(x => x.Id == periodId).First();

                period.Calculated = false;
                period.UpdatedOn = DateTime.Now;
                period.UpdatedBy = HttpContext.User.Identity.Name;

                _context.GetMPSContext().Period.Update(period);
                _context.GetMPSContext().SaveChanges();

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }
        }

        //End Methods
    }
}
