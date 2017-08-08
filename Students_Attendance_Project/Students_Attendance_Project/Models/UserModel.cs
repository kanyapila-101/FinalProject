using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Students_Attendance_Project.Models
{

    public class FilterModel
    {
        public int page { get; set; }
        public string Query { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Disable { get; set; }
        public FilterModel()
        {
            page = 1;
        }
    }
    public class UserMedel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public int DeptCode { get; set; }
        public string DeptName { get; set; }
        public string Role { get; set; }
    }
    public class RegisterModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Confirm { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public int DeptCode { get; set; }
        public string DeptName { get; set; }
        public string Role { get; set; }
    }
   

}