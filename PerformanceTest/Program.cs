using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VerifySQL;
using VerifySQL.Models;
using VerifySQLTests;
using Xunit;

namespace PerformanceTest
{
    class Program
    {
        const int MAX_EMPLOYEES = 10000;
        const int PUNCHES = 1000;
        static void Main(string[] args)
        {
            /*
             * On a 4 core i5 2500k desktop I was able to process 10,000,000 entries in 6-7 seconds
             * */
            var timeSlips = new List<TimeDifference>();
            for (int employeeID = 1; employeeID <= MAX_EMPLOYEES; employeeID++)
            {
                var start = new DateTime(2001, 01, 01, 12, 0, 0);
                for (int punchCount = 1; punchCount <= PUNCHES; punchCount++)
                {
                    start = GetRandomDateTimes(start);
                    var end = GetRandomDateTimes(start);
                    timeSlips.Add(new TimeDifference(new TimeSlip(employeeID, start, end)));
                    start = end;
                }
            }
            //Dot Net Benchmark would be better for this, but this is quick and dirty
            var startClock = DateTime.Now;
            var payroll = PayrollCalculator.Calculate(timeSlips);
            Console.WriteLine(DateTime.Now.Subtract(startClock).TotalSeconds);
            Assert.True(payroll.AllHoursWorkedByEmployee.Keys.Count == MAX_EMPLOYEES);
            foreach(var key in payroll.AllHoursWorkedByEmployee.Keys)
            {
                Assert.True(payroll.AllHoursWorkedByEmployee[key].Count == PUNCHES);
                Assert.True(payroll.InvalidHoursWorkedByEmployee[key].Count + payroll.ValidHoursWorkedByEmployee[key].Count == PUNCHES);
            }
        }


        static DateTime GetRandomDateTimes(DateTime start)
        {
            Random rnd = new Random();
            var newStart = start.AddMinutes(rnd.Next(40, 60 * 24 * 4));
            return new DateTime(newStart.Year, newStart.Month, newStart.Day, newStart.Hour, newStart.Minute, 0);

        }
    }
    
}
