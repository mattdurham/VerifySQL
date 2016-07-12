using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifySQL.Models;

namespace VerifySQL
{
    internal class HoursCalculator
    {
        public Payroll GetPayroll(DateTime start, DateTime end, IEnumerable<TimeDifference> payrollRecords)
        {
            var payroll = new Payroll(start, end);
            Parallel.ForEach(payrollRecords, record =>
            {
                var hoursWorked = record.EvaluateHoursAndMinutes();
                lock(payroll)
                {
                    payroll.AddRecord(hoursWorked);
                }
            });
            return payroll;
        }
    }
}
