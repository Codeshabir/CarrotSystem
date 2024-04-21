using CarrotSystem.Helpers;
using CarrotSystem.Models.PO;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Primitives;
using ServiceStack;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CarrotSystem.Controllers
{
    //[Authorize(Roles = "Employee,Supervisor,Manager,SystemAdmin")]
    public class POFController : Controller
    {
        private readonly IContextService _context;
        private readonly ISystemService _system;
        private readonly IAPIService _api;
        private readonly IEventWriter _logger;
        private readonly IEmailService _emailService;
        private readonly IGenPDFService _pdfService;

        public POFController(IEventWriter logger, IContextService context, IAPIService api, IGenPDFService pdfService, ISystemService system, IEmailService emailService)
        {
            _logger = logger;
            _context = context;
            _pdfService = pdfService;
            _api = api;
            _system = system;
            _emailService = emailService;
        }

        [Authorize(Roles = "Employee,Supervisor,Manager,SystemAdmin")]
        public IActionResult PurchaseOrderList()
        {
            ViewPOF viewModel = new ViewPOF();

            viewModel.dateFrom = _context.GetDateByNow(-15);
            viewModel.dateTo = DateTime.Now.AddDays(7).Date;

            List<Poform> poList = new List<Poform>();
            poList = _context.GetPOContext().Poforms.Where(x => x.DateIssued.Date.CompareTo(viewModel.dateFrom.Value.Date) >= 0 && x.DateIssued.Date.CompareTo(viewModel.dateTo.Value.Date) <= 0).ToList();
            viewModel.formList = poList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Employee,Supervisor,Manager,SystemAdmin")]
        public IActionResult SelectedPurchaseOrderList(ViewPOF selectedModel)
        {
            ViewPOF viewModel = new ViewPOF();

            DateTime dateFrom = DateTime.ParseExact(selectedModel.stringDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dateTo = DateTime.ParseExact(selectedModel.stringDateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            viewModel.dateFrom = dateFrom;
            viewModel.dateTo = dateTo;

            List<Poform> poList = new List<Poform>();
            poList = _context.GetPOContext().Poforms.Where(x => x.DateIssued.Date.CompareTo(viewModel.dateFrom.Value.Date) >= 0 && x.DateIssued.Date.CompareTo(viewModel.dateTo.Value.Date) <= 0).ToList();
            viewModel.formList = poList;

            return View("PurchaseOrderList", viewModel);
        }

        [Authorize(Roles = "Employee,Supervisor,Manager,SystemAdmin")]
        public IActionResult NewOrderForm(int purchaseId)
        {
            var purchase = _api.GetPurchase(purchaseId);

            if (purchase != null)
            {
                ViewPOF viewModel = new ViewPOF();

                var user = _system.GetSystemUserByLoginId(HttpContext.User.Identity.Name);
                viewModel.user = user;
                viewModel.userFullName = user.FirstName + " " + user.LastName;
                viewModel.purchase = purchase;
                viewModel.supplierName = purchase.Company;
                viewModel.purchaseItems = _api.GetPurchaseItemList(purchase.InvoiceId, purchase.Company);
                viewModel.purchaseTotal = _api.CalcPurchaseTotal(viewModel.purchaseItems);
                viewModel.fileList = new List<PoformFile>();

                return View("PurchaseOrderForm", viewModel);
            }
            else
            {
                return RedirectToAction("PurchaseList", "Purchases");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employee,Supervisor,Manager,SystemAdmin")]
        public IActionResult DeleteFile(int aId)
        {
            if (_context.GetPOContext().PoformFiles.Any(x => x.Aid.Equals(aId)))
            {
                var attFile = _context.GetPOContext().PoformFiles.Where(x => x.Aid.Equals(aId)).First();

                UploadService.DeleteFile(attFile.AttachedFileAddress);

                _context.GetPOContext().PoformFiles.Remove(attFile);
                _context.GetPOContext().SaveChanges();

                return new JsonResult("Success");
            }
            else
            {
                return new JsonResult("Failed");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employee,Supervisor,Manager,SystemAdmin")]
        public IActionResult UpdateOrderForm(ViewPOF inputModel)
        {
            var purchase = _api.GetPurchase(inputModel.purchaseId);
            var user = _system.GetSystemUserByLoginId(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;

            Poform poForm = new Poform();

            if (inputModel.formStatus == "New")
            {
                var latest = 1;

                if (_context.GetPOContext().Poforms.Any(x => x.SentBy.Equals(user.LoginId) && x.Code == purchase.InvoiceId.ToString()))
                {
                    var latestPO = _context.GetPOContext().Poforms.Where(x => x.SentBy.Equals(user.LoginId) && x.Code == purchase.InvoiceId.ToString()).OrderByDescending(x => x.FormId).First().Ponumber;

                    poForm.Ponumber = user.EmployeeCode + (int.Parse(latestPO.Substring(2)) + 1).ToString().PadLeft(5, '0');
                }
                else
                {
                    latest++;

                    poForm.Ponumber = user.EmployeeCode + latest.ToString().PadLeft(5, '0');
                }

                poForm.SentBy = inputModel.poForm.SentBy;
                poForm.SentOn = DateTime.ParseExact(inputModel.stringDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                poForm.RequiredBy = inputModel.poForm.RequiredBy;
                poForm.Supplier = inputModel.poForm.Supplier;
                poForm.Price = inputModel.poForm.Price;
                poForm.Code = purchase.InvoiceId.ToString();
                poForm.Description = inputModel.poForm.Description;

                poForm.IssuedBy = user.LoginId;
                poForm.DateIssued = timeSync;
                poForm.Status = "Saved";

                _context.GetPOContext().Poforms.Add(poForm);
                if(_context.GetPOContext().SaveChanges() > 0)
                {
                    poForm = _context.GetPOContext().Poforms.FirstOrDefault(x => x.DateIssued == timeSync);
                }

                if (inputModel.attachFile != null)
                {
                    var fileSetId = poForm.FormId;

                    PoformFile newFile = new PoformFile();

                    _logger.WriteTestLog("File :" + inputModel.attachFile.FileName);

                    newFile.FileSetId = fileSetId;
                    newFile.FormId = fileSetId;
                    newFile.AttachedFileAddress = UploadService.UploadFile(inputModel.attachFile, "Invoice");
                    newFile.AttachedFileName = inputModel.attachFile.FileName;

                    _context.GetPOContext().PoformFiles.Add(newFile);
                    _context.GetPOContext().SaveChanges();

                    poForm.FileSetId = fileSetId;
                    _context.GetPOContext().Poforms.Update(poForm);
                    _context.GetPOContext().SaveChanges();
                }

                PoformLog poFormLog = new PoformLog();

                poFormLog.PoformId = poForm.FormId;
                poFormLog.DateCreated = timeSync;
                poFormLog.TimeLabelBgColor = "bg-green";
                poFormLog.TimeLabelHeader = "Added by " + poForm.SentBy;

                _context.GetPOContext().PoformLogs.Add(poFormLog);
                _context.GetPOContext().SaveChanges();

                if (inputModel.formAction.Equals("Send"))
                {
                    SendRequestEmail(user.LoginId, poForm.FormId, false);
                }

                ViewPOF viewModel = new ViewPOF();

                viewModel.user = user;
                viewModel.userFullName = user.FirstName + " " + user.LastName;

                viewModel.purchase = purchase;
                viewModel.supplierName = purchase.Company;
                viewModel.purchaseItems = _api.GetPurchaseItemList(purchase.InvoiceId, purchase.Company);
                viewModel.purchaseTotal = _api.CalcPurchaseTotal(viewModel.purchaseItems);

                var fileList = new List<PoformFile>();
                fileList = _context.GetPOContext().PoformFiles.Where(x => x.FileSetId == poForm.FileSetId).ToList();

                viewModel.fileList = fileList;

                return View("PurchaseOrderForm", viewModel);
            }
            else
            {
                if (_context.GetPOContext().Poforms.Any(x => x.FormId.Equals(inputModel.savedForm.FormId)))
                {
                    poForm = _context.GetPOContext().Poforms.Where(x => x.FormId.Equals(inputModel.savedForm.FormId)).First();

                    var modifiedLog = "";

                    if (!string.IsNullOrEmpty(inputModel.savedForm.SentBy))
                    {
                        poForm.SentBy = inputModel.savedForm.SentBy;
                        poForm.SentOn = DateTime.ParseExact(inputModel.stringDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }

                    var latest = 1;

                    if (_context.GetPOContext().Poforms.Any(x => x.SentBy.Equals(user.LoginId) && x.Status.Equals("Sent")))
                    {
                        var latestPO = _context.GetPOContext().Poforms.Where(x => x.SentBy.Equals(user.LoginId) && x.Status.Equals("Sent")).OrderByDescending(x => x.FormId).First().Ponumber;

                        poForm.Ponumber = user.EmployeeCode + (int.Parse(latestPO.Substring(2)) + 1).ToString().PadLeft(5, '0');
                    }
                    else
                    {
                        latest++;

                        poForm.Ponumber = user.EmployeeCode + latest.ToString().PadLeft(5, '0');
                    }

                    if (!string.IsNullOrEmpty(inputModel.savedForm.RequiredBy))
                    {
                        if (!poForm.RequiredBy.Equals(inputModel.savedForm.RequiredBy))
                        {
                            modifiedLog = modifiedLog + "Required By : '" + poForm.RequiredBy + "' -> '" + inputModel.savedForm.RequiredBy + "'";
                        }

                        poForm.RequiredBy = inputModel.savedForm.RequiredBy;
                    }

                    if (!string.IsNullOrEmpty(inputModel.savedForm.Supplier))
                    {
                        if (!poForm.Supplier.Equals(inputModel.savedForm.Supplier))
                        {
                            modifiedLog = modifiedLog + "Supplier : '" + poForm.Supplier + "' -> '" + inputModel.savedForm.Supplier + "'";
                        }

                        poForm.Supplier = inputModel.savedForm.Supplier;
                    }

                    if (!string.IsNullOrEmpty(inputModel.savedForm.Description))
                    {
                        if (!poForm.Description.Equals(inputModel.savedForm.Description))
                        {
                            modifiedLog = modifiedLog + "Description : '" + poForm.Description + "' -> '" + inputModel.savedForm.Description + "'";
                        }

                        poForm.Description = inputModel.savedForm.Description;
                    }

                    if (inputModel.savedForm.Price > 0)
                    {
                        if (poForm.Price != inputModel.savedForm.Price)
                        {
                            modifiedLog = modifiedLog + "Price : $" + poForm.Price + " -> $" + inputModel.savedForm.Price;
                        }

                        poForm.Price = inputModel.savedForm.Price;
                    }

                    if (!string.IsNullOrEmpty(inputModel.savedForm.Code))
                    {
                        if (!poForm.Code.Equals(inputModel.savedForm.Code))
                        {
                            modifiedLog = modifiedLog + "Purchase Id : '" + poForm.Code + "' -> '" + inputModel.savedForm.Code + "'";
                        }

                        poForm.Code = inputModel.savedForm.Code;
                    }

                    poForm.IssuedBy = user.LoginId;
                    poForm.DateIssued = timeSync;

                    poForm.Status = "Saved";

                    _context.GetPOContext().Poforms.Update(poForm);
                    if (_context.GetPOContext().SaveChanges() > 0)
                    {
                        poForm = _context.GetPOContext().Poforms.Where(x => x.DateIssued.Equals(timeSync)).First();
                    }

                    if (inputModel.attachFile != null)
                    {
                        _logger.WriteTestLog("Added it");

                        var fileSetId = poForm.FormId;

                        PoformFile newFile = new PoformFile();

                        newFile.FileSetId = fileSetId;
                        newFile.FormId = fileSetId;
                        newFile.AttachedFileAddress = UploadService.UploadFile(inputModel.attachFile, "Invoice");
                        newFile.AttachedFileName = inputModel.attachFile.FileName;

                        _context.GetPOContext().PoformFiles.Add(newFile);
                        _context.GetPOContext().SaveChanges();

                        poForm.FileSetId = fileSetId;
                        _context.GetPOContext().Poforms.Update(poForm);
                        _context.GetPOContext().SaveChanges();
                    }

                    if (!string.IsNullOrEmpty(modifiedLog))
                    {
                        PoformLog poFormLog = new PoformLog();

                        poFormLog.PoformId = poForm.FormId;
                        poFormLog.DateCreated = timeSync;
                        poFormLog.TimeLabelBgColor = "bg-green";
                        poFormLog.TimeLabelHeader = "Updated by " + poForm.SentBy + ", " + modifiedLog;

                        _context.GetPOContext().PoformLogs.Add(poFormLog);
                        _context.GetPOContext().SaveChanges();
                    }

                    if (inputModel.formStatus.Equals("UpdateSend"))
                    {
                        SendRequestEmail(user.LoginId, poForm.FormId, false);
                    }
                }

                return RedirectToAction("ViewOrder", new { id = poForm.FormId });
            }
        }

        [AllowAnonymous]
        public IActionResult CantFound()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ApproveForm(ViewPOF inputModel)
        {
            if (_context.GetPOContext().Poforms.Any(w => w.FormId.Equals(inputModel.poForm.FormId)))
            {
                var poform = _context.GetPOContext().Poforms.Where(w => w.FormId.Equals(inputModel.poForm.FormId)).First();
                var purchase = _api.GetPurchase(inputModel.purchaseId);
                var userName = inputModel.userName;

                if(inputModel.appStatus.Equals("Approve"))
                {
                    PoformLog poFormLog = new PoformLog();

                    poFormLog.PoformId = poform.FormId;
                    poFormLog.DateCreated = DateTime.Now;
                    poFormLog.TimeLabelBgColor = "bg-red";
                    poFormLog.TimeLabelHeader = "Approved by " + userName;

                    _context.GetPOContext().PoformLogs.Add(poFormLog);
                    _context.GetPOContext().SaveChanges();

                    poform.DateVerified = DateTime.Now;
                    poform.VerificationBy = userName;

                    _context.GetPOContext().Poforms.Update(poform);
                    _context.GetPOContext().SaveChanges();

                    var mpsPurchase = _context.GetMPSContext().Purchase.FirstOrDefault(x => x.InvoiceId == inputModel.purchaseId);

                    mpsPurchase.Status = "Invoice";
                    mpsPurchase.UpdatedOn = DateTime.Now;
                    mpsPurchase.UpdatedBy = userName;

                    _context.GetMPSContext().Purchase.Update(mpsPurchase);
                    _context.GetMPSContext().SaveChanges();

                    if (poform.Status.Equals("Approved"))
                    {
                        SendApproveEmail("Approved", "", poform.SentBy, poform.FormId, true);
                    }
                    else
                    {
                        SendApproveEmail("Approved", "", poform.SentBy, poform.FormId, false);
                    }
                }
                else if (inputModel.appStatus.Equals("Decline"))
                {
                    var comment = inputModel.appComment;

                    PoformLog poFormLog = new PoformLog();

                    poFormLog.PoformId = poform.FormId;
                    poFormLog.DateCreated = DateTime.Now;
                    poFormLog.TimeLabelBgColor = "bg-red";
                    poFormLog.TimeLabelHeader = "Declined by " + userName + ", " + comment;

                    _context.GetPOContext().PoformLogs.Add(poFormLog);
                    _context.GetPOContext().SaveChanges();

                    if (poform.Status.Equals("Decline"))
                    {
                        SendApproveEmail("Decline", "", poform.SentBy, poform.FormId, true);
                    }
                    else
                    {
                        SendApproveEmail("Decline", "", poform.SentBy, poform.FormId, false);
                    }
                }
                else if (inputModel.appStatus.Equals("Save") || inputModel.appStatus.Equals("Send"))
                {
                    var user = _system.GetSystemUserByLoginId(HttpContext.User.Identity.Name);
                    var timeSync = DateTime.Now;

                    Poform savedForm = new Poform();

                    if (_context.GetPOContext().Poforms.Any(x => x.FormId.Equals(inputModel.poForm.FormId)))
                    {
                        savedForm = _context.GetPOContext().Poforms.Where(x => x.FormId.Equals(inputModel.poForm.FormId)).First();

                        var modifiedLog = "";

                        if (!string.IsNullOrEmpty(inputModel.poForm.SentBy))
                        {
                            savedForm.SentBy = inputModel.poForm.SentBy;
                            savedForm.SentOn = DateTime.ParseExact(inputModel.stringDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        }

                        var latest = 1;

                        if (_context.GetPOContext().Poforms.Any(x => x.SentBy.Equals(user.LoginId) && x.Status.Equals("Sent")))
                        {
                            var latestPO = _context.GetPOContext().Poforms.Where(x => x.SentBy.Equals(user.LoginId) && x.Status.Equals("Sent")).OrderByDescending(x => x.FormId).First().Ponumber;

                            savedForm.Ponumber = user.EmployeeCode + (int.Parse(latestPO.Substring(2)) + 1).ToString().PadLeft(5,'0');
                        }
                        else
                        {
                            latest++;

                            savedForm.Ponumber = user.EmployeeCode + latest.ToString().PadLeft(5, '0');
                        }

                        if (!string.IsNullOrEmpty(inputModel.poForm.RequiredBy))
                        {
                            if (!savedForm.RequiredBy.Equals(inputModel.poForm.RequiredBy))
                            {
                                modifiedLog = modifiedLog + "Required By : '" + savedForm.RequiredBy + "' -> '" + inputModel.poForm.RequiredBy + "'";
                            }

                            savedForm.RequiredBy = inputModel.poForm.RequiredBy;
                        }

                        if (!string.IsNullOrEmpty(inputModel.poForm.Supplier))
                        {
                            if (!savedForm.Supplier.Equals(inputModel.poForm.Supplier))
                            {
                                modifiedLog = modifiedLog + "Supplier : '" + savedForm.Supplier + "' -> '" + inputModel.poForm.Supplier + "'";
                            }

                            savedForm.Supplier = inputModel.poForm.Supplier;
                        }

                        if (!string.IsNullOrEmpty(inputModel.poForm.Description))
                        {
                            if (!savedForm.Description.Equals(inputModel.poForm.Description))
                            {
                                modifiedLog = modifiedLog + "Description : '" + savedForm.Description + "' -> '" + inputModel.poForm.Description + "'";
                            }

                            savedForm.Description = inputModel.poForm.Description;
                        }

                        if (inputModel.poForm.Price > 0)
                        {
                            if (savedForm.Price != inputModel.poForm.Price)
                            {
                                modifiedLog = modifiedLog + "Price : $" + savedForm.Price + " -> $" + inputModel.poForm.Price;
                            }

                            savedForm.Price = inputModel.poForm.Price;
                        }

                        if (!string.IsNullOrEmpty(inputModel.poForm.Code))
                        {
                            if (!savedForm.Code.Equals(inputModel.poForm.Code))
                            {
                                modifiedLog = modifiedLog + "Code : '" + savedForm.Code + "' -> '" + inputModel.poForm.Code + "'";
                            }

                            savedForm.Code = inputModel.poForm.Code;
                        }

                        if (!string.IsNullOrEmpty(inputModel.poForm.PaymentMethod))
                        {
                            if (!savedForm.PaymentMethod.Equals(inputModel.poForm.PaymentMethod))
                            {
                                modifiedLog = modifiedLog + "PaymentMethod : '" + savedForm.PaymentMethod + "' -> '" + inputModel.poForm.PaymentMethod + "'";
                            }

                            savedForm.PaymentMethod = inputModel.poForm.PaymentMethod;
                        }

                        savedForm.IssuedBy = user.LoginId;
                        savedForm.DateIssued = timeSync;

                        //savedForm.Status = "Saved";

                        _context.GetPOContext().Poforms.Update(savedForm);
                        if (_context.GetPOContext().SaveChanges() > 0)
                        {
                            savedForm = _context.GetPOContext().Poforms.Where(x => x.DateIssued.Equals(timeSync)).First();
                        }

                        if (inputModel.attachFile != null)
                        {
                            var fileSetId = savedForm.FormId;

                            PoformFile newFile = new PoformFile();

                            newFile.FileSetId = fileSetId;
                            newFile.FormId = fileSetId;
                            newFile.AttachedFileAddress = UploadService.UploadFile(inputModel.attachFile, "Invoice");
                            newFile.AttachedFileName = inputModel.attachFile.FileName;

                            _context.GetPOContext().PoformFiles.Add(newFile);
                            _context.GetPOContext().SaveChanges();

                            savedForm.FileSetId = fileSetId;
                            _context.GetPOContext().Poforms.Update(savedForm);
                            _context.GetPOContext().SaveChanges();
                        }

                        if(!string.IsNullOrEmpty(modifiedLog))
                        {
                            PoformLog poFormLog = new PoformLog();

                            poFormLog.PoformId = savedForm.FormId;
                            poFormLog.DateCreated = timeSync;
                            poFormLog.TimeLabelBgColor = "bg-green";
                            poFormLog.TimeLabelHeader = "Updated by " + savedForm.SentBy + ", " + modifiedLog;

                            _context.GetPOContext().PoformLogs.Add(poFormLog);
                            _context.GetPOContext().SaveChanges();
                        }

                        if (inputModel.appStatus.Equals("Send"))
                        {
                            if(savedForm.Status.Equals("Sent") || savedForm.Status.Equals("Approved"))
                            {
                                SendRequestEmail(user.LoginId, savedForm.FormId, true);
                            }
                            else
                            {
                                SendRequestEmail(user.LoginId, savedForm.FormId, false);
                            }
                        }
                    }
                }

                if(inputModel.appBackPage.Equals("ViewForm"))
                {
                    return RedirectToAction("ViewForm", new { formId = poform.FormId, viewBy = userName });
                }
                else
                {
                    return RedirectToAction("ViewOrder", new { id = poform.FormId });
                }

            }
            else
            {
                return RedirectToAction("CantFound");
            }
        }

        [Authorize(Roles = "Employee,Supervisor,Manager,SystemAdmin")]
        public IActionResult ViewOrder(string id)
        {
            var formId = int.Parse(id);
            var user = _system.GetSystemUserByLoginId(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;

            ViewPOF viewModel = new ViewPOF();
            if (_context.GetPOContext().Poforms.Any(x => x.FormId.Equals(formId)))
            {
                var poform = _context.GetPOContext().Poforms.Where(w => w.FormId.Equals(formId)).First();

                var purchase = _api.GetPurchase(int.Parse(poform.Code));

                viewModel.poForm = poform;
                viewModel.savedForm = poform;
                viewModel.user = user;
                viewModel.userFullName = user.FirstName + " " + user.LastName;
                viewModel.purchase = purchase;
                viewModel.supplierName = purchase.Company;
                viewModel.purchaseItems = _api.GetPurchaseItemList(purchase.InvoiceId, purchase.Company);
                viewModel.purchaseTotal = _api.CalcPurchaseTotal(viewModel.purchaseItems);

                List<PoformLog> formLogList = new List<PoformLog>();

                formLogList = _context.GetPOContext().PoformLogs.Where(x => x.PoformId.Equals(formId)).ToList();

                List<PoformFile> fileList = new List<PoformFile>();

                if (_context.GetPOContext().PoformFiles.Any(x => (x.FormId > 0 && x.FormId.Equals(poform.FormId))))
                {
                    fileList = _context.GetPOContext().PoformFiles.Where(x => x.FormId > 0 && x.FormId.Equals(poform.FormId)).ToList();
                }

                viewModel.fileList = fileList;
                viewModel.formLogList = formLogList;

                return View(viewModel);
            }
            else
            {
                return RedirectToAction("PurchaseOrderList");
            }
        }

        [AllowAnonymous]
        public IActionResult ViewForm(int formId, string viewBy)
        {
            var timeSync = DateTime.Now;
            ViewPOF viewModel = new ViewPOF();
            var user = _system.GetSystemUserByLoginId(HttpContext.User.Identity.Name);

            if (_context.GetPOContext().Poforms.Any(x => x.FormId.Equals(formId)))
            {
                var poform = _context.GetPOContext().Poforms.Where(w => w.FormId.Equals(formId)).First();

                var purchase = _api.GetPurchase(int.Parse(poform.Code));

                viewModel.user = user;
                viewModel.userFullName = user.FirstName + " " + user.LastName;
                viewModel.poForm = poform;
                viewModel.purchase = purchase;
                viewModel.supplierName = purchase.Company;
                viewModel.purchaseItems = _api.GetPurchaseItemList(purchase.InvoiceId, purchase.Company);
                viewModel.purchaseTotal = _api.CalcPurchaseTotal(viewModel.purchaseItems);

                List<PoformLog> formLogList = new List<PoformLog>();

                formLogList = _context.GetPOContext().PoformLogs.Where(x => x.PoformId.Equals(formId)).ToList();

                List<PoformFile> fileList = new List<PoformFile>();

                if (_context.GetPOContext().PoformFiles.Any(x => (x.FormId > 0 && x.FormId.Equals(poform.FormId))))
                {
                    fileList = _context.GetPOContext().PoformFiles.Where(x => x.FormId > 0 && x.FormId.Equals(poform.FormId)).ToList();
                }

                viewModel.fileList = fileList;
                viewModel.formLogList = formLogList;
                viewModel.userName = viewBy;

                return View(viewModel);
            }
            else
            {
                return RedirectToAction("CantFound");
            }
        }

        [HttpPost]
        public IActionResult SendRequestEmail(string sendTo, int requestId, bool isUpdate)
        {
            DateTime timeSync = DateTime.Now;
            Poform poForm = new Poform();

            var user = _system.GetSystemUserByLoginId(sendTo);

            if (_context.GetPOContext().Poforms.Any(x => x.FormId.Equals(requestId)))
            {
                poForm = _context.GetPOContext().Poforms.Where(x => x.FormId.Equals(requestId)).First();

                var purchase = _api.GetPurchase(int.Parse(poForm.Code));
                var purchaseItems = _api.GetPurchaseItemList(purchase.InvoiceId, purchase.Company);
                var purchaseTotal = _api.CalcPurchaseTotal(purchaseItems);

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
                emailBodyTable.Append("<b>Purchase Order Form</b><br/><br/><br/>");
                emailBodyTable.Append("<b>Sent by</b> : " + poForm.IssuedBy + "<br/>");
                emailBodyTable.Append("<b>Sent on</b> : " + poForm.DateIssued.ToString("dd/MM/yyyy") + "<br/><br/>");

                emailBodyTable.Append("<table style='width:1200px;' cellspacing='0' cellpadding='0'>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Purchase Order Number</b></td>");
                emailBodyTable.Append("<td>");
                if (!string.IsNullOrEmpty(poForm.Ponumber))
                {
                    emailBodyTable.Append(poForm.Ponumber);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("<td><b>Order Required By</b></td>");
                emailBodyTable.Append("<td>");
                if (!string.IsNullOrEmpty(poForm.RequiredBy))
                {
                    emailBodyTable.Append(poForm.RequiredBy);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Supplier</b></td>");
                emailBodyTable.Append("<td colspan='2'>");
                if (!string.IsNullOrEmpty(poForm.Supplier))
                {
                    emailBodyTable.Append(poForm.Supplier);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                    emailBodyTable.Append("<td colspan='3'><b>Purchase Items</b></td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                    emailBodyTable.Append("<td><b>Item Cpde</b></td>");
                    emailBodyTable.Append("<td><b>Qty</b></td>");
                    emailBodyTable.Append("<td><b>Price</b></td>");
                emailBodyTable.Append("</tr>");

                foreach (var purchaseItem in purchaseItems)
                {
                    emailBodyTable.Append("<tr>");
                    if(string.IsNullOrEmpty(purchaseItem.ProductCode))
                    {
                        emailBodyTable.Append($"<td><b>{purchaseItem.ProductCode}</b></td>");
                    }
                    else
                    {
                        emailBodyTable.Append($"<td><b>{purchaseItem.CompanyCode}</b></td>");
                    }
                        emailBodyTable.Append("<td>");
                            emailBodyTable.Append(purchaseItem.InvoicedQty);
                        emailBodyTable.Append("</td>");

                        emailBodyTable.Append("<td>");
                            emailBodyTable.Append(purchaseItem.Price);
                        emailBodyTable.Append("</td>");

                    emailBodyTable.Append("</tr>");
                }

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Description of Order</b></td>");
                emailBodyTable.Append("<td colspan='3'>");
                if (!string.IsNullOrEmpty(poForm.Description))
                {
                    emailBodyTable.Append(poForm.Description);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Price</b></td>");
                emailBodyTable.Append("<td>");
                emailBodyTable.Append(purchaseTotal.InvoiceTotal.ToString("$ #,##0.##"));
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("<td><b>GST</b></td>");
                emailBodyTable.Append("<td>");
                emailBodyTable.Append(purchaseTotal.TaxTotal.ToString("$ #,##0.##"));
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("</table>");

                emailBodyTable.Append("<br/><br/><br/>");

                emailBodyTable.Append("<table style='width:200px;' cellspacing='0' cellpadding='0' style='border:0;'>");
                emailBodyTable.Append("<tr><td><table cellspacing='0' cellpadding='0'>");
                emailBodyTable.Append("<tr><td style='border-radius: 2px;' bgcolor='#ED2939'>");
                emailBodyTable.Append("<a href='https://mps.mercorella.au/POF/ViewForm?formId=" + poForm.FormId + "&sendBy=" + user.LoginId + "' ");
                emailBodyTable.Append("style='padding: 8px 12px; border: 1px solid #ED2939;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;text-decoration: none;font-weight:bold;display: inline-block;'>");
                emailBodyTable.Append("Check & Approve</a>");
                emailBodyTable.Append("</td></tr></table></td></tr></table>");

                emailBodyTable.Append("</body></html>");

                emailBodyTable = emailHeaderTable.Append(emailBodyTable.ToString());

                var emailGroup = _context.GetMPSContext().EmailGroup.Where(x => x.GroupName == "Purchase Order").FirstOrDefault();

                var emails = "";

                if(emailGroup == null)
                {
                    emails = String.IsNullOrEmpty(user.Email) ? "" : user.Email;
                }
                else
                {
                    emails = emailGroup.EmailAddress;
                }

                string from = "admin@myproductionsystem.au";

                string subject = "";

                if (isUpdate)
                {
                    subject = "[Updated] Purchase Order Request Form " + poForm.DateIssued.ToString("dd/MM/yyyy");
                }
                else
                {
                    subject = "Purchase Order Request Form " + poForm.DateIssued.ToString("dd/MM/yyyy");
                }

                string emailMessage = emailBodyTable.ToString();

                _emailService.ExecuteByHTML(from, emails, "None", subject, emailMessage);

                poForm.Status = "Sent";

                _logger.WriteTestLog("Email to emails, " + poForm.Status);

                _context.GetPOContext().Poforms.Update(poForm);
                _context.GetPOContext().SaveChanges();

                PoformLog poFormLog = new PoformLog();

                poFormLog.PoformId = poForm.FormId;
                poFormLog.DateCreated = timeSync;
                poFormLog.TimeLabelBgColor = "bg-green";
                poFormLog.TimeLabelHeader = "Sent Email by " + poForm.SentBy;
                poFormLog.TimeLabelBody = "Sent Email to " + sendTo;

                _context.GetPOContext().PoformLogs.Add(poFormLog);
                _context.GetPOContext().SaveChanges();

                return new JsonResult("Successed");
            }
            else
            {
                return new JsonResult("Failed");
            }

        }

        [HttpPost]
        public IActionResult SendApproveEmail(string appType, string appComment, string sendTo, int formId, bool isUpdate)
        {
            DateTime timeSync = DateTime.Now;
            Poform poForm = new Poform();

            var user = _system.GetSystemUserByLoginId(sendTo);
            if (_context.GetPOContext().Poforms.Any(x => x.FormId.Equals(formId)))
            {
                poForm = _context.GetPOContext().Poforms.Where(x => x.FormId.Equals(formId)).First();

                var purchase = _api.GetPurchase(int.Parse(poForm.Code));
                var purchaseItems = _api.GetPurchaseItemList(purchase.InvoiceId, purchase.Company);
                var purchaseTotal = _api.CalcPurchaseTotal(purchaseItems);

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
                emailBodyTable.Append("<b>[" + poForm.Status +"] Purchase Order Form #" + poForm.FormId + "</b><br/><br/><br/>");
                emailBodyTable.Append("<b>Sent by</b> : " + poForm.IssuedBy + "<br/>");
                emailBodyTable.Append("<b>Sent on</b> : " + poForm.DateIssued.ToString("dd/MM/yyyy") + "<br/><br/>");

                emailBodyTable.Append("<table style='width:1200px;' cellspacing='0' cellpadding='0'>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Purchase Order Number</b></td>");
                emailBodyTable.Append("<td>");
                if (!string.IsNullOrEmpty(poForm.Ponumber))
                {
                    emailBodyTable.Append(poForm.Ponumber);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("<td><b>Order Required By</b></td>");
                emailBodyTable.Append("<td>");
                if (!string.IsNullOrEmpty(poForm.RequiredBy))
                {
                    emailBodyTable.Append(poForm.RequiredBy);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Supplier</b></td>");
                emailBodyTable.Append("<td colspan='2'>");
                if (!string.IsNullOrEmpty(poForm.Supplier))
                {
                    emailBodyTable.Append(poForm.Supplier);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td colspan='3'><b>Purchase Items</b></td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Item Cpde</b></td>");
                emailBodyTable.Append("<td><b>Qty</b></td>");
                emailBodyTable.Append("<td><b>Price</b></td>");
                emailBodyTable.Append("</tr>");

                foreach (var purchaseItem in purchaseItems)
                {
                    emailBodyTable.Append("<tr>");
                    if (string.IsNullOrEmpty(purchaseItem.ProductCode))
                    {
                        emailBodyTable.Append($"<td><b>{purchaseItem.ProductCode}</b></td>");
                    }
                    else
                    {
                        emailBodyTable.Append($"<td><b>{purchaseItem.CompanyCode}</b></td>");
                    }
                    emailBodyTable.Append("<td>");
                    emailBodyTable.Append(purchaseItem.InvoicedQty);
                    emailBodyTable.Append("</td>");

                    emailBodyTable.Append("<td>");
                    emailBodyTable.Append(purchaseItem.Price);
                    emailBodyTable.Append("</td>");

                    emailBodyTable.Append("</tr>");
                }

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Description of Order</b></td>");
                emailBodyTable.Append("<td colspan='3'>");
                if (!string.IsNullOrEmpty(poForm.Description))
                {
                    emailBodyTable.Append(poForm.Description);
                }
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                emailBodyTable.Append("<tr>");
                emailBodyTable.Append("<td><b>Price</b></td>");
                emailBodyTable.Append("<td>");
                emailBodyTable.Append(purchaseTotal.InvoiceTotal.ToString("$ #,##0.##"));
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("<td><b>GST</b></td>");
                emailBodyTable.Append("<td>");
                emailBodyTable.Append(purchaseTotal.TaxTotal.ToString("$ #,##0.##"));
                emailBodyTable.Append("</td>");
                emailBodyTable.Append("</tr>");

                if (!string.IsNullOrEmpty(appComment))
                {
                    emailBodyTable.Append("<tr>");
                    emailBodyTable.Append("<td><b>Comment</b></td>");
                    emailBodyTable.Append("<td>");
                    emailBodyTable.Append(appComment);
                    emailBodyTable.Append("</td>");
                    emailBodyTable.Append("</tr>");
                }

                emailBodyTable.Append("</table>");

                emailBodyTable.Append("<br/><br/><br/>");

                emailBodyTable.Append("</body></html>");

                emailBodyTable = emailHeaderTable.Append(emailBodyTable.ToString());

                string emails = String.IsNullOrEmpty(user.Email) ? "" : user.Email;
                string from = "admin@myproductionsystem.au";

                string subject = "";

                if(appType.Equals("Approved"))
                {
                    if (isUpdate)
                    {
                        subject = "[Updated] Purchase Order Request Form Approved on" + poForm.DateVerified.Value.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        subject = "Purchase Order Request Form Approved on" + poForm.DateIssued.ToString("dd/MM/yyyy");
                    }

                    poForm.Status = "Approved";

                    PoformLog poFormLog = new PoformLog();

                    poFormLog.PoformId = poForm.FormId;
                    poFormLog.DateCreated = timeSync;
                    poFormLog.TimeLabelBgColor = "bg-green";
                    poFormLog.TimeLabelHeader = "Sent Approved Email by " + poForm.SentBy;
                    poFormLog.TimeLabelBody = "Sent Approved Email to " + sendTo;

                    _context.GetPOContext().PoformLogs.Add(poFormLog);
                    _context.GetPOContext().SaveChanges();
                }
                else
                {
                    if (isUpdate)
                    {
                        subject = "[Updated] Purchase Order Request Form Declined on" + poForm.DateVerified.Value.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        subject = "Purchase Order Request Form Declined on" + poForm.DateIssued.ToString("dd/MM/yyyy");
                    }

                    PoformLog poFormLog = new PoformLog();

                    poFormLog.PoformId = poForm.FormId;
                    poFormLog.DateCreated = timeSync;
                    poFormLog.TimeLabelBgColor = "bg-red";
                    poFormLog.TimeLabelHeader = "Sent Declined Email by " + poForm.SentBy;
                    poFormLog.TimeLabelBody = "Sent Declined Email to " + sendTo;

                    _context.GetPOContext().PoformLogs.Add(poFormLog);
                    _context.GetPOContext().SaveChanges();
                }

                string emailMessage = emailBodyTable.ToString();

                _emailService.ExecuteByHTML(from, emails, "None", subject, emailMessage);
                    
                _logger.WriteTestLog("Email to emails, " + poForm.Status);

                _context.GetPOContext().Poforms.Update(poForm);
                _context.GetPOContext().SaveChanges();

                return new JsonResult("Successed");
            }
            else
            {
                return new JsonResult("Failed");
            }

        }

        public IActionResult ShowFile(string filePath, string fileName)
        {
            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Determine the content type based on the file extension
            string contentType;
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                // Add more cases as needed for other file types
                default:
                    contentType = "application/octet-stream";
                    break;
            }

            // Read the file into a byte array
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Return the file as a stream
            //return File(fileBytes, contentType, fileName);
            return File(fileBytes, contentType);
        }



        //End
    }
}