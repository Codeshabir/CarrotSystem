using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using Xero.Api.Core.Model.Reports;

namespace CarrotSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IEventWriter _logger;
        private readonly IContextService _context;
        private readonly ISystemService _common;
        private readonly IReportsService _report;

        public ReportsController(IEventWriter logger, IContextService context, ISystemService common, IReportsService report)
        {
            _logger = logger;
            _context = context;
            _common = common; 
            _report = report;
        }

        public IActionResult RawProduceList()
        {
            ViewReportSummary viewModel = new ViewReportSummary();

            viewModel.dateFrom = _common.GetMondayByTime(DateTime.Now.AddDays(-7));
            viewModel.dateTo = _common.GetSundayByTime(DateTime.Now.AddDays(-7));

            List<RawProduceView> rawProduceList = new List<RawProduceView>();
            rawProduceList = _report.GetRawProduceList(viewModel.dateFrom.Value, viewModel.dateTo.Value);

            viewModel.rawProduceList = rawProduceList;

            List<EmailGroup> emailList = new List<EmailGroup>();
            emailList = _context.GetMPSContext().EmailGroup.ToList();
            viewModel.emailList = emailList;

            return View("SummaryDetailRawProduce", viewModel);
        }

        public IActionResult IPProduceList()
        {
            ViewReportSummary viewModel = new ViewReportSummary();

            viewModel.dateFrom = _common.GetMondayByTime(DateTime.Now.AddDays(-7));
            viewModel.dateTo = _common.GetSundayByTime(DateTime.Now.AddDays(-7));

            List<RawProduceView> rawProduceList = new List<RawProduceView>();
            rawProduceList = _report.GetIPProduceList(viewModel.dateFrom.Value, viewModel.dateTo.Value);

            viewModel.rawProduceList = rawProduceList;

            List<EmailGroup> emailList = new List<EmailGroup>();
            emailList = _context.GetMPSContext().EmailGroup.ToList();
            viewModel.emailList = emailList;

            return View("SummaryDetailIPProduce", viewModel);
        }

        public IActionResult IFProduceList()
        {
            ViewReportSummary viewModel = new ViewReportSummary();

            viewModel.dateFrom = _common.GetMondayByTime(DateTime.Now.AddDays(-7));
            viewModel.dateTo = _common.GetSundayByTime(DateTime.Now.AddDays(-7));

            List<RawProduceView> rawProduceList = new List<RawProduceView>();
            rawProduceList = _report.GetIFProduceList(viewModel.dateFrom.Value, viewModel.dateTo.Value);

            viewModel.rawProduceList = rawProduceList;

            List<EmailGroup> emailList = new List<EmailGroup>();
            emailList = _context.GetMPSContext().EmailGroup.ToList();
            viewModel.emailList = emailList;

            return View("SummaryDetailIFProduce", viewModel);
        }

        [HttpPost]
        public IActionResult SelectedReportSummaryDetail(ViewReportSummary selectedModel)
        {
            
            var dateFrom = DateTime.Now;
            var dateTo = DateTime.Now;

            dateFrom = DateTime.ParseExact(selectedModel.stringDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            dateTo = DateTime.ParseExact(selectedModel.stringDateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            ViewReportSummary viewModel = new ViewReportSummary();

            viewModel.dateFrom = dateFrom;
            viewModel.dateTo = dateTo;
            viewModel.pageType = selectedModel.pageType;

            List<RawProduceView> rawProduceList = new List<RawProduceView>();

            if (viewModel.pageType.Equals("RProduce"))
            {
                rawProduceList = _report.GetRawProduceList(viewModel.dateFrom.Value, viewModel.dateTo.Value);

                viewModel.rawProduceList = rawProduceList;

                return View("SummaryDetailRawProduce", viewModel);
            }
            else if (viewModel.pageType.Equals("PProduce"))
            {
                rawProduceList = _report.GetIPProduceList(viewModel.dateFrom.Value, viewModel.dateTo.Value);

                viewModel.rawProduceList = rawProduceList;

                return View("SummaryDetailIPProduce", viewModel);
            }
            else if (viewModel.pageType.Equals("FProduce"))
            {
                rawProduceList = _report.GetIPProduceList(viewModel.dateFrom.Value, viewModel.dateTo.Value);

                viewModel.rawProduceList = rawProduceList;

                return View("SummaryDetailIFProduce", viewModel);
            }
            else
            {
                return View("SummaryDetailRawProduce", viewModel);
            }
        }


        // End
    }
}