using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class DepartmentModel
    {
        public int CheckAddORupdate { get; set; }
        public int DeptID { get; set; }
        public int DeptCode { get; set; }
        public string DeptName { get; set; }
        public string ShortName { get; set; }
        public int FacultyCode { get; set; }
        public string FacultyName { get; set; }
    }
}