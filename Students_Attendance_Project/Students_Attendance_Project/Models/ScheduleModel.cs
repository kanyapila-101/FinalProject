using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class ScheduleModel
    {
        public int ScheduleID { get; set; }
        public string DayTeach { get; set; }
        public string SubjectCode { get; set; }
        public string Course { get; set; }
        public int StartTimeInt { get; set; }
        public int EndTimeInt { get; set; }
        public string RoomNo { get; set; }
        public int StudyGroupID { get; set; }
        public int UserID { get; set; }
        public int SchYearID { get; set; }
        public int SubjectTheory { get; set; }
        public int SubjectPractice { get; set; }
        public int TypeSubject { get; set; }

        public string BuildingCode { get; set; }
        public string BuildingName { get; set; }

    }

    public class ScheduleShow
    {
        public int ScheduleID { get; set; }
        public string DayTeach { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int StartTimeInt { get; set; }
        public int EndTimeInt { get; set; }
        public string RoomNo { get; set; }
        public int StudyGroupID { get; set; }
        public int UserID { get; set; }
        public int SchYearID { get; set; }
        public int? TotalHour { get; set; }
    }

    public class GenerateDateModel
    {
        public int SchYearID { get; set; }
        public int DayTeach { get; set; }
        public int StudyGroupID { get; set; }
    }

    public class GenStudentModel
    {
        public int StudyGroupID { get; set; }
        public int StdID { get; set; }
        public string StdCode { get; set; }
    }
}