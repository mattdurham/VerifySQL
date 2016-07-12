using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Diagnostics;
using VerifySQL.Models;

namespace VerifySQL
{
    class Program
    {

        static void Main(string[] args)
        {
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
                connection.Close();
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

    }

    

}
