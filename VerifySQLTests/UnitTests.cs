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
    
    public class UnitTests
    {
        /// <summary>
        /// Tests a very simple same day
        /// </summary>
        [Fact]
        public void TestSameDaySimple()
        {
            var timeList = CreateSimpleDifference(1, "10/6/2015 8:40AM", "10/6/2015 4:40PM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 8);
            Assert.True(record.Minutes == 0);
        }

        /// <summary>
        /// Tests a simple across days
        /// </summary>
        [Fact]
        public void TestAcrossDaysSimple()
        {
            var timeList = CreateSimpleDifference(1, "10/7/2015 1:15PM", "10/9/2015 1:15 PM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 18);
            Assert.True(record.Minutes == 0);
        }

        /// <summary>
        /// Tests a over the weekend and should have ZERO time but still be valid
        /// </summary>
        [Fact]
        public void TestWeekendClock()
        {
            var timeList = CreateSimpleDifference(1, "10/10/2015 8:35AM", "10/11/2015 8:45PM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 0);
            Assert.True(record.Minutes == 0);
        }

        /// <summary>
        /// This test should cap the clockout at 5PM
        /// </summary>
        [Fact]
        public void TestSameDayClockOutAfter5()
        {
            var timeList = CreateSimpleDifference(1, "10/12/2015 10:15 AM", "10/12/2015 5:45 PM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 6);
            Assert.True(record.Minutes == 45);
        }

        /// <summary>
        /// This test should test what happens when the clock out happens before 8AM, in this case
        /// it should cap the time at 5PM the previous workday
        /// </summary>
        [Fact]
        public void TestClockOutBefore8AM()
        {
            var timeList = CreateSimpleDifference(1, "10/28/2015 2:45 PM", "10/29/2015 7:45 AM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 2);
            Assert.True(record.Minutes == 15);
        }

        /// <summary>
        /// Test edge case of clock in after clockout
        /// </summary>
        [Fact]
        public void TestClockOutBeforeClockIn()
        {
            var timeList = CreateSimpleDifference(1, "10/28/2015 2:45 PM", "10/28/2015 1:45 PM");
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

        [Fact]
        public void TestClockInAndClockOutAtSameTime()
        {
            var timeList = CreateSimpleDifference(1, "10/28/2015 2:45 PM", "10/28/2015 2:45 PM");
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
        }

        private List<TimeDifference> CreateSimpleDifference(int employeeID, string clockin, string clockout)
        {
            var timeslip = new TimeSlip(1, DateTime.Parse(clockin), DateTime.Parse(clockout));
            var timeDifference = new TimeDifference(timeslip);
            var timeList = new List<TimeDifference>();
            timeList.Add(timeDifference);
            return timeList;
        }
    }
}
