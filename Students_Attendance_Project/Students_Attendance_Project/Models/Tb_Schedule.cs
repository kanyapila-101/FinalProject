
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Students_Attendance_Project.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Tb_Schedule
{

    public int ScheduleID { get; set; }

    public string DayTeach { get; set; }

    public int StudyGroupID { get; set; }

    public string SubjectCode { get; set; }

    public string Course { get; set; }

    public string StartTime { get; set; }

    public string EndTime { get; set; }

    public string BuildingCode { get; set; }

    public string RoomNo { get; set; }

    public int UserID { get; set; }

    public int SchYearID { get; set; }

    public Nullable<int> StartTimeInt { get; set; }

    public Nullable<int> EndTimeInt { get; set; }

    public Nullable<int> TotalHour { get; set; }

    public int TypeSubject { get; set; }



    public virtual Tb_Room Tb_Room { get; set; }

    public virtual Tb_SchoolYear Tb_SchoolYear { get; set; }

    public virtual Tb_StudyGroup Tb_StudyGroup { get; set; }

    public virtual Tb_User Tb_User { get; set; }

}

}
