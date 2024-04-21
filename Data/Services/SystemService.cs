using CarrotSystem.Models.MPS;
using System.Globalization;

namespace CarrotSystem.Services
{
    public interface ISystemService
    {
        string GetFullNameByLoginID(string LoginID);

        DateTime GetDateByNow(int interval);
        DateTime GetMondayByNow();
        DateTime GetMondayByTime(DateTime inDate);
        DateTime GetSundayByTime(DateTime inDate);
        DateTime GetSaturdayByTime(DateTime inDate);

        DateTime GetFinanceDateFrom();
        DateTime GetFinanceDateTo();

        string GetFullNameByLoginId(string loginId);
        User GetSystemUserByLoginId(string loginId);

        List<string> GetSupplierListByUserName(string userName);
        List<string> GetPaymentMethodListByUserName(string userName);
    }

    public class SystemService : ISystemService
    {
        private IContextService _context;
        private readonly ILogger<SystemService> _logger;
        public string userName ="";
        public string loginId = "";

        public SystemService(ILogger<SystemService> logger, IContextService context)
        {
            _logger = logger;
            _context = context;
        }

        public SystemService()
        {
        }

        public string GetFullNameByLoginID(string loginId)
        {
           // var user = _context.GetMPSContext().Users.Where(w => w.LoginId.Equals(loginId)).First();
            var FirstName = "Shabir";
            var LastName = "Hussain";
             return FirstName + " " + LastName;
        }

        public DateTime GetFinanceDateFrom()
        {
            DateTime returnDate = DateTime.Now;

            if(returnDate.Month < 7)
            {
                returnDate = DateTime.ParseExact("01/07/" + returnDate.AddYears(-1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            else if (returnDate.Month > 6)
            {
                returnDate = DateTime.ParseExact("01/07/" + returnDate.Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return returnDate;
        }

        public DateTime GetFinanceDateTo()
        {
            DateTime returnDate = DateTime.Now;

            if (returnDate.Month < 7)
            {
                returnDate = DateTime.ParseExact("30/06/" + returnDate.Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            else if (returnDate.Month > 6)
            {
                returnDate = DateTime.ParseExact("30/06/" + returnDate.AddYears(1).Year, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return returnDate;
        }

        // Util Method

        public DateTime GetDateByNow(int interval)
        {
            DateTime returnDate = DateTime.Now.Date;

            if (interval < 0)
            {
                if (returnDate.DayOfWeek == DayOfWeek.Monday)
                {
                    returnDate = returnDate.Date.AddDays(-2);
                }

                returnDate = returnDate.Date.AddDays(interval);
            }

            return returnDate.Date;
        }

        public DateTime GetSaturdayByTime(DateTime inDate)
        {
            DateTime returnDate = inDate;

            if (returnDate.DayOfWeek == DayOfWeek.Monday)
            {
                returnDate = returnDate.AddDays(+5);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                returnDate = returnDate.AddDays(+4);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Wednesday)
            {
                returnDate = returnDate.AddDays(+3);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Thursday)
            {
                returnDate = returnDate.AddDays(+2);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Friday)
            {
                returnDate = returnDate.AddDays(+1);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Saturday)
            {

            }
            else if (returnDate.DayOfWeek == DayOfWeek.Sunday)
            {
                returnDate = returnDate.AddDays(+6);
            }

            return returnDate.Date;
        }

        public DateTime GetMondayByNow()
        {
            DateTime returnDate = DateTime.Now;

            if (returnDate.DayOfWeek == DayOfWeek.Monday)
            {

            }
            else if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                returnDate = returnDate.AddDays(-1);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Wednesday)
            {
                returnDate = returnDate.AddDays(-2);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Thursday)
            {
                returnDate = returnDate.AddDays(-3);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Friday)
            {
                returnDate = returnDate.AddDays(-4);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Saturday)
            {
                returnDate = returnDate.AddDays(-5);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Sunday)
            {
                returnDate = returnDate.AddDays(-6);
            }

            return returnDate.Date;
        }

        public DateTime GetMondayByTime(DateTime inDate)
        {
            DateTime returnDate = inDate;

            if (returnDate.DayOfWeek == DayOfWeek.Monday)
            {

            }
            else if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                returnDate = returnDate.AddDays(-1);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Wednesday)
            {
                returnDate = returnDate.AddDays(-2);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Thursday)
            {
                returnDate = returnDate.AddDays(-3);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Friday)
            {
                returnDate = returnDate.AddDays(-4);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Saturday)
            {
                returnDate = returnDate.AddDays(-5);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Sunday)
            {
                returnDate = returnDate.AddDays(-6);
            }

            return returnDate.Date;
        }

        public DateTime GetSundayByTime(DateTime inDate)
        {
            DateTime returnDate = inDate;

            if (returnDate.DayOfWeek == DayOfWeek.Monday)
            {
                returnDate = returnDate.AddDays(+6);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                returnDate = returnDate.AddDays(+5);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Wednesday)
            {
                returnDate = returnDate.AddDays(+4);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Thursday)
            {
                returnDate = returnDate.AddDays(+3);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Friday)
            {
                returnDate = returnDate.AddDays(+2);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Saturday)
            {
                returnDate = returnDate.AddDays(+1);
            }
            else if (returnDate.DayOfWeek == DayOfWeek.Sunday)
            {
            }

            return returnDate.Date;
        }

        public string GetFullNameByLoginId(string loginId)
        {
            if (_context.GetMPSContext().Users.Any(x => x.LoginId.Equals(loginId)))
            {
                var user = _context.GetMPSContext().Users.Where(x => x.LoginId.Equals(loginId)).First();

                return user.FirstName + " " + user.LastName;
            }
            else
            {
                return "None";
            }
        }

        public User GetSystemUserByLoginId(string loginId)
        {
            if (_context.GetMPSContext().Users.Any(x => x.LoginId.Equals(loginId)))
            {
                var user = _context.GetMPSContext().Users.Where(x => x.LoginId.Equals(loginId)).First();

                return user;
            }
            else
            {
                return new User();
            }
        }

        public List<string> GetSupplierListByUserName(string userName)
        {
            List<string> supplierList = new List<string>();

            if (_context.GetPOContext().Poforms.Any(x => (!string.IsNullOrEmpty(x.SentBy)) && x.SentBy.Equals(userName)))
            {
                supplierList.AddRange(_context.GetPOContext().Poforms.Where(x => (!string.IsNullOrEmpty(x.SentBy)) && x.SentBy.Equals(userName) && (!string.IsNullOrEmpty(x.Supplier))).Select(s => s.Supplier).ToList());
            }

            if (_context.GetPOContext().PoformTemplates.Any(x => (!string.IsNullOrEmpty(x.IssuedBy)) && x.IssuedBy.Equals(userName)))
            {
                supplierList.AddRange(_context.GetPOContext().PoformTemplates.Where(x => (!string.IsNullOrEmpty(x.IssuedBy)) && x.IssuedBy.Equals(userName) && (!string.IsNullOrEmpty(x.Supplier))).Select(s => s.Supplier).ToList());
            }

            return supplierList.Distinct().ToList();
        }

        public List<string> GetPaymentMethodListByUserName(string userName)
        {
            List<string> paymentMethodList = new List<string>();

            if (_context.GetPOContext().Poforms.Any(x => (!string.IsNullOrEmpty(x.SentBy)) && x.SentBy.Equals(userName)))
            {
                paymentMethodList.AddRange(_context.GetPOContext().Poforms.Where(x => (!string.IsNullOrEmpty(x.SentBy)) && x.SentBy.Equals(userName) && (!string.IsNullOrEmpty(x.PaymentMethod))).Select(s => s.PaymentMethod).ToList());
            }

            if (_context.GetPOContext().PoformTemplates.Any(x => (!string.IsNullOrEmpty(x.IssuedBy)) && x.IssuedBy.Equals(userName)))
            {
                paymentMethodList.AddRange(_context.GetPOContext().PoformTemplates.Where(x => (!string.IsNullOrEmpty(x.IssuedBy)) && x.IssuedBy.Equals(userName) && (!string.IsNullOrEmpty(x.PaymentMethod))).Select(s => s.PaymentMethod).ToList());
            }

            return paymentMethodList.Distinct().ToList();
        }


        //End

    }
}
