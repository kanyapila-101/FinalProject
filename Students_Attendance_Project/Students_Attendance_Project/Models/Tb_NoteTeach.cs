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
    
    public partial class Tb_NoteTeach
    {
        public int NoteID { get; set; }
        public string NoteName { get; set; }
        public System.DateTime DateNote { get; set; }
        public string DetailNote { get; set; }
        public int StudyGroupID { get; set; }
    
        public virtual Tb_StudyGroup Tb_StudyGroup { get; set; }
    }
}
