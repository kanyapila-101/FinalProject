using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{

    public class SummaryModel
    {
        public int StudyGroupID { get; set; }
        public string StudyGroupCode { get; set; }
        public int DeptCode { get; set; }
        public string DeptName { get; set; }
        public int FacultyCode { get; set; }
        public string FacultyName { get; set; }
        public int SchYearID { get; set; }
        public string Term { get; set; }
        public string Year { get; set; }
        public string SubjectCode { get; set; }
        public string Course { get; set; }
        public string SubjectName { get; set; }
        public int? SubjectTheory { get; set; }
        public int? SubjectPractice { get; set; }
        public string StdCode { get; set; }
        public string NameTH { get; set; }
        public string NameEN { get; set; }
        public int UserID { get; set; }
        public string StatusID { get; set; }
        public DateTime? DateCheck { get; set; }
        public int Count { get; set; }
    }

    public class DisplaySumModel
    {
        public DateTime DateCheck { get; set; }
        public string Note { get; set; }
        public int? StatusID { get; set; }
    }

}