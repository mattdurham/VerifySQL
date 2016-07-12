using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifySQL.Models;

namespace VerifySQLTests
{
    public static class ModelCreationHelper
    {

        public static List<TimeDifference> CreateSimpleDifferenceList(int employeeID, string clockin, string clockout)
        {
            var timeDifference = CreateTimeDifference(employeeID, clockin, clockout);
            var timeList = new List<TimeDifference>();
            timeList.Add(timeDifference);
            return timeList;
        }

        public static  TimeDifference CreateTimeDifference(int employeeID, string clockin, string clockout)
        {
            var timeslip = new TimeSlip(employeeID, DateTime.Parse(clockin), DateTime.Parse(clockout));
            var timeDifference = new TimeDifference(timeslip);
            return timeDifference;
        }
    }
}
