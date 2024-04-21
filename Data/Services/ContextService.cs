using CarrotSystem.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;

namespace CarrotSystem.Services
{
    public interface IContextService
    {
        POContext GetPOContext();
        MPSContext GetMPSContext();

        DateTime GetDateByNow(int interval);
        DateTime GetMondayByNow();
        DateTime GetMondayByTime(DateTime inDate);
        DateTime GetSundayByTime(DateTime inDate);
    }

    public class ContextService: IContextService
    {
          private POContext _poContext = new POContext();
    //    private MPSContext _mpsContext = new MPSContext();
        // changes by SHABEER
        private readonly DbContextOptions<MPSContext> _mpsContextOptions;

        public ContextService(DbContextOptions<MPSContext> mpsContextOptions)
        {
            _mpsContextOptions = mpsContextOptions;
        }

        public MPSContext GetMPSContext()
        {
            return new MPSContext(_mpsContextOptions);
        }





        //
        //public MPSContext GetMPSContext()
        //{
        //    return _mpsContext;
        //}

        public POContext GetPOContext()
        {
            return _poContext;
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

        public DateTime GetMondayByNow()
        {
            DateTime returnDate = DateTime.Now;

            if (returnDate.DayOfWeek == DayOfWeek.Monday)
            {}
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


    }
}
