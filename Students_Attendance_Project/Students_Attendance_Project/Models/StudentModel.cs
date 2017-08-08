using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class StudentModel
    {
        public int StdID { get; set; }
        public string StdCode { get; set; }
        public string NameTH { get; set; }
        public string NameEN { get; set; }
        public int StudyGroupID { get; set; }
        public int? StatusID { get; set; }
        //public int ScheduleID { get; set; }
        public int UserID { get; set; }
        public string Prefix { get; set; }
        public string PrefixEN { get; set; }


        public string StudyGroupCode { get; set; }
        public int SchYearID { get; set; }
        public string Term { get; set; }
        public string Year { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string Course { get; set; }
        public string StatusName { get; set; }
    }

    public class StudentShow
    {
        public int StdID { get; set; }
        public string StdCode { get; set; }
        public string NameTH { get; set; }
        public string NameEN { get; set; }
        public int StudyGroupID { get; set; }
        public int? StatusID { get; set; }
        public string StatusName { get; set; }
    }
}