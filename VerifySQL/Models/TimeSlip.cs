using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{

    public class TimeSlip
    {
        public TimeSlip(int employeeID, DateTime clockIn, DateTime clockOut)
        {
            EmployeeId = employeeID;
            ClockIn = clockIn;
            ClockOut = clockOut;
        }

        public int EmployeeId { get; private set; }
        public DateTime ClockIn { get; private set; }
        public DateTime ClockOut { get; private set; }
    }
}
