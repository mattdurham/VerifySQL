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
        /// <summary>
        /// Calculate the Payroll for a given number of records. Will parralize the task.
        /// </summary>
        /// <param name="payrollRecords"></param>
        /// <returns></returns>
        public static Payroll Calculate( IEnumerable<TimeDifference> payrollRecords)
        {
            var payroll = new Payroll();
            Parallel.ForEach(payrollRecords, record =>
            {
                var hoursWorked = record.EvaluateHoursAndMinutes();
                //Payroll is not thread safe so we need to lock the object to add a record
                lock(payroll)
                {
                    payroll.AddRecord(hoursWorked);
                }
            });
         
            return payroll;
        }
    }
}
