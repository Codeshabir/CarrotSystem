
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarrotSystem.Controllers
{
    public class AuditController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly ICalcService _calc;
        private readonly ISystemService _system;
        private readonly IEventWriter _logger;

        public string eventBy = "Purchase";

        public AuditController(IEventWriter logger, IContextService context, ISystemService system, IAPIService api, ICalcService calc)
        {
            _logger = logger;
            _context = context;
            _system = system;
            _api = api;
            _calc = calc;
        }

        public IActionResult EventViewer()
        {
            ViewAudit viewModel = new ViewAudit();

            DateTime dateFrom = DateTime.Now.AddMonths(-1);
            DateTime dateTo = DateTime.Now;

            viewModel.dateFrom = dateFrom;
            viewModel.dateTo = dateTo;
            viewModel.eventType = "All";
            viewModel.actionType = "All";

            List<Event> eventList = GetEventList(dateFrom, dateTo, viewModel.eventType, viewModel.actionType);
            viewModel.eventList = eventList;

            // Group By Event Type
            var eventTypeList = _context.GetMPSContext().EventSettings.GroupBy(g => g.EventType).Select(s => s.Key).ToList();
            viewModel.eventTypeList = eventTypeList;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SelectedEventList(ViewAudit selectedModel)
        {
            ViewAudit viewModel = new ViewAudit();

            DateTime dateFrom = DateTime.ParseExact(selectedModel.strDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateTo = DateTime.ParseExact(selectedModel.strDateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            viewModel.dateFrom = dateFrom;
            viewModel.dateTo = dateTo;
            viewModel.eventType = selectedModel.eventType;
            viewModel.actionType = selectedModel.actionType;

            List<Event> eventList = GetEventList(dateFrom, dateTo, viewModel.eventType, viewModel.actionType);
            viewModel.eventList = eventList;

            var eventTypeList = eventList.GroupBy(g => g.EventType).Select(s => s.Key).ToList();
            viewModel.eventTypeList = eventTypeList;

            return View("EventViewer", viewModel);
        }

        public List<Event> GetEventList(DateTime dateFrom, DateTime dateTo, string eventType, string actionType)
        {
            List<Event> evntList = new List<Event>();

            evntList = _context.GetMPSContext().Events.Where(w => w.EventDate.HasValue && w.EventDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.EventDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

            if(!eventType.Equals("All"))
            {
                evntList = evntList.Where(w=>w.EventType.Equals(eventType)).ToList();
            }

            if(!actionType.Equals("All"))
            {
                evntList = evntList.Where(w=>w.ActionType.Equals(actionType)).ToList();
            }

            return evntList;
        }

        [HttpPost]
        public IActionResult GetActionType(string eventType)
        {
            var returnList = _context.GetMPSContext().EventSettings.Where(w=>w.EventType.Equals(eventType)).Select(s=>s.ActionType).ToList();

            return new JsonResult(returnList);
        }



        //End Methods
    }
}
