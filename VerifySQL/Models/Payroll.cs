using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{
    public class Payroll
    {
        public Payroll()
        {
            ValidHoursWorkedByEmployee = new Dictionary<int, List<HoursWorked>>();
            InvalidHoursWorkedByEmployee = new Dictionary<int, List<HoursWorked>>();
            AllHoursWorkedByEmployee = new Dictionary<int, List<HoursWorked>>();
        }

        
        /// <summary>
        /// Contains all the valid records by employee id, NOTE that if an employee only has INVALID records they will have an entry here; the list will have zero records
        /// </summary>
        public Dictionary<int, List<HoursWorked>> ValidHoursWorkedByEmployee { get; set; }

        /// <summary>
        /// Contains all the invalid records by employee id, NOTE that if an employee only has VALID records they will have an entry here; the list will have zero records 
        /// </summary>
        public Dictionary<int, List<HoursWorked>> InvalidHoursWorkedByEmployee { get; set; }

        /// <summary>
        /// Contains all the records for an employee
        /// </summary>
        public Dictionary<int, List<HoursWorked>> AllHoursWorkedByEmployee { get; set; }


        internal void AddRecord(HoursWorked hoursWorked)
        {
            //Adding all the items at once
            if(!AllHoursWorkedByEmployee.ContainsKey(hoursWorked.ParentRecord.EmployeeID))
            {
                AllHoursWorkedByEmployee.Add(hoursWorked.ParentRecord.EmployeeID, new List<HoursWorked>());
                InvalidHoursWorkedByEmployee.Add(hoursWorked.ParentRecord.EmployeeID, new List<HoursWorked>());
                ValidHoursWorkedByEmployee.Add(hoursWorked.ParentRecord.EmployeeID, new List<HoursWorked>());
            }
            AllHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Add(hoursWorked);

            if(hoursWorked.IsValid)
            {
                ValidHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Add(hoursWorked);
            }
            else
            {
                InvalidHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Add(hoursWorked);
            }

        }

        
    }
}
