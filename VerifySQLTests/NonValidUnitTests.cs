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


        /// <summary>
        /// Test Overlap with two records
        /// </summary>
        [Fact]
        public void TestSimpleOverlap()
        {
            var emp1Clock1 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 8:00AM", "07/11/2016 5:00PM");
            var emp1Clock2 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 9:00AM", "07/11/2016 4:00PM");
            var timeList = new List<TimeDifference>() { emp1Clock1, emp1Clock2 };

            var payroll = PayrollCalculator.Calculate(timeList);

            //Should be zero valid records
            Assert.True(payroll.ValidHoursWorkedByEmployee[1].Count == 0);
            //Should be two invalid records
            Assert.True(payroll.InvalidHoursWorkedByEmployee[1].Count == 2);
            //Should be one overlap record
            Assert.True(payroll.OverlappingRecordsByEmployeeID[1].Count == 2);

            foreach(var record in payroll.InvalidHoursWorkedByEmployee[1])
            {
                Assert.True(record.StatusCode == StatusCode.OverlappingRecord);
            }

        }


        /// <summary>
        /// Test That an overlap does NOT occur when there is an invalid record
        /// </summary>
        [Fact]
        public void TestForNonOverlapWhenInvalidOverlap()
        {
            var emp1Clock1 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 8:00AM", "07/11/2016 5:00PM");
            var emp1Clock2 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 4:00PM", "07/11/2016 9:00AM");
            var timeList = new List<TimeDifference>() { emp1Clock1, emp1Clock2 };

            var payroll = PayrollCalculator.Calculate(timeList);

            //Should be zero valid records
            Assert.True(payroll.ValidHoursWorkedByEmployee[1].Count == 1);
            //Should be two invalid records
            Assert.True(payroll.InvalidHoursWorkedByEmployee[1].Count == 1);
            //Should be one overlap record
            Assert.True(payroll.OverlappingRecordsByEmployeeID[1].Count == 0);

        }


        /// <summary>
        /// Test That an overlap does NOT occur when there is an invalid record
        /// </summary>
        [Fact]
        public void TestForNonOverlapWhenSameTimeOverlap()
        {
            var emp1Clock1 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 8:00AM", "07/11/2016 5:00PM");
            var emp1Clock2 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 9:00AM", "07/11/2016 9:00AM");
            var timeList = new List<TimeDifference>() { emp1Clock1, emp1Clock2 };

            var payroll = PayrollCalculator.Calculate(timeList);

            //Should be zero valid records
            Assert.True(payroll.ValidHoursWorkedByEmployee[1].Count == 1);
            //Should be two invalid records
            Assert.True(payroll.InvalidHoursWorkedByEmployee[1].Count == 1);
            //Should be one overlap record
            Assert.True(payroll.OverlappingRecordsByEmployeeID[1].Count == 0);

        }

        /// <summary>
        /// Test Overlap with two sets of overlap records
        /// </summary>
        [Fact]
        public void TestTwoSetOverlap()
        {
            var emp1Clock1 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 8:00AM", "07/11/2016 5:00PM");
            var emp1Clock2 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 9:00AM", "07/11/2016 4:00PM");

            var emp1Clock3 = ModelCreationHelper.CreateTimeDifference(1, "07/12/2016 8:00AM", "07/12/2016 5:00PM");
            var emp1Clock4 = ModelCreationHelper.CreateTimeDifference(1, "07/12/2016 9:00AM", "07/12/2016 4:00PM");

            var timeList = new List<TimeDifference>() { emp1Clock1, emp1Clock2 , emp1Clock3, emp1Clock4};

            var payroll = PayrollCalculator.Calculate(timeList);

            //Should be zero valid records
            Assert.True(payroll.ValidHoursWorkedByEmployee[1].Count == 0);
            //Should be two invalid records
            Assert.True(payroll.InvalidHoursWorkedByEmployee[1].Count == 4);
            //Should be one overlap record
            Assert.True(payroll.OverlappingRecordsByEmployeeID[1].Count == 4);

            foreach (var record in payroll.InvalidHoursWorkedByEmployee[1])
            {
                Assert.True(record.StatusCode == StatusCode.OverlappingRecord);
            }

            Assert.True(payroll.OverlappingRecordsByEmployeeID[1].Count == 4);
            

        }


        /// <summary>
        /// Test Overlap with two sets of overlap records with a shared overlap
        /// </summary>
        [Fact]
        public void TestTwoSetsWithSharedOverlap()
        {
            var emp1Clock1 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 8:00AM", "07/11/2016 5:00PM");
            var emp1Clock2 = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 9:00AM", "07/11/2016 4:00PM");

            var emp1Clock3 = ModelCreationHelper.CreateTimeDifference(1, "07/12/2016 8:00AM", "07/12/2016 5:00PM");
            var emp1Clock4 = ModelCreationHelper.CreateTimeDifference(1, "07/12/2016 9:00AM", "07/12/2016 4:00PM");

            var sharedClock = ModelCreationHelper.CreateTimeDifference(1, "07/11/2016 4:00PM", "07/12/2016 10:00AM");

            var timeList = new List<TimeDifference>() { emp1Clock1, emp1Clock2, emp1Clock3, emp1Clock4, sharedClock };

            var payroll = PayrollCalculator.Calculate(timeList);

            //Should be zero valid records
            Assert.True(payroll.ValidHoursWorkedByEmployee[1].Count == 0);
            //Should be two invalid records
            Assert.True(payroll.InvalidHoursWorkedByEmployee[1].Count == 5);
            //Should be one overlap record
            Assert.True(payroll.OverlappingRecordsByEmployeeID[1].Count == 5);

            foreach (var record in payroll.InvalidHoursWorkedByEmployee[1])
            {
                Assert.True(record.StatusCode == StatusCode.OverlappingRecord);
            }

            

        }

    }
}
