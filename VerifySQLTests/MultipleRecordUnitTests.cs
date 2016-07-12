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
    public class MultipleRecordUnitTests
    {
        [Fact]
        public void TestMultipleEmployeeSingleEntry()
        {
            var emp1Clock1 = ModelCreationHelper.CreateTimeDifference(1, "10/6/2015 8:40AM", "10/6/2015 4:40PM");
            var emp2Clock1 = ModelCreationHelper.CreateTimeDifference(2, "10/6/2015 8:40AM", "10/6/2015 4:40PM");
            var timeList = new List<TimeDifference>() { emp1Clock1, emp2Clock1 };
            var payroll = PayrollCalculator.Calculate(timeList);
            //Verify the payroll list
            Assert.True(payroll != null);
            Assert.True(payroll.AllHoursWorkedByEmployee.Count > 0);
            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(1));
            Assert.True(payroll.AllHoursWorkedByEmployee[1].Count > 0);

            Assert.True(payroll.AllHoursWorkedByEmployee.ContainsKey(2));
            Assert.True(payroll.AllHoursWorkedByEmployee[2].Count > 0);
        }

    }
}
