using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class Json
    {

    }

    public class JsonResponse
    {
        public bool status { get; set; }
        public object data { get; set; }
        public string message { get; set; }
    }
}