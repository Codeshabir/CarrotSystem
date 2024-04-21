
using CarrotSystem.Dto;
using CarrotSystem.Helper;
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarrotSystem.Controllers
{
   [Authorize]
    public class HomeController : Controller
    {
        private readonly IContextService _context;
        private readonly ISystemService _system;
        private readonly IEventWriter _logger;
        public string eventBy = "Dashboard";

        public HomeController(IEventWriter logger, IContextService context, ISystemService system)
        {
            _logger = logger;
            _context = context;
            _system = system;
        }

        //GET: Dashboard
        public IActionResult Dashboard()
        {
            ViewDashboard viewModel = new ViewDashboard();
            viewModel.LoginID = HttpContext.User.Identity.Name;
            viewModel.LoginFullName = _system.GetFullNameByLoginID(viewModel.LoginID);

            return View(viewModel);
        }

        [HttpPost("get-dashboard-info")]
        public async Task<IActionResult> GetDashboardInfo(int filterType)
        {
            int purchaseCount = 0;
            int saleCount = 0;
            int expenseCount = 0;
            int stockCount = 0;

            List<string> salesLabels = new List<string>();
            List<int> salesValues = new List<int>();


            List<string> purchaseLabels = new List<string>();
            List<int> purchaseValues = new List<int>();


            if (filterType == 1) 
            {
                // Perform weekly calculations
                purchaseCount = await _context.GetMPSContext().Purchase
                    .Where(p => p.DeliveryDate >= DateTime.Today.AddDays(-7))
                    .AsNoTracking()
                    .CountAsync();

                saleCount = await _context.GetMPSContext().Sale
                    .Where(s => s.DeliveryDate >= DateTime.Today.AddDays(-7))
                    .AsNoTracking()
                    .CountAsync();

                expenseCount = await _context.GetMPSContext().Expense
                    .Where(e => e.UpdatedOn >= DateTime.Today.AddDays(-7))
                    .AsNoTracking()
                    .CountAsync();

                stockCount = await _context.GetMPSContext().StockCount
                    .Where(s => s.UpdatedOn >= DateTime.Today.AddDays(-7))
                    .AsNoTracking()
                    .CountAsync();

                DateTime currentDate = DateTime.Today;
                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = currentDate.AddDays(-i);
                    salesLabels.Add(date.ToString("d MMM"));

                    int count = await _context.GetMPSContext().Purchase
                        .Where(p => p.DeliveryDate == date.Date)
                        .AsNoTracking()
                        .CountAsync();

                    salesValues.Add(count);
                }

                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = currentDate.AddDays(-i);
                    purchaseLabels.Add(date.ToString("d MMM"));

                    int count = await _context.GetMPSContext().Purchase
                        .Where(p => p.DeliveryDate == date.Date)
                        .AsNoTracking()
                        .CountAsync();

                    purchaseValues.Add(count);
                }



                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = currentDate.AddDays(-i);
                    salesLabels.Add(date.ToString("d MMM"));

                    int count = await _context.GetMPSContext().Sale
                        .Where(p => p.DeliveryDate == date.Date)
                        .AsNoTracking()
                        .CountAsync();

                    salesValues.Add(count);
                }
                
            }

            if (filterType == 2) // Monthly
            {
                // Perform monthly calculations
                purchaseCount = await _context.GetMPSContext().Purchase
                    .Where(p => p.DeliveryDate.Value.Year == DateTime.Today.Year && p.DeliveryDate.Value.Month == DateTime.Today.Month)
                    .AsNoTracking()
                    .CountAsync();

                saleCount = await _context.GetMPSContext().Sale
                    .Where(s => s.DeliveryDate.Value.Year == DateTime.Today.Year && s.DeliveryDate.Value.Month == DateTime.Today.Month)
                    .AsNoTracking()
                    .CountAsync();

                expenseCount = await _context.GetMPSContext().Expense
                    .Where(e => e.UpdatedOn.Value.Year == DateTime.Today.Year && e.UpdatedOn.Value.Month == DateTime.Today.Month)
                    .AsNoTracking()
                    .CountAsync();

                stockCount = await _context.GetMPSContext().StockCount
                    .Where(s => s.UpdatedOn.Value.Year == DateTime.Today.Year && s.UpdatedOn.Value.Month == DateTime.Today.Month)
                    .AsNoTracking()
                    .CountAsync();


                DateTime currentDate = DateTime.Today;
                int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                for (int i = 1; i <= daysInMonth; i++)
                {
                    DateTime date = new DateTime(currentDate.Year, currentDate.Month, i);
                    purchaseLabels.Add(date.ToString("d MMM"));
                    int count = await _context.GetMPSContext().Purchase
                        .Where(p => p.DeliveryDate.Value.Year == date.Year 
                        && p.DeliveryDate.Value.Month == date.Month)
                        .AsNoTracking()
                        .CountAsync();
                    purchaseValues.Add(count);
                }

                for (int i = 1; i <= daysInMonth; i++)
                {
                    DateTime date = new DateTime(currentDate.Year, currentDate.Month, i);
                    salesLabels.Add(date.ToString("d MMM"));
                    int count = await _context.GetMPSContext().Purchase
                        .Where(p => p.DeliveryDate.Value.Year == date.Year
                        && p.DeliveryDate.Value.Month == date.Month)
                        .AsNoTracking()
                        .CountAsync();
                    salesValues.Add(count);
                }

            }
            
            if (filterType == 3) // Yearly
            {
                // Perform yearly calculations
                purchaseCount = await _context.GetMPSContext().Purchase
                    .Where(p => p.DeliveryDate.Value.Year == DateTime.Today.Year)
                    .AsNoTracking()
                    .CountAsync();

                saleCount = await _context.GetMPSContext().Sale
                    .Where(s => s.DeliveryDate.Value.Year == DateTime.Today.Year)
                    .AsNoTracking()
                    .CountAsync();

                expenseCount = await _context.GetMPSContext().Expense
                    .Where(e => e.UpdatedOn.Value.Year == DateTime.Today.Year)
                    .AsNoTracking()
                    .CountAsync();

                stockCount = await _context.GetMPSContext().StockCount
                    .Where(s => s.UpdatedOn.Value.Year == DateTime.Today.Year)
                    .AsNoTracking()
                    .CountAsync();

                // Get current year months
                DateTime currentDate = DateTime.Today;
                for (int i = 1; i <= 12; i++)
                {
                    DateTime date = new DateTime(currentDate.Year, i, 1);
                    purchaseLabels.Add(date.ToString("MMM"));
                    int count = await _context.GetMPSContext().Purchase
                       .Where(p => p.DeliveryDate.Value.Year == date.Year
                       && p.DeliveryDate.Value.Month == date.Month)
                       .AsNoTracking()
                       .CountAsync();
                    purchaseValues.Add(count);
                }

                for (int i = 1; i <= 12; i++)
                {
                    DateTime date = new DateTime(currentDate.Year, i, 1);
                    salesLabels.Add(date.ToString("MMM"));
                    int count = await _context.GetMPSContext().Sale
                       .Where(p => p.DeliveryDate.Value.Year == date.Year
                       && p.DeliveryDate.Value.Month == date.Month)
                       .AsNoTracking()
                       .CountAsync();
                    salesValues.Add(count);
                }


            }

            var dashboard = new DashboardDto()
            {
                Purchase = purchaseCount,
                Sale = saleCount,
                Expense = expenseCount,
                Stock = stockCount,
                PurchaseLabels = purchaseLabels,
                PurchaseValues = purchaseValues,
                SalesLabels = salesLabels,
                SalesValues = salesValues
            };

            return Ok(dashboard);
        }



        

        //User List
        public IActionResult UsersList()
        {
            ViewUser viewModel = new ViewUser();

            List<User> usersList = new List<User>();

            usersList = _context.GetMPSContext().Users.Where(w => w.IsActivated.Equals(true)).ToList();

            viewModel.userList = usersList;

            return View(viewModel);
        }

        //Settings
        public IActionResult EmailHistory()
        {
            ViewHistory viewModel = new ViewHistory();

            List<Event> eventList = new List<Event>();

            eventList = _context.GetMPSContext().Events.Where(w =>w.EventType.Equals("Emails")).OrderByDescending(o => o.EventDate).ToList();

            viewModel.eventList = eventList;

            return View(viewModel);
        }

        //Settings
        public IActionResult EmailGroup()
        {
            ViewSettings viewModel = new ViewSettings();

            List<EmailGroup> emailList = new List<EmailGroup>();

            emailList = _context.GetMPSContext().EmailGroup.Where(w => w.Id > 0).ToList();

            viewModel.emailList = emailList;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddEmail(string newName, string newEmail)
        {
            var userName = _system.GetFullNameByLoginID(HttpContext.User.Identity.Name);

            if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(newEmail))
            {
                EmailGroup newEmailGroup = new EmailGroup();

                newEmailGroup.GroupName = newName;
                newEmailGroup.EmailAddress = newEmail.Trim();

                _context.GetMPSContext().EmailGroup.Add(newEmailGroup);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Email", "Added", "Email [" + newEmailGroup.EmailAddress + "] Added");
                }
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult EditEmail(int emailId, string editName, string editEmail)
        {
            var userName = _system.GetFullNameByLoginID(HttpContext.User.Identity.Name);

            if (emailId > 0)
            {
                if (!string.IsNullOrEmpty(editName) && !string.IsNullOrEmpty(editEmail))
                {
                    var editEmailGroup = _context.GetMPSContext().EmailGroup.Where(w => w.Id.Equals(emailId)).First();
                    
                    editEmailGroup.GroupName = editName;
                    editEmailGroup.EmailAddress = editEmail.Trim();

                    _context.GetMPSContext().EmailGroup.Update(editEmailGroup);
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "Email", "Adjustment", "Email [" + editEmailGroup.EmailAddress + "] Updated");
                    }
                }
            }

            return new JsonResult("Success");
        }

        public IActionResult DeleteEmail(int emailId)
        {
            var userName = _system.GetFullNameByLoginID(HttpContext.User.Identity.Name);

            if (emailId > 0)
            {
                var editEmailGroup = _context.GetMPSContext().EmailGroup.Where(w => w.Id.Equals(emailId)).First();

                _context.GetMPSContext().EmailGroup.Update(editEmailGroup);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Email", "Deleted", "Email ID#" + emailId + " Deleted");
                }
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult GetDetails(string content, int contentId)
        {
            if (content.Equals("Email"))
            {
                if (contentId > 0)
                {
                    var returnData = _context.GetMPSContext().EmailGroup.Where(w => w.Id.Equals(contentId)).First();

                    return new JsonResult(returnData);
                }
            }

            return new JsonResult("");
        }

        [HttpPost]
        public IActionResult GetUserData(string dataId)
        {
            var user = _context.GetMPSContext().Users.Where(w => w.LoginId.Equals(dataId)).First();

            return new JsonResult(user);
        }

        //POST : Edit User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(ViewUser viewModel)
        {
            var userName = _system.GetFullNameByLoginID(HttpContext.User.Identity.Name);
            var editUser = _context.GetMPSContext().Users.Where(w => w.LoginId.Equals(viewModel.loginId)).First();

            editUser.FirstName = viewModel.user.FirstName;
            editUser.LastName = viewModel.user.LastName;
            editUser.Role = viewModel.user.Role;

            if (!string.IsNullOrEmpty(viewModel.user.Password))
            {
                editUser.Password = PasswordHelper.GetPasswordHash(viewModel.user.Password);
            }

            if (!string.IsNullOrEmpty(viewModel.user.MobileNumbers))
            {
                editUser.MobileNumbers = viewModel.user.MobileNumbers;
            }

            if (!string.IsNullOrEmpty(viewModel.user.Email))
            {
                editUser.Email = viewModel.user.Email;
            }

            editUser.IsModified = true;
            editUser.ModifiedBy = userName;
            editUser.DateModified = DateTime.Now;

            _context.GetMPSContext().Users.Update(editUser);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "User", "Adjustment", "Login ID :" + viewModel.loginId + " Updated");
            }

            return RedirectToAction("UsersList");
        }
    }
}
