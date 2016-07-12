using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{
    public class TimeDifference
    {
      

        public TimeDifference(int employeeID, DateTime clockIn, DateTime clockOut)
        {
            ClockIn = clockIn;
            ClockOut = clockOut;
            EmployeeID = employeeID;
        }

        public TimeDifference(TimeSlip slip)
        {
            ClockIn = slip.ClockIn;
            ClockOut = slip.ClockOut;
            EmployeeID = slip.EmployeeId;
        }

        public int EmployeeID{ get; private set; }

        public DateTime ClockIn { get; private set; }

        public DateTime ClockOut { get; private set; }
        
        private DateTime? _cacheClockIn;
        /// <summary>
        /// Adjusts the clock in to the most valid date
        /// for instance if clocked in before 8AM will adjust to 8AM
        /// or if clocked in a weekend will move to the Monday at 8AM. 
        /// Or if clocked in after 5PM will move to next workday at 8AM
        /// </summary>
        private DateTime NewAdjustedClockIn
        {
            get
            {
                if (_cacheClockIn == null)
                {
                    _cacheClockIn = CalculateClockIn();
                }
                return _cacheClockIn.Value;
            }
        }

        private DateTime? _cacheClockOut;
        private DateTime NewAdjustedClockOut
        {
            get
            {
                if (_cacheClockOut == null)
                {
                    _cacheClockOut = CalculateClockOut();
                }
                return _cacheClockOut.Value;
            }
        }

        private int WorkingDaysBetween
        {
            get
            {
                return WorkDaysBetween(NewAdjustedClockIn, NewAdjustedClockOut);
            }
        }

        
        private bool InToOutOnlyCoversWeekend
        {
            get
            {
                //If clockin/clockout isnt on a weekend return early
                if(ClockIn.DayOfWeek != DayOfWeek.Saturday && ClockIn.DayOfWeek != DayOfWeek.Sunday)
                {
                    return false;
                }
                else if (ClockOut.DayOfWeek != DayOfWeek.Saturday && ClockOut.DayOfWeek != DayOfWeek.Sunday)
                {
                    return false;
                }
                //If same day and the day of week is saturday or sunday then don't do anything
                else if(ClockIn.Date == ClockOut.Date && (ClockIn.DayOfWeek == DayOfWeek.Saturday || ClockIn.DayOfWeek == DayOfWeek.Sunday))
                {
                    return true;
                }
                //If same day and NOT Saturday or Sunday return early
                else if(ClockIn.Date == ClockOut.Date && (ClockIn.DayOfWeek != DayOfWeek.Saturday && ClockIn.DayOfWeek != DayOfWeek.Sunday))
                {
                    return false;
                }
                
                return WorkDaysBetween(ClockIn,ClockOut) == 0;
            }
        }

        private int WorkDaysBetween(DateTime start, DateTime end)
        {
            var workinDaysBetween = 0;
            var indexDate = start.Date.AddDays(1);
            while (indexDate.Date < end.Date)
            {
                if (indexDate.DayOfWeek != DayOfWeek.Sunday && indexDate.DayOfWeek != DayOfWeek.Saturday)
                {
                    workinDaysBetween++;
                }
                indexDate = indexDate.Date.AddDays(1);
            }
            return workinDaysBetween;
        }

        private DateTime CalculateClockIn()
        {
            //If the clock in is on a Saturday set it to Monday Morning at 8AM
            if (ClockIn.DayOfWeek == DayOfWeek.Saturday)
            {
                var monday = ClockIn.AddDays(2);
                return new DateTime(monday.Year, monday.Month, monday.Day, 8, 0, 0);
            }
            else if (ClockIn.DayOfWeek == DayOfWeek.Sunday)
            {
                var monday = ClockIn.AddDays(1);
                return new DateTime(monday.Year, monday.Month, monday.Day, 8, 0, 0);
            }
            else if (ClockIn.Hour < 8)
            {
                return new DateTime(ClockIn.Year, ClockIn.Month, ClockIn.Day, 8, 0, 0);
            }
            else if (ClockIn.Hour >= 17)
            {
                //if user clocked in after 5PM on a Friday we need to move it to the next Monday at 8AM
                var daysToScroll = 1;
                if (ClockIn.DayOfWeek == DayOfWeek.Friday)
                {
                    daysToScroll = 3;
                }
                var nextDay = ClockIn.AddDays(daysToScroll);
                return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 8, 0, 0);
            }
            else
            {
                return ClockIn;
            }
        }

        private DateTime CalculateClockOut()
        {
            if (ClockOut.DayOfWeek == DayOfWeek.Saturday)
            {
                var monday = ClockOut.AddDays(-1);
                return new DateTime(monday.Year, monday.Month, monday.Day, 17, 0, 0);
            }
            else if (ClockOut.DayOfWeek == DayOfWeek.Sunday)
            {
                var monday = ClockOut.AddDays(-2);
                return new DateTime(monday.Year, monday.Month, monday.Day, 17, 0, 0);
            }
            else if (ClockOut.Hour < 8)
            {
                //if user clocked out before 8AM on a monday we need to move it to Friday at 5PM
                var daysToScroll = -1;
                if (ClockOut.DayOfWeek == DayOfWeek.Monday)
                {
                    daysToScroll = -3;
                }
                var previousDay = ClockOut.AddDays(daysToScroll);
                return new DateTime(previousDay.Year, previousDay.Month, previousDay.Day, 17, 0, 0);
            }
            else if (ClockOut.Hour >= 17)
            {
                return new DateTime(ClockOut.Year, ClockOut.Month, ClockOut.Day, 17, 0, 0);
            }
            else
            {
                return ClockOut;
            }
        }

        internal HoursWorked EvaluateHoursAndMinutes()
        {
            var hoursWorked = new HoursWorked(this);
            hoursWorked.StatusCode = StatusCode.ValidTime;
            //If the range only covers weekend then end early
            if(InToOutOnlyCoversWeekend)
            {
                return hoursWorked;
            }
            //If there is some error then return early
            if (NewAdjustedClockIn > NewAdjustedClockOut)
            {
                hoursWorked.StatusCode = StatusCode.ClockInAfterClockOut;
                return hoursWorked;
            }
            //If the times are the same exit earl with zeros
            if (NewAdjustedClockIn == NewAdjustedClockOut)
            {
                hoursWorked.StatusCode = StatusCode.ClockInAndClockOutSame;
                return hoursWorked;
            }
            //We can simply calculate the time for this single day
            else if (NewAdjustedClockIn.Date == NewAdjustedClockOut.Date)
            {
                //If the date is the same and its Sunday or Saturday we can leave early.
                if (NewAdjustedClockIn.Date.DayOfWeek == DayOfWeek.Sunday || NewAdjustedClockIn.DayOfWeek == DayOfWeek.Saturday)
                {
                    return hoursWorked;
                }

                var timediff = NewAdjustedClockOut.Subtract(NewAdjustedClockIn);
                hoursWorked.Hours = timediff.Hours;
                hoursWorked.Minutes = timediff.Minutes;
                return hoursWorked;
            }
            
            //Get the time to the end of the day for the clock in event
            var endOfDay = new DateTime(NewAdjustedClockIn.Year, NewAdjustedClockIn.Month, NewAdjustedClockIn.Day, 17, 0, 0);
            TimeSpan differenceClockInDay = new TimeSpan();
            differenceClockInDay = endOfDay.Subtract(NewAdjustedClockIn);

            //Get the time for the start of day to the clock out date
            var startOfDate = new DateTime(NewAdjustedClockOut.Year, NewAdjustedClockOut.Month, NewAdjustedClockOut.Day, 8, 0, 0);
            TimeSpan differenceClockOutDay = new TimeSpan();
            differenceClockOutDay = NewAdjustedClockOut.Subtract(startOfDate);

            var bothDays = differenceClockInDay + differenceClockOutDay;
            hoursWorked.Hours = (WorkingDaysBetween * 9) + bothDays.Hours;
            hoursWorked.Minutes = bothDays.Minutes;
            return hoursWorked;
        }

        public override string ToString()
        {
            return string.Format("Employee ID {0} \t ClockIn {1} \t ClockOut {2}", this.EmployeeID, this.ClockIn, this.ClockOut);
        }
    }
}
