using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class StudyGroupModel
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
        public int UserID { get; set; }
        
        public int CheckAddORupdate { get; set; }
    }
    
}