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
    
    public class SingleRecordUnitTests
    {
        /// <summary>
        /// Tests a very simple same day
        /// </summary>
        [Fact]
        public void TestSameDaySimple()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "10/6/2015 8:40AM", "10/6/2015 4:40PM");
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

            //Lets verify that the invalid record key exists but that the list has zero records
            Assert.True(payroll.ValidHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.ValidHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.ValidHoursWorkedByEmployee[1].Count == 1);

            //Verify that the invalid records exists for the employee id and zero records
            Assert.True(payroll.InvalidHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.InvalidHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.InvalidHoursWorkedByEmployee[1].Count == 0);
        }

        /// <summary>
        /// Tests a simple across days
        /// </summary>
        [Fact]
        public void TestAcrossDaysSimple()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "10/7/2015 1:15PM", "10/9/2015 1:15 PM");
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
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "10/10/2015 8:35AM", "10/11/2015 8:45PM");
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
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "10/12/2015 10:15 AM", "10/12/2015 5:45 PM");
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
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "10/28/2015 2:45 PM", "10/29/2015 7:45 AM");
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

        /// <summary>
        /// Test that if you clock out at 2AM it moves the clockout to the previous day at 5PM
        /// </summary>
        [Fact]
        public void TestCaseOverSeveralDaysWithClockOutAt2AM()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "1/26/2016 9:54:00 AM", "1/29/2016 2:18:00 AM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 25);
            Assert.True(record.Minutes == 6);
        }

        /// <summary>
        /// Test that if the Clock in is on a sunday that the clock in moves to 8AM on Monday
        /// </summary>
        [Fact]
        public void TestCaseWhereClockInOnSunday()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "3/27/2016 12:34:00 AM", "3/29/2016 8:46:00 AM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 9);
            Assert.True(record.Minutes == 46);
        }


        /// <summary>
        /// Test that clockin on Friday after 5PM and the clockout is on Monday moves the clockin time to 8AM
        /// </summary>
        [Fact]
        public void TestCaseWhereClockInOnFridayAfter5PM()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "3/05/2016 7:34:00 PM", "3/07/2016 9:00:00 AM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 1);
            Assert.True(record.Minutes == 0);
        }


        /// <summary>
        /// Test that clockin on Friday after 5PM and the clockout is on Monday moves the clockin time to 8AM
        /// </summary>
        [Fact]
        public void TestCaseWhereClockOutOnMondayBefore8AM()
        {
            var timeList = ModelCreationHelper.CreateSimpleDifferenceList(1, "3/04/2016 4:00:00 PM", "3/07/2016 7:00:00 AM");
            var payroll = PayrollCalculator.Calculate(timeList);

            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            //Lets verify the individual record
            var record = payroll.AllHoursWorkedByEmployee[1].ElementAt(0);
            Assert.True(record.IsValid);
            Assert.True(record.Hours == 1);
            Assert.True(record.Minutes == 0);
        }


    }
}
