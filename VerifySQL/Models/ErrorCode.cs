using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{
    /// <summary>
    /// Identifies the status of the Hoursworked Record
    /// </summary>
    public enum StatusCode { ValidTime = 0, ClockInAndClockOutSame = 1, ClockInAfterClockOut = 2};
    
}
