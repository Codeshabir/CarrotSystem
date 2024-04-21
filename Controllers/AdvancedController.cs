using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarrotSystem.Controllers
{
   // [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class AdvancedController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly ISystemService _service;
        private readonly IEventWriter _logger;
        public string eventBy = "Dashboard";

        public AdvancedController(IEventWriter logger, IContextService context, ISystemService service, IAPIService api)
        {
            _logger = logger;
            _context = context;
            _service = service;
            _api = api;
        }

        public IActionResult PeriodList()
        {
            ViewAdvanced viewModel = new ViewAdvanced();

            DateTime today = DateTime.Now;

            _api.CheckAndGeneratePeriod();

            List<Period> periodList = new List<Period>();
            periodList = GetPeriodList(today);

            viewModel.focusId = _context.GetMPSContext().Period.Where(w => today.Date.CompareTo(w.StartDate.Value.Date) >= 0 && today.Date.CompareTo(w.EndDate.Value.Date) <= 0).First().Id;
            viewModel.periodList = periodList;
            viewModel.endDateList = GetEndDateList(periodList);

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SelectedPeriodList(ViewDefinitions selectedModel)
        {
            ViewAdvanced viewModel = new ViewAdvanced();

            DateTime today = DateTime.Now;

            List<Period> periodList = new List<Period>();
            periodList = GetPeriodList(today);

            viewModel.focusId = _context.GetMPSContext().Period.Where(w => today.Date.CompareTo(w.StartDate.Value.Date) >= 0 && today.Date.CompareTo(w.EndDate.Value.Date) <= 0).First().Id;
            viewModel.periodList = periodList;
            viewModel.endDateList = GetEndDateList(periodList);

            return View("PeriodList", viewModel);
        }

        public IActionResult NewPeriod()
        {
            var currentLast = _context.GetMPSContext().Period.OrderByDescending(o => o.Id).FirstOrDefault();

            if(currentLast != null)
            {
                Period newPeriod = new Period();
                newPeriod.Calculated = false;
                newPeriod.StartDate = currentLast.StartDate.Value.AddDays(7);
                newPeriod.EndDate = currentLast.EndDate.Value.AddDays(7);
                newPeriod.Status = "Open";
                newPeriod.UpdatedBy = "System";
                newPeriod.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().Period.Add(newPeriod);
                _context.GetMPSContext().SaveChanges();
            }

            return RedirectToAction("PeriodList");
        }

        public IActionResult TaxRates()
        {
            ViewAdvanced viewModel = new ViewAdvanced();

            List<Tax> taxratesList = new List<Tax>();
            taxratesList = _context.GetMPSContext().Tax.ToList();

            viewModel.taxrateList = taxratesList;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult GetPeriod (string period)
        {
            DateTime basis = DateTime.ParseExact(period, "dd/MM/yy", CultureInfo.InvariantCulture);

            var periodId = _context.GetMPSContext().Period.Where(w => basis.Date.CompareTo(w.StartDate.Value.Date) >= 0 && basis.Date.CompareTo(w.EndDate.Value.Date) <= 0).First().Id;

            return new JsonResult(periodId);
        }

        public List<Period> GetPeriodList(DateTime basis)
        {
            DateTime dateFrom = DateTime.ParseExact("01/07/"+ basis.AddYears(-1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateTo = DateTime.ParseExact("30/06/" + basis.AddYears(1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            List<Period> periodList = new List<Period>();
            periodList = _context.GetMPSContext().Period.Where(w=>w.StartDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.StartDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

            return periodList;
        }

        public List<string> GetEndDateList(List<Period> periodList)
        {
            List<string> endDateList = new List<string>();
            
            foreach(var period in periodList)
            {
                endDateList.Add(period.EndDate.Value.ToString("dd/MM/yy"));
            }

            return endDateList;
        }

        public IActionResult ActivePeriod(string type, string endDate)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var periodEndDate = DateTime.ParseExact(endDate, "dd/MM/yy", CultureInfo.InvariantCulture);

            if (type.Equals("open"))
            {
                _api.OpenPeriodByEndDate(periodEndDate, userName);
            }
            else
            {
                _api.ClosePeriodByEndDate(periodEndDate, userName);
            }

            return new JsonResult("Success");
        }

        //End Methods
    }
}
