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

            td = GetTimeDifference(new DateTime(2016, 01, 27, 13, 57, 0), new DateTime(2016, 01, 30, 17, 48, 0));
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
                var start = new DateTime(2016, 01, 22, 12, 0, 0);
                for(int i = 0; i < 1000;i++)
                {
                    start = GetRandomDateTimes(start);
                    var end = GetRandomDateTimes(start);
                    Debug.Assert(end > start);
                    connection.Execute("INSERT INTO #EmployeePunches (ClockIn,ClockOut) VALUES (@clockin, @clockout)", new { clockin = start, clockout = end });
                    start = end;
                }
                slips = connection.Query<TimeSlip>(HIDEOUS_SQL);
                connection.Close();
            }
            Parallel.ForEach(slips, slip =>
            {
                var result = GetTimeDifference(slip.ClockIn, slip.ClockOut);
                Debug.Assert(result.Hours == slip.ValidHours);
                Debug.Assert(result.Minutes == slip.ValidMinutes);

            });

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
            var timeDifference = new TimeDifference();
            timeDifference.ClockIn = clockIn;
            timeDifference.ClockOut = clockOut;

            var adjustedClockInDatetime = clockIn;
            if (clockIn.Hour < 8)
            {
                adjustedClockInDatetime = new DateTime(clockIn.Year, clockIn.Month, clockIn.Day, 8, 0, 0);
            }
            else if (clockIn.Hour >= 17)
            {
                adjustedClockInDatetime = new DateTime(clockIn.Year, clockIn.Month, clockIn.Day, 8, 0, 0).AddDays(1);
            }

            var adjustedClockOutDatetime = clockOut;
            if (clockOut.Hour >= 17)
            {
                adjustedClockOutDatetime = new DateTime(clockOut.Year, clockOut.Month, clockOut.Day, 17, 0, 0);
            }
            else if (clockOut.Hour < 8)
            {
                adjustedClockOutDatetime = new DateTime(clockOut.Year, clockOut.Month, clockOut.Day, 17, 0, 0).AddDays(-1);
            }

            //We can simply calculate the time for this single day
            if (adjustedClockInDatetime.Date == adjustedClockOutDatetime.Date)
            {
                //If the date is the same and its Sunday or Saturday we can leave early.
                if (adjustedClockInDatetime.Date.DayOfWeek == DayOfWeek.Sunday || adjustedClockInDatetime.DayOfWeek == DayOfWeek.Saturday)
                {
                    return timeDifference;
                }

                var timediff = adjustedClockOutDatetime.Subtract(adjustedClockInDatetime);
                timeDifference.Hours = timediff.Hours;
                timeDifference.Minutes = timediff.Minutes;
                return timeDifference;
            }

            //Lets try to handle the case where the days are all on the weekend, this is probably not the most
            //  effecient code but since its a sanity check I want clarity over performance
            var indexDate = adjustedClockInDatetime.Date;
            var weekdays = 0;
            while (indexDate.Date != adjustedClockOutDatetime.Date)
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
                return timeDifference;
            }

            //Now for the complicated test of trying to figure out what we need to do with multi item days.

            //Get the time to the end of the day for the clock in event
            var endOfDay = new DateTime(adjustedClockInDatetime.Year, adjustedClockInDatetime.Month, adjustedClockInDatetime.Day, 17, 0, 0);
            TimeSpan differenceClockInDay = new TimeSpan();
            if (adjustedClockInDatetime.DayOfWeek != DayOfWeek.Sunday && adjustedClockInDatetime.DayOfWeek != DayOfWeek.Saturday)
            {
                differenceClockInDay = endOfDay.Subtract(adjustedClockInDatetime);
            }

            //Get the time for the start of day to the clock out date
            var startOfDate = new DateTime(adjustedClockOutDatetime.Year, adjustedClockOutDatetime.Month, adjustedClockOutDatetime.Day, 8, 0, 0);
            TimeSpan differenceClockOutDay = new TimeSpan();
            if (adjustedClockOutDatetime.DayOfWeek != DayOfWeek.Sunday && adjustedClockOutDatetime.DayOfWeek != DayOfWeek.Saturday)
            {
                differenceClockOutDay = adjustedClockOutDatetime.Subtract(startOfDate);
            }

            var bothDays = differenceClockInDay + differenceClockOutDay;

            //Now we need to find the working days between them
            //Start on the next day
            indexDate = adjustedClockInDatetime.Date.AddDays(1);
            var workinDaysBetween = 0;
            while (indexDate.Date != adjustedClockOutDatetime.Date)
            {
                if (indexDate.DayOfWeek != DayOfWeek.Sunday && indexDate.DayOfWeek != DayOfWeek.Saturday)
                {
                    workinDaysBetween++;
                }
                indexDate = indexDate.Date.AddDays(1);
            }
            var totalHours = (workinDaysBetween * 9) + bothDays.Hours;
            var totalMinutes = bothDays.Minutes;
            timeDifference.Hours = totalHours;
            timeDifference.Minutes = totalMinutes;
            return timeDifference;
        }

        const string HIDEOUS_SQL = @"
select P1.PunchID,EmpID,
	ClockIn,
	ClockOut,
	AdjClockIn,
	AdjClockOut,
	WorkDays,
	datename(dw,clockin) as ClockInDay,
	datename(dw,ClockOut) as ClockOutDay,
	WorkDays - IIF(cast(ClockIn as date) <> cast(AdjClockIn as date), 1,0) - IIF(cast(ClockOut as date) <> cast(AdjClockOut as date), 1,0) as AdjustedWorkDays,
	case 
		when WorkDays < 0 then 
			0
	    when (WorkDays - IIF(cast(ClockIn as date) <> cast(AdjClockIn as date), 1,0) - IIF(cast(ClockOut as date) <> cast(AdjClockOut as date), 1,0)) > 0 or cast(AdjClockIn as date) <> cast(AdjClockOut as date) then 
			--Get the clock in to the 5PM end of day then add those hours to 8 AM to clock out finally calculate the full work days between there as 9 hours each
			cast(floor((cast(dateadd(hour,17,cast(cast(AdjClockIn as date) as datetime)) - AdjClockIn as numeric(16,8)) * 24.0) + (cast(AdjClockOut - dateadd(hour,8,cast(cast(AdjClockOut as date) as datetime)) as numeric(16,8)) * 24.0)) + ((WorkDays  - IIF(cast(ClockIn as date) <> cast(AdjClockIn as date), 1,0) - IIF(cast(ClockOut as date) <> cast(AdjClockOut as date), 1,0)) * 9.0) as int)
		else 
			floor(cast(cast(cast(AdjClockOut as datetime) - cast(AdjClockIn as datetime) as float) * 24.0 as int))
	    end as 'ValidHours',
	case when WorkDays < 0
			then 0
	    when WorkDays > 0
			then  cast(ROUND((cast(cast(dateadd(hour,17,cast(cast(AdjClockIn as date) as datetime)) - AdjClockIn as numeric(16,8)) * 24.0 + cast(AdjClockOut - dateadd(hour,8,cast(cast(AdjClockOut as date) as datetime)) as numeric(16,8)) * 24.0 as numeric(16,8)) % 1) * 60,0) as int)
	else DATEDIFF(minute,AdjClockIn,AdjClockOut) % 60
	end as 'ValidMinutes'
	from #EmployeePunches as p1
	join (
			--Sanitize the Clock In dates
			select case 
					when DATENAME(dw,clockin) = 'Saturday' 
						--Set the date to Monday at 8 AM
						then dateadd(hour,8,dateadd(day,2,cast(cast(clockin as date) as datetime)))
					when DATENAME(dw,clockin) = 'Sunday' 
						--Set the date to Monday at 8 AM			
						then dateadd(hour,8,dateadd(day,1,cast(cast(clockin as date) as datetime)))
					when datepart(minute,clockin) + (datepart(hour,clockin) * 60) < 480 
						then dateadd(hh,8,cast(cast(clockin as date) as datetime)) 
					when  datepart(minute,clockin) + (datepart(hour,clockin) * 60) > 1020
						then dateadd(hh,8,cast(dateadd(day,1,cast(clockin as date)) as datetime)) 
					else 
						clockin 
					end  as AdjClockIn,
			       case 
						when DATENAME(dw,clockout) = 'Saturday' 
						--Set the date to Monday at 8 AM
						then dateadd(hour,17,dateadd(day,-1,cast(cast(clockout as date) as datetime)))
					when DATENAME(dw,clockout) = 'Sunday' 
						--Set the date to Monday at 8 AM			
						then dateadd(hour,17,dateadd(day,-2,cast(cast(clockout as date) as datetime)))
					when datepart(minute,clockout) + (datepart(hour,clockout) * 60) > 1020 
						then dateadd(hh,17,cast(cast(clockout as date) as datetime)) 
					when datepart(minute,clockout) + (datepart(hour,clockout) * 60) < 480 
						then dateadd(hh,17,cast(dateadd(day,-1,cast(clockout as date)) as datetime)) 
					else 
						clockout end as AdjClockOut,
				   case 
					when cast(clockin as date) = cast(clockout as date) 
						then 0 
				   else ( 
							(datepart(dy,clockout) - datepart(dy,clockin) - 1) - 
							(DATEDIFF(wk,clockin, clockout) * 2) - 
							(CASE 
								WHEN DATENAME(dw, clockout) = 'Sunday' THEN 1 ELSE 0 END) - (CASE WHEN DATENAME(dw, clockout) = 'Saturday' THEN 1 ELSE 0 END))  END as WorkDays,
				   PunchID
				   from #EmployeePunches) as p2 on p2.PunchID = p1.PunchID
    ";
    }

    class TimeDifference
    {
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }

        public override string ToString()
        {
            return "Clock In: " + ClockIn.ToString() + Environment.NewLine + "Clock Out: " + ClockOut.ToString() + Environment.NewLine + "Hours:" + Hours.ToString() + " Minutes:" + Minutes.ToString();
        }
    }

    class TimeSlip
    {
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public DateTime AdjClockIn { get; set; }
        public DateTime AdjClockOut { get; set; }
        public int WorkDays { get; set; }
        public string ClockInDay { get; set; }
        public string ClockOutDay { get; set; }
        public int ValidHours { get; set; }
        public int ValidMinutes { get; set; }
    }
}
