using ServiceStack.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Students_Attendance_Project.Models;

namespace Students_Attendance_Project.Common
{
    public static class Shared
    {
        public class CustomAuthorizeAttribute : AuthorizeAttribute
        {
            //public override void OnAuthorization(AuthorizationContext filterContext)
            //{
            //    base.OnAuthorization(filterContext);
            //    if (filterContext.Result is HttpUnauthorizedResult)
            //    {
            //        filterContext.Result = new RedirectResult("~/Home/Error");
            //    }
            //}
        }

        public static CultureInfo CultureInfo
        {
            
            get { return new CultureInfo("en-US"); }
        }
        
        public static CultureInfo CultureInfoTh
        {
            get { return new CultureInfo("th-TH"); }
        }

        public static Tb_User getUserLogon() // ส่งค่า ข้อมูลผู้ใช้ไปแสดงผลที่ Layout
        {
            Tb_User Userlogon = null;
            var x = HttpContext.Current.User as CustomPrincipal;
            int userid = 0;
            if (x != null)
            {
                userid = x.UserID;
            }
            using (var db = new Student_AttendanceEntities()) {
                Userlogon = db.Tb_User.Where(r => r.UserID == userid).FirstOrDefault();
            }
            return Userlogon;
        }
    }
}