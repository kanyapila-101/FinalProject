using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class HolidayModel
    {
        public int HolidayID { get; set; }
        public DateTime HolidayDate { get; set; }
        public string dataDate { get; set; }
        public string HolidayName { get; set; }
    }
}