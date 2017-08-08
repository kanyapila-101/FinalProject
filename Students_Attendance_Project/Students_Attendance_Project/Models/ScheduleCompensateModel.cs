using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class Schedule_CompensateModel
    {
        public string Date_compensate { get; set; }
        public string Date_Normal { get; set; }
        public string Day_Teaches { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int ScheduleID { get; set; }
        public int UserID { get; set; }
    }
}