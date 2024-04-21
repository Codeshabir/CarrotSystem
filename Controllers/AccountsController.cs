using CarrotSystem.Helper;
using CarrotSystem.Helpers;
using CarrotSystem.Models;
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace CarrotSystem.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IContextService _context;
        private readonly IEventWriter _logger;
        private readonly IAPIService _api;
       // private readonly IEmailService _emailService;

        public string eventBy = "Account System";

        public AccountsController(IEventWriter logger, IContextService context, IAPIService api)
        {
            _logger = logger;
            _context = context;
            _api = api;
          //  _emailService = email;
        }

        //GET: Index
       // [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        
        [AllowAnonymous]
        public IActionResult Login()
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // POST: Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(string loginid, string password, string rememberme)
        {
            ViewBag.Color = "text-danger";

            if (_context.GetMPSContext().Users.Any(u => u.LoginId == loginid))
            {
                var _user = _context.GetMPSContext().Users.Where(u => u.LoginId == loginid).FirstOrDefault();

                if (PasswordHelper.Verify(password, _user.Password))
                {
                    if (_user.IsActivated)
                    {
                        bool rem = rememberme != null ? true : false;

                        //HttpContext.Session.SetString("loginId", loginid);
                        //HttpContext.Session.SetInt32("remember", rem ? 1 : 0);

                        await SaveLoggedInAsync(rem, _user);
                        
                        return RedirectToAction("Dashboard", "Home");
                    }
                    else
                    {
                        ViewBag.Message = "Your Account is not activated";
                        return View();
                    }
                }
                else
                {
                    ViewBag.Message = "Please Check your Password or Login ID";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "Invalid Login ID or Password";
                return View();
            }
        }

        //Get Logout
        [AllowAnonymous]
        public async Task<ActionResult> Logout()
        {
            string name = HttpContext.User.Identity.Name;

            if(!string.IsNullOrEmpty(name))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                //Update Login Date at DB and Log
                LoginDateUpdate(name, "Logout");
            }

            return RedirectToAction("Login");
        }

        [Authorize(Roles = "Manager,SystemAdmin")]
        [HttpPost]
        public IActionResult EditUser(ViewUser viewModel)
        {
            string userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (viewModel.userAddType == "New")
            {
                var newUser = new User();

                newUser.LoginId = viewModel.user.LoginId;
                newUser.Password = PasswordHelper.GetPasswordHash(viewModel.user.Password.Trim());
                newUser.FirstName = viewModel.user.FirstName;
                newUser.LastName = viewModel.user.LastName;
                newUser.EmployeeCode = newUser.FirstName.Substring(0, 2) + newUser.LastName.Substring(0, 2);

                if (!string.IsNullOrEmpty(viewModel.user.MobileNumbers))
                {
                    newUser.MobileNumbers = viewModel.user.MobileNumbers;
                }

                newUser.Email = viewModel.user.Email;
                newUser.Role = viewModel.user.Role;
                newUser.IsActivated = true;

                newUser.DateCreated = DateTime.Now;
                newUser.DateModified = DateTime.Now;

                _context.GetMPSContext().Users.Add(newUser);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "User", "Added", "User ID [" + newUser.LoginId + "] Added");
                }
            }
            else if (viewModel.userAddType == "Edit")
            {
                var editUser = _context.GetMPSContext().Users.FirstOrDefault(x => x.LoginId == viewModel.user.LoginId);
                
                if(editUser != null)
                {
                    //editUser.Password = PasswordHelper.GetPasswordHash(viewModel.user.Password.Trim());
                    editUser.FirstName = viewModel.user.FirstName;
                    editUser.LastName = viewModel.user.LastName;
                    editUser.EmployeeCode = editUser.FirstName.Substring(0, 2) + editUser.LastName.Substring(0, 2);
                    editUser.Email = viewModel.user.Email;
                    editUser.Role = viewModel.user.Role;
                    editUser.IsActivated = true;

                    if (string.IsNullOrEmpty(viewModel.user.MobileNumbers))
                    {
                        editUser.MobileNumbers = "";
                    }
                    else
                    {
                        editUser.MobileNumbers = viewModel.user.MobileNumbers;
                    }

                    editUser.DateModified = DateTime.Now;

                    _context.GetMPSContext().Users.Update(editUser);
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "User", "Updated", "User ID [" + editUser.LoginId + "] Updated");
                    }
                }
            }

            return RedirectToAction("UsersList", "Home");
        }

        [Authorize(Roles = "Manager,SystemAdmin")]
        [HttpPost]
        public IActionResult DeleteUser(string loginId)
        {
            string userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (_context.GetMPSContext().Users.Any(x=>x.LoginId.Equals(loginId)))
            {
                var user = _context.GetMPSContext().Users.Where(x=>x.LoginId.Equals(loginId)).First();

                _context.GetMPSContext().Users.Remove(user);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "User", "Deleted", "User ID [" + loginId + "] Deleted");
                }
            }

            return new JsonResult("Success");
        }

        //Update Login Date
        [AllowAnonymous]
        [HttpPost]
        public void LoginDateUpdate(string userName, string bindName)
        {
            if (bindName.Equals("Login") || bindName.Equals("Logout"))
            {
                var _user = _context.GetMPSContext().Users.Where(u => u.LoginId.Equals(userName)).First();
                var fullName = _user.FirstName + " " + _user.LastName;

                if (_user == null)
                {
                    string descrip = "Unable to load user for update last login.";
                }

                if (bindName.Equals("Login"))
                {
                    _user.DateLastLogin = DateTime.Now;
                    _user.DateLastLogout = DateTime.Now;
                }
                else if (bindName.Equals("Logout"))
                {
                    _user.DateLastLogout = DateTime.Now;
                }

                _context.GetMPSContext().Update(_user);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(fullName, "User", bindName, "User [" + fullName + "] " + bindName);
                }
            }

            return;
        }

        [AllowAnonymous]
        private async Task SaveLoggedInAsync(bool v, User user)
        {
            var days = v ? DateTime.UtcNow.AddDays(3) : DateTime.UtcNow.AddHours(1);
            var claims = new List<Claim>() {
                        new Claim(ClaimTypes.Name, user.LoginId),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
            {
                IsPersistent = v,
                AllowRefresh = true,
                ExpiresUtc = days
            });

            // Update Login Date at DB and Log
            LoginDateUpdate(user.LoginId, "Login");
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ForgotPassword(string Email)
        {
            var user = _context.GetMPSContext().Users.FirstOrDefault(s => (!string.IsNullOrEmpty(s.Email)) && s.Email == Email);
            
            if (user != null)
            {
                user.Token = Guid.NewGuid().ToString();
                user.TokenExpired = DateTime.Now.AddHours(1);
                user.VerifyType = "ForgotPassword";

                _context.GetMPSContext().Users.Update(user);
                _context.GetMPSContext().SaveChanges();

                SendForgotPassword(user);

                ViewBag.Color = "text-success";
                ViewBag.message = "Confirmation email sent successfully. Please check your email.";
            }
            else
            {
                ViewBag.Color = "text-danger";
                ViewBag.message = "Please enter valid email address.";
            }

            return View();
        }

        [AllowAnonymous]
        public void SendForgotPassword(User user)
        {
            DateTime timeSync = DateTime.Now;
            
            StringBuilder emailHeaderTable = new StringBuilder();
            StringBuilder emailBodyTable = new StringBuilder();

            //Title
            emailHeaderTable.Append("<html><head><style>");
            emailHeaderTable.Append("body { font: 14pt Calibri; border-collapse: collapse;}");
            emailHeaderTable.Append("table { font: 14pt Calibri; border-collapse: collapse;}");
            emailHeaderTable.Append("th, td { padding: 8px; text-align: left; border: 1px solid #ddd; }");
            emailHeaderTable.Append("tr:nth-child(even) { background-color: #f2f2f2; }");
            emailHeaderTable.Append("tr:hover { background-color:#f5f5f5;}");
            emailHeaderTable.Append(".custButton a{ background-color: white; border: 2px solid #008CBA; color: black; ");
            emailHeaderTable.Append("padding: 16px 32px; text-align: center; text-decoration: none; display: inline-block; ");
            emailHeaderTable.Append("font-size: 16px; margin: 4px 2px; transition-duration: 0.4s; cursor: pointer;}");
            emailHeaderTable.Append(".custButton a:hover { background-color: #008CBA; color: white; }");
            emailHeaderTable.Append("</style></head>");

            emailBodyTable.Append("<body>");
            emailBodyTable.Append($"<p><b>Hi {user.LastName + " " + user.FirstName},</b></p><br/><br/><br/>");

            emailBodyTable.Append("<table style='width:1200px;' cellspacing='0' cellpadding='0'>");

            emailBodyTable.Append("<tr>");
            emailBodyTable.Append("<td><b>Reset Password</b></td>");
            emailBodyTable.Append("</tr>");


            emailBodyTable.Append("<tr>");
            emailBodyTable.Append("<td colspan='3'>" +
                "We have received a request to reset the password for your\r\naccount on Production System. In case you have not made any such request, please\r\ndisregard this email.\r\n" +
                "We take security and confidentiality seriously. We recommend\r\nthat you act promptly to ensure that your account remains secure.\r\n" +
                "Thank you for your cooperation and understanding in this\r\nmatter." + 
                "</td>");
            emailBodyTable.Append("</tr>");

            emailBodyTable.Append("<tr>");
            emailBodyTable.Append("<td colspan='3'></td>");
            emailBodyTable.Append("</tr>");
            
            string resetLink = $"https://mps.mercorella.au/Accounts/AddNewPassword?email={user.Email}&token={user.Token}";

            emailBodyTable.Append("<tr>");
            emailBodyTable.Append($"<td colspan='3'><a href='{resetLink}'>Reset Password</a></td>");
            emailBodyTable.Append("</tr>");

            emailBodyTable.Append("</table>");

            emailBodyTable.Append("<br/><br/><br/>");

            emailBodyTable.Append("</body></html>");

            emailBodyTable = emailHeaderTable.Append(emailBodyTable.ToString());

            string emails = String.IsNullOrEmpty(user.Email) ? "" : user.Email;
            
            string from = "admin@myproductionsystem.au";

            string subject = "Reset Password";

            string emailMessage = emailBodyTable.ToString();

           // _emailService.ExecuteByHTML(from, emails, "None", subject, emailMessage);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AddNewPassword(string email, string token)
        {
            var user = _context.GetMPSContext().Users.FirstOrDefault(u => u.Email == email);

            if (user != null && user.TokenExpired >= DateTime.Now && user.Token == token)
            {
                ViewBag.loginId = user.LoginId;

                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult AddNewPassword(string LoginId, string NewPassword, string ConfirmPassword)
        {
            ViewBag.loginId = LoginId;

            var user = _context.GetMPSContext().Users.FirstOrDefault(u => u.LoginId == LoginId);
            
            if (NewPassword != ConfirmPassword)
            {
               ViewBag.Color = "text-danger";
               ViewBag.message = "Confirm password doesn't match.";
            }
            else if (user != null)
            {
                user.Password = PasswordHelper.GetPasswordHash(NewPassword);
                
                _context.GetMPSContext().Users.Update(user);
                _context.GetMPSContext().SaveChanges();

                return RedirectToAction("Login", "Accounts");
            }

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            // handle different codes or just return the default error view
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorCode = code });
        }
    }
}
