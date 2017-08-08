using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Students_Attendance_Project.Models
{
    public class LoginModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserRoleID { get; set; }
        public string UserRoleName { get; set; }
    }

    interface ICustomPrincipal : IPrincipal
    {
        int UserID { get; set; }
        DateTime ExpDate { get; set; }
        string ClientInfo { get; set; }
    }

    public class CustomPrincipal : ICustomPrincipal
    {
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role) { return false; }

        public CustomPrincipal(string email)
        {
            this.Identity = new GenericIdentity(email);
        }
        public string Role { get; set; }
        public int UserID { get; set; }
        public DateTime ExpDate { get; set; }
        public string ClientInfo { get; set; }
    }

    public class DataLogin {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}