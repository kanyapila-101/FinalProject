using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Students_Attendance_Project.Models
{
    public class NoteTeachModel
    {
        public int NoteID { get; set; }
        public string NoteName { get; set; }
        public System.DateTime DateNote { get; set; }
        public string DetailNote { get; set; }
        public int StudyGroupID { get; set; }
    }

    public class ShowNoteModel
    {
        public int NoteID { get; set; }
        public string NoteName { get; set; }
        public System.DateTime DateNote { get; set; }
        public string DetailNote { get; set; }
        public int StudyGroupID { get; set; }
        public string dateNote { get; set; }
    }
}