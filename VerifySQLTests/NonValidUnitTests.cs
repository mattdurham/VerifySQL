using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifySQL;
using VerifySQL.Models;
using Xunit;

namespace VerifySQLTests
{
    public class NonValidUnitTests
    {

        /// <summary>
        /// Test edge case of clock in after clockout
        /// </summary>
        [Fact]
        public void TestClockOutBeforeClockIn()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "10/28/2015 2:45 PM", "10/28/2015 1:45 PM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid == false);
            Assert.True(record.Hours == 0);
            Assert.True(record.Minutes == 0);
            Assert.True(record.StatusCode == StatusCode.ClockInAfterClockOut);
        }


        /// <summary>
        /// Test edge case around the times being the same
        /// </summary>
        [Fact]
        public void TestClockInAndClockOutAtSameTime()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "10/28/2015 2:45 PM", "10/28/2015 2:45 PM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid == false);
            Assert.True(record.Hours == 0);
            Assert.True(record.Minutes == 0);
            Assert.True(record.StatusCode == StatusCode.ClockInAndClockOutSame);

            //Lets verify that the ValidRecords exists for the employee ID but has zero records
            Assert.True(payroll.ValidHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.ValidHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.ValidHoursWorkedByEmployee[1].Count == 0);

            //Verify that the invalid records exists for the employee id and has a single record
            Assert.True(payroll.InvalidHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.InvalidHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.InvalidHoursWorkedByEmployee[1].Count == 1);
        }

    }
}
