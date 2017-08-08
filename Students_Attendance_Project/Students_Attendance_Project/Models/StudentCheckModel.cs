using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class StudentCheckModel
    {
        public int StdCheckID { get; set; }
        public int StdID { get; set; }
        public string StdCode { get; set; }
        public DateTime? DateCheck { get; set; }
        public int StudyGroupID { get; set; }
        public string Note { get; set; }
        public int? StatusID { get; set; }


        public string NameTH { get; set; }
        
        //public int UserID { get; set; }
        //public int ScheduleID { get; set; }
        //public int CompensateID { get; set; }
    }
    public class DataModel
    {
        public int StdCheckID { get; set; }
        public string StudyGroupID { get; set; }
        public string StdID { get; set; }
        public string StatusID { get; set; }
        public string StatusRe { get; set; }
        public string Note { get; set; }
        public DateTime? DateCheck { get; set; }
       
    }

    public class DateModel
    {
        public DateTime DateCheck { get; set; }
    }

    public class CompensateModel
    {
        public DateTime DateOrigin { get; set; }
        public string Note { get; set; }
        public int? ID { get; set; }
    }
    public class CompensateModel1
    {
        public string DateOrigin { get; set; }
        public string Note { get; set; }
        public int? ID { get; set; }
    }
}