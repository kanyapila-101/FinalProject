using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class SubjectModel
    {
        public int CheckAddORupdate { get; set; }
        public string SubjectCode { get; set; }
        public string Course { get; set; }
        public string SubjectName { get; set; }
        public string SubjectNameEN { get; set; }
        public double Condition { get; set; }
        public int SubjectTheory { get; set; }
        public int TimeTheory { get; set; }
        public int SubjectPractice { get; set; }
        public int TimePractice { get; set; }

        public int checkPractice { get; set; }
        public int checkThoery { get; set; }
    }
    
}