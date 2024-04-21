using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Encodings.Web;

namespace CarrotSystem.Controllers
{
   // [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class AccountingController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly IEventWriter _logger;
        private readonly IGenPDFService _pdfService;
        public string eventType = "Accounting";

        public AccountingController(IEventWriter logger, IContextService context, IAPIService api, IGenPDFService pdfService)
        {
            _logger = logger;
            _context = context;
            _pdfService = pdfService;
            _api = api;
        }

        public IActionResult Expenses()
        {
            ViewAccounting viewModel = new ViewAccounting();

            _api.CheckAndGeneratePeriod();

            List<PeriodicView> periodList = new List<PeriodicView>();
            periodList = _api.GetPeriodicList(DateTime.Now, "Expenses");
            viewModel.periodList = periodList;

            var lastPeriodId = 0;
            lastPeriodId = _api.GetLatePeriodId("Expenses");
            viewModel.lastPeriodId = lastPeriodId;

            return View(viewModel);
        }

        public IActionResult GetExpenseList(string periodId)
        {
            List<ExpenseDetailView> expenseViewList = new List<ExpenseDetailView>();

            expenseViewList = _api.GetExpenseList(periodId);

            return PartialView("_ExpenseDetails", expenseViewList.OrderBy(o => o.ExpenseCode).ToList());
        }

        [HttpPost]
        public IActionResult ChangeExpense(int itemId, string type, string chgValue)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (itemId > 0)
            {
                var item = _context.GetMPSContext().Expense.Where(w => w.Id == itemId).FirstOrDefault();
                
                if(item != null) 
                {
                    var originalValue = "";

                    if (type.Equals("Price"))
                    {
                        originalValue = item.Price.ToString();
                        item.Price = double.Parse(chgValue);
                    }

                    item.UpdatedBy = userName;
                    item.UpdatedOn = DateTime.Now;

                    _context.GetMPSContext().Expense.Update(item);
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "Expense", "Adjustment", "Expense [" + item.ExpenseCode + "] " + type + " Adjustment " + originalValue + " -> " + item.Price);
                    }

                    return new JsonResult(_api.GetExpenseTotal(item.PeriodId.Value));
                }
                else
                {
                    return new JsonResult("Failed");
                }
                
            }
            else
            {
                return new JsonResult("Failed");
            }

        }

        public IActionResult CalculateList()
        {
            ViewAdvanced viewModel = new ViewAdvanced();

            DateTime today = DateTime.Now;

            _api.CheckAndGeneratePeriod();

            List<Period> periodList = new List<Period>();
            periodList = GetPeriodList(today);

            var lastPeriodId = 0;
            lastPeriodId = _context.GetMPSContext().Period.Where(w => today.Date.CompareTo(w.StartDate.Value.Date) >= 0 && today.Date.CompareTo(w.EndDate.Value.Date) <= 0).First().Id;
            viewModel.focusId = lastPeriodId;
            viewModel.periodList = periodList;

            return View(viewModel);
        }

        public List<Period> GetPeriodList(DateTime basis)
        {
            DateTime dateFrom = DateTime.ParseExact("01/07/" + basis.AddYears(-1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateTo = DateTime.ParseExact("30/06/" + basis.AddYears(1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            List<Period> periodList = new List<Period>();
            periodList = _context.GetMPSContext().Period.Where(w => w.StartDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.StartDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

            return periodList;
        }

        [HttpPost]
        public IActionResult DeleteItem(string type, int itemId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (type.Equals("Expenses"))
            {
                var deleteList = _context.GetMPSContext().Expense.Where(w => w.PeriodId.Equals(itemId)).ToList();

                _context.GetMPSContext().Expense.RemoveRange(deleteList);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Expense", "Deleted", "Expense ID#" + itemId + " Deleted");
                }
            }

            return new JsonResult("Success");
        }


        //End
    }
}
