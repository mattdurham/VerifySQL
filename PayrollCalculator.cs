using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifySQL.Models;

namespace VerifySQL
{
    public class PayrollCalculator
    {
        public static Payroll Calculate( IEnumerable<TimeDifference> payrollRecords)
        {
            var payroll = new Payroll();
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
