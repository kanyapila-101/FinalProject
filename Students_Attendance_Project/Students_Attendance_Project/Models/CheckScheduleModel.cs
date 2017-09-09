using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class showHolidayModel
    {
        public string HolidayDate { get; set; }
        public string HolidayName { get; set; }
        public string description { get; set; }
    }

    public class CheckScheduleModel
    {
        public string StudyGroupID { get; set; }
        public string SchYearID { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string start { get; set; }
        public string end { get; set; }
    }

    public class showSchoolYearModel
    {
        public string SchYearID { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string Term { get; set; }
        public string Year { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartMidterm { get; set; }
        public string EndMidterm { get; set; }
        public string StartFinal { get; set; }
        public string EndFinal { get; set; }
    }
}  