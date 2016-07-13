using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifySQL.Models
{
    /// <summary>
    /// Identifies a batch of payroll records, includes both valid and invalid items
    /// </summary>
    public class Payroll
    {
        public Payroll()
        {
            ValidHoursWorkedByEmployee = new Dictionary<int, List<HoursWorked>>();
            InvalidHoursWorkedByEmployee = new Dictionary<int, List<HoursWorked>>();
            AllHoursWorkedByEmployee = new Dictionary<int, List<HoursWorked>>();
            OverlappingRecordsByEmployeeID = new Dictionary<int, List<HoursWorked>>();
        }

        
        /// <summary>
        /// Contains all the valid records by employee id, NOTE that if an employee only has INVALID records they will have an entry here; the list will have zero records
        /// </summary>
        public Dictionary<int, List<HoursWorked>> ValidHoursWorkedByEmployee { get; private set; }

        /// <summary>
        /// Contains all the invalid records by employee id, NOTE that if an employee only has VALID records they will have an entry here; the list will have zero records 
        /// </summary>
        public Dictionary<int, List<HoursWorked>> InvalidHoursWorkedByEmployee { get; private set; }

        /// <summary>
        /// Contains all the records for an employee
        /// </summary>
        public Dictionary<int, List<HoursWorked>> AllHoursWorkedByEmployee { get; private set; }

        /// <summary>
        /// Contains all the overlapping records. A overlapping record will exist both here and invalid records in addition to all. This will also force all overlaps into here
        /// So the first record would become an overlap and invalid whenever the second one is found to overlap
        /// </summary>
        public Dictionary<int, List<HoursWorked>> OverlappingRecordsByEmployeeID { get; private set; }

        /// <summary>
        /// Add a record to the the appropriate lists
        /// </summary>
        /// <param name="hoursWorked"></param>
        internal void AddRecord(HoursWorked hoursWorked)
        {
            //Adding all the items at once
            if(!AllHoursWorkedByEmployee.ContainsKey(hoursWorked.ParentRecord.EmployeeID))
            {
                AllHoursWorkedByEmployee.Add(hoursWorked.ParentRecord.EmployeeID, new List<HoursWorked>());
                InvalidHoursWorkedByEmployee.Add(hoursWorked.ParentRecord.EmployeeID, new List<HoursWorked>());
                ValidHoursWorkedByEmployee.Add(hoursWorked.ParentRecord.EmployeeID, new List<HoursWorked>());
                OverlappingRecordsByEmployeeID.Add(hoursWorked.ParentRecord.EmployeeID, new List<HoursWorked>());
            }


            //Check for an overlap, we only check valid records for overlap
            if (hoursWorked.IsValid)
            {
                hoursWorked = AddedToOverlap(hoursWorked);
            }
            //Add all items to the all hours worked
            AllHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Add(hoursWorked);
            
            if (hoursWorked.IsValid)
            {
                ValidHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Add(hoursWorked);
            }
            else
            {
                InvalidHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Add(hoursWorked);
            }
            
            

        }

        /// <summary>
        /// This function will check to see if the record is an overlap. It will remove any records from Valid and add them to Overlap
        /// </summary>
        /// <param name="hoursWorked"></param>
        /// <returns></returns>
        private HoursWorked AddedToOverlap(HoursWorked hoursWorked)
        {
            //TODO REFACTOR THIS AFTER UNIT TESTS

            //Check to see if we have an existing overlap
            var employeeOverlap = OverlappingRecordsByEmployeeID[hoursWorked.ParentRecord.EmployeeID];

            var existingOverlaps = (from record in employeeOverlap
                                    where hoursWorked.ParentRecord.ClockIn <= record.ParentRecord.ClockOut && record.ParentRecord.ClockIn <= hoursWorked.ParentRecord.ClockOut
                                    select record);
            if (existingOverlaps.Count() > 0)
            {
                hoursWorked.StatusCode = StatusCode.OverlappingRecord;
                employeeOverlap.Add(hoursWorked);
                return hoursWorked;
            }

            //Check to see if we have an new overlap with VALID records only. We shall assume invalids are bad anyway and not do any overlap detection
            var employeeRecords = AllHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID];

            //Find any records that overlap and have a status code of valid time or overlap. 
            var overlaps = (from record in employeeRecords
                            where (hoursWorked.ParentRecord.ClockIn <= record.ParentRecord.ClockOut && record.ParentRecord.ClockIn <= hoursWorked.ParentRecord.ClockOut)
                            && (record.StatusCode == StatusCode.ValidTime || record.StatusCode == StatusCode.OverlappingRecord)
                            select record);
            if (overlaps.Count() == 0)
            {
                return hoursWorked;
            }
            hoursWorked.StatusCode = StatusCode.OverlappingRecord;
            OverlappingRecordsByEmployeeID[hoursWorked.ParentRecord.EmployeeID].Add(hoursWorked);
            //Set each record to status Overlapping Record, add it to the invalid and create a new overlap record
            foreach (var record in overlaps)
            {
                record.StatusCode = StatusCode.OverlappingRecord;
                InvalidHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Add(record);
                ValidHoursWorkedByEmployee[hoursWorked.ParentRecord.EmployeeID].Remove(record);
                OverlappingRecordsByEmployeeID[hoursWorked.ParentRecord.EmployeeID].Add(record);
            }
            return hoursWorked;
        }
        
    }
}
