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
    }

    public class CheckScheduleModel
    {
        public string StudyGroupID { get; set; }
        //public string StudyGroupCode { get; set; }
        public string SchYearID { get; set; }
        //public string SubjectName { get; set; }
        //public string SubjectCode { get; set; }
        //public string DateCheck { get; set; }
        //public string StartTime { get; set; }
        //public string EndTime { get; set; }
        //public string TypeSubject { get; set; }
        //public string RoomNo { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string url { get; set; }
    }
}  