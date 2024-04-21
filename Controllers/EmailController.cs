
using CarrotSystem.Helpers;
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MYOB.AccountRight.SDK.Services;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace CarrotSystem.Controllers
{
   // [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class EmailController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly IEventWriter _logger;
        private IEmailService _emailService;
        private readonly IGenPDFService _pdfService;
        public string eventBy = "Email";

        public EmailController(IEventWriter logger, IContextService context, IEmailService emailservice, IAPIService api, IGenPDFService pdfService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailservice;
            _api = api;
            _pdfService = pdfService;
        }

        [HttpPost]
        public IActionResult SendEmailWithAttacnment(string sendType, int sendTo, int targetId)
        {
            //Email Information
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var email = _context.GetMPSContext().EmailGroup.Where(w => w.Id == sendTo).FirstOrDefault();

            //Don't Change from Address
            string from = "admin@myproductionsystem.au";

            if (email != null) 
            {
                string subject = "MPS " + sendType + "_" + DateTime.Now.ToString("yyyyMMdd");
                
                string emailMessage = "Please find the attached file.<br />" +
                                   "Thank you and kind regards. <br /><br />" +
                                   "<b>[THIS IS AN AUTOMATED MESSAGE - PLEASE DO NOT REPLY DIRECTLY TO THIS EMAIL]</b>";

                var pdfFile = _pdfService.GeneratePDF(subject, sendType, targetId);

                _emailService.ExecuteWithFile(from, email.EmailAddress, subject, emailMessage, new MemoryStream(pdfFile), subject + ".pdf");
                
                _logger.WriteEvents(userName, "Email", "Sent", sendType);

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }

        }

        [HttpPost]
        public IActionResult SendReportEmailWithAttacnment(string sendType, int sendTo, string stringDateFrom, string stringDateTo)
        {
            //Email Information
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var email = _context.GetMPSContext().EmailGroup.Where(w => w.Id == sendTo).FirstOrDefault();

            var dateFrom = DateTime.ParseExact(stringDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var dateTo = DateTime.ParseExact(stringDateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            //Don't Change from Address
            string from = "admin@myproductionsystem.au";

            if (email != null)
            {
                string subject = "MPS " + sendType + "_" + DateTime.Now.ToString("yyyyMMdd");

                string emailMessage = "Please find the attached file.<br />" +
                                   "Thank you and kind regards. <br /><br />" +
                                   "<b>[THIS IS AN AUTOMATED MESSAGE - PLEASE DO NOT REPLY DIRECTLY TO THIS EMAIL]</b>";

                var pdfFile = _pdfService.GenerateReportPDF(subject, sendType, dateFrom, dateTo);

                _emailService.ExecuteWithFile(from, email.EmailAddress, subject, emailMessage, new MemoryStream(pdfFile), subject + ".pdf");

                _logger.WriteEvents(userName, "Email", "Sent", sendType);

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }

        }


        // End Methods
    }
}
