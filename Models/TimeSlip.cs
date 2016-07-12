using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{

    internal class TimeSlip
    {
        public int EmployeeId { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public int WorkDays { get; set; }
        public string ClockInDay { get; set; }
        public string ClockOutDay { get; set; }
        public int ValidHours { get; set; }
        public int ValidMinutes { get; set; }
    }
}
