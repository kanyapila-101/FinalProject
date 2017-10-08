using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class TaskModel
    {
        public int TaskID { get; set; }
        public int Type { get; set; }
        public string TaskName { get; set; }
        public int FullScore { get; set; }
        public string Note { get; set; }
        public int StudyGroupID { get; set; }
        public string StudyGroupCode { get; set; }
        public int quantityGroup { get; set; }
    }

    public class SingleTaskModel
    {
        public int TaskID { get; set; }
        public int StdID { get; set; }
        public int FullScore { get; set; }
        public double? Score { get; set; }
        public string NameTH { get; set; }
        public string StdCode { get; set; }
    }

    public class GroupTaskModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int TaskID { get; set; }
        public int FullScore { get; set; }
        public int StudyGroupID { get; set; }
        public int StdID { get; set; }
        public int isSelect { get; set; }
        public string NameTH { get; set; }
        public double Score { get; set; }
    }

    public class GroupModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int TaskID { get; set; }
        public int StudyGroupID { get; set; }

    }

    public class AddGroupModel
    {
        public string GroupName { get; set; }
        public string valueKey { get; set; }
    }
}