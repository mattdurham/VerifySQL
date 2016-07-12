using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{
    public class HoursWorked
    {

        public HoursWorked(TimeDifference parentRecord)
        {
            ParentRecord = parentRecord;
        }
        public TimeDifference ParentRecord { get; private set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public bool IsValid
        {
            get
            {
                return StatusCode == StatusCode.ValidTime;
            }
        }
        public StatusCode StatusCode { get; set; }
    }
}
