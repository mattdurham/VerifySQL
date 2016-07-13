using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{
    /// <summary>
    /// Result class for hours worked
    /// </summary>
    public class HoursWorked
    {

        public HoursWorked(TimeDifference parentRecord)
        {
            ParentRecord = parentRecord;
        }

        public TimeDifference ParentRecord { get; private set; }

        /// <summary>
        /// If IsValid == false this will be zero
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// If IsValid == false this will be zero
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// If the status code is ValidTime then return true
        /// </summary>
        public bool IsValid
        {
            get
            {
                return StatusCode == StatusCode.ValidTime;
            }
        }

        public StatusCode StatusCode { get; set; }

        public override string ToString()
        {
            return ParentRecord.ToString() + String.Format("\t Status Code: {0} \t Hours: {1} \t Minutes:{2}", Enum.GetName(StatusCode.GetType(), StatusCode), Hours, Minutes);
        }
    }
}
