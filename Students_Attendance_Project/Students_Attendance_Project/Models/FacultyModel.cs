using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class FacultyModel
    {
        public int CheckAddORupdate { get; set; }
        public int FacultyCode { get; set; }
        public string FacultyName { get; set; }
    }
}