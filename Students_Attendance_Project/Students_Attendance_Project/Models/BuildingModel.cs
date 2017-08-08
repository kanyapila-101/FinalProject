using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class BuildingModel
    {
        public int NewBuilding { get; set; }
        public string BuildingCode { get; set; }
        public string BuildingName { get; set; }
        public string RoomNo { get; set; }
    }

    public class RoomModel
    {
        public int NewRoom { get; set; }
        public string BuildingCode { get; set; }
        public string BuildingName { get; set; }
        public string RoomNo { get; set; }
    }
}