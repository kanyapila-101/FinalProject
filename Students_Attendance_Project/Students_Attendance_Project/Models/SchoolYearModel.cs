using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class SchoolYearModel
    {
        
        public int SchYearID { get; set; }
        public string Term { get; set; }
        public string Year { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public DateTime dateStartMidterm { get; set; }
        public DateTime dateEndMidterm { get; set; }
        public string StartMidterm { get; set; }
        public string EndMidterm { get; set; }
        public DateTime dateStartFinal { get; set; }
        public DateTime dateEndFinal { get; set; }
        public string StartFinal { get; set; }
        public string EndFinal { get; set; }
    }
}