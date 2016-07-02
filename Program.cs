using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Diagnostics;

namespace VerifySQL
{
    class Program
    {

        static void Main(string[] args)
        {
            //Test the items given
            var td = GetTimeDifference(new DateTime(2015, 10, 06, 8, 40, 0), new DateTime(2015, 10, 06, 16, 40, 0));
            Console.WriteLine(td.ToString());

            td = GetTimeDifference(new DateTime(2015, 10, 07, 13, 15, 0), new DateTime(2015, 10, 09, 13, 15, 0));
            Console.WriteLine(td.ToString());

            td = GetTimeDifference(new DateTime(2015, 10, 10, 8, 35, 0), new DateTime(2015, 10, 11, 20, 45, 0));
            Console.WriteLine(td.ToString());

            td = GetTimeDifference(new DateTime(2015, 10, 12, 10, 15, 0), new DateTime(2015, 10, 12, 17, 45, 0));
            Console.WriteLine(td.ToString());

            td = GetTimeDifference(new DateTime(2015, 10, 28, 14, 45, 0), new DateTime(2015, 10, 29, 7, 45, 0));
            Console.WriteLine(td.ToString());

            td = GetTimeDifference(new DateTime(2016, 01, 27, 13, 57, 0), new DateTime(2016, 01, 30, 17, 48, 0));
            Console.WriteLine(td.ToString());

            //'1/24/2016 2:58:00 AM','1/25/2016 5:56:00 PM'

            td = GetTimeDifference(new DateTime(2016, 01, 24, 2, 58, 0), new DateTime(2016, 01, 25, 17, 56, 0));
            Console.WriteLine(td.ToString());


            //Now Lets test some random entries
            var sb = new SqlConnectionStringBuilder();
            sb.IntegratedSecurity = true;
            sb.InitialCatalog = "master";
            sb.DataSource = "localhost";
            IEnumerable<TimeSlip> slips = new List<TimeSlip>();
            using (var connection = new SqlConnection(sb.ToString()))
            {
                connection.Open();
                connection.Execute("CREATE TABLE #EmployeePunches (     PunchID     INT IDENTITY(1, 1)     ,  ClockIn    DATETIME     , ClockOut   DATETIME, EmpID int default 100 ) ");
                var start = new DateTime(2001, 01, 01, 12, 0, 0);
                var maxTime = DateTime.Parse("2054-10-04 00:00:00.000");
                while (true)
                {
                    start = GetRandomDateTimes(start);
                    var end = GetRandomDateTimes(start);
                    //If we go over the max we can handle then exit
                    if(start > maxTime || end > maxTime)
                    {
                        break;
                    }
                    connection.Execute("INSERT INTO #EmployeePunches (ClockIn,ClockOut) VALUES (@clockin, @clockout)", new { clockin = start, clockout = end });
                    start = end;
                }
                slips = connection.Query<TimeSlip>(HIDEOUS_SQL);
                connection.Close();
            }
            foreach (var slip in slips)
            {
                var result = GetTimeDifference(slip.ClockIn, slip.ClockOut);
                if(result.Hours != slip.ValidHours)
                {
                    Console.WriteLine("Error!");
                    Console.ReadLine();
                }
                if(result.Minutes != slip.ValidMinutes)
                {
                    Console.WriteLine("Error!");
                    Console.ReadLine();
                }

            }
            Console.WriteLine("All Tests Pass");
            Console.ReadLine();

        }

        static DateTime GetRandomDateTimes(DateTime start)
        {
            Random rnd = new Random();
            var newStart = start.AddMinutes(rnd.Next(40, 60 * 24 * 4));
            return new DateTime(newStart.Year, newStart.Month, newStart.Day, newStart.Hour, newStart.Minute, 0);
           
        }



        static TimeDifference GetTimeDifference(DateTime clockIn, DateTime clockOut)
        {
            var timeDifference = new TimeDifference(clockIn,clockOut);
            return timeDifference;
        }

        const string HIDEOUS_SQL = @"
select *, minutesworked / 60 as validhours, minutesworked % 60 as validminutes from (
select punchid
	,sum(datediff(minute,'1900-01-01 00:00:00.000',
		case
		--Invalid data detection
		when ClockOut < ClockIn
			then dateadd(hour,0,'1900-01-01 00:00:00.000')
		--Edge case detection, if the times are the same
		when clockout = ClockIn 
			then dateadd(hour,0,'1900-01-01 00:00:00.000')
		--Early check if the clockout is before start of day
		when clockout < startofday 
			then dateadd(hour,0,'1900-01-01 00:00:00.000')
		--Early check if checkin is after the end of the day
		when clockin > endofday 
			then dateadd(hour,0,'1900-01-01 00:00:00.000')
		--If this is a date in between the clock in and clock out just use whatever the referenced work hours available is
		-- ie 9 for M-F and 0 for Sat-Sun
		when cast(clockin as date) <> cast(datereference as date) and cast(clockout as date) <> cast(datereference as date) 
			then  dateadd(hour,workhoursavailable,'1900-01-01 00:00:00.000')  
		--If its a single day and its work day, then get the boundaries if clockin before 8 AM then set it to 8AM and if clockout is 
		--   after 5PM then set it to 5 PM
        when cast(clockin as date) = cast(ClockOut as date) and workhoursavailable > 0
			then iif(ClockOut > endofday,endofday,clockout) - iif(clockin < startofday,startofday,clockin)
		--Calculate for the clock in date, again checking for boundaries and that it is a work day
		when cast(clockin as date) = datereference and workhoursavailable > 0
			then endofday - iif(clockin < startofday,startofday,clockin) 
		--Calculate for the clock out date, again checking for boundaries and that it is a work day
		when cast(ClockOut as date) = datereference and workhoursavailable > 0
			then iif(ClockOut > endofday,endofday,clockout) - startofday
		else 0
		end)) as minutesworked
	from  #EmployeePunches 
, 
(
select 
	datereference, 
	case 
		when datename(dw,datereference) in ('Saturday','Sunday') 
			then 0 
		else 9 
		end as workhoursavailable
		,dateadd(hour,8,datereference) as startofday
		,dateadd(hour,17,datereference) as endofday
		from
		
	(SELECT TOP 20000 dateadd(day,row_number() over(order by t1.number) ,'2000-01-01') as datereference
		FROM master..spt_values t1 
		CROSS JOIN master..spt_values t2) as dates) as dateref
	where cast(ClockIn as date) <= cast(datereference as date) and cast(clockout as date) >= cast(datereference as date) group by PunchID) as calculated

	join #EmployeePunches as base on base.PunchID = calculated.PunchID
    ";
    }

    class TimeDifference
    {
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }


        public TimeDifference(DateTime clockIn, DateTime clockOut)
        {
            ClockIn = clockIn;
            ClockOut = clockOut;
            EvaluateHoursAndMinutes();
        }

        public DateTime NewAdjustedClockIn
        {
            get
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
        }

        public DateTime NewAdjustedClockOut
        {
            get
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
                    if(ClockOut.DayOfWeek == DayOfWeek.Monday)
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
        }

        public int WorkingDaysBetween
        {
            get
            {
                var workinDaysBetween = 0;
                var indexDate = NewAdjustedClockIn.Date.AddDays(1);
                while (indexDate.Date < NewAdjustedClockOut.Date)
                {
                    if (indexDate.DayOfWeek != DayOfWeek.Sunday && indexDate.DayOfWeek != DayOfWeek.Saturday)
                    {
                        workinDaysBetween++;
                    }
                    indexDate = indexDate.Date.AddDays(1);
                }
                return workinDaysBetween;
            }
        }


        private void EvaluateHoursAndMinutes()
        {
            //If there is some error then return early
            if(NewAdjustedClockIn > NewAdjustedClockOut)
            {
                return;
            }
            //If the times are the same exit earl with zeros
            if(NewAdjustedClockIn == NewAdjustedClockOut)
            {
                return;
            }
            //We can simply calculate the time for this single day
            else if (NewAdjustedClockIn.Date == NewAdjustedClockOut.Date)
            {
                //If the date is the same and its Sunday or Saturday we can leave early.
                if (NewAdjustedClockIn.Date.DayOfWeek == DayOfWeek.Sunday || NewAdjustedClockIn.DayOfWeek == DayOfWeek.Saturday)
                {
                    return;
                }

                var timediff = NewAdjustedClockOut.Subtract(NewAdjustedClockIn);
                this.Hours = timediff.Hours;
                this.Minutes = timediff.Minutes;
                return;
            }

            //Lets try to handle the case where the days are all on the weekend, this is probably not the most
            //  effecient code but since its a sanity check I want clarity over performance
            var indexDate = NewAdjustedClockIn.Date;
            var weekdays = 0;
            while (indexDate.Date != NewAdjustedClockOut.Date)
            {
                if (indexDate.DayOfWeek != DayOfWeek.Sunday && indexDate.DayOfWeek != DayOfWeek.Saturday)
                {
                    weekdays++;
                    break;
                }
                indexDate = indexDate.AddDays(1);
            }
            if (weekdays == 0)
            {
                return;
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

           
            var totalHours = (WorkingDaysBetween * 9) + bothDays.Hours;
            var totalMinutes = bothDays.Minutes;
            this.Hours = totalHours;
            this.Minutes = totalMinutes;
        }

        public override string ToString()
        {
            return "Clock In: " + ClockIn.ToString() + Environment.NewLine + "Clock Out: " + ClockOut.ToString() + Environment.NewLine + "Hours:" + Hours.ToString() + " Minutes:" + Minutes.ToString();
        }
    }

    class TimeSlip
    {
      
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public int WorkDays { get; set; }
        public string ClockInDay { get; set; }
        public string ClockOutDay { get; set; }
        public int ValidHours { get; set; }
       
        public int ValidMinutes { get; set; }

      
        
    }
}
