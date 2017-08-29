using Students_Attendance_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Students_Attendance_Project
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                try
                {
                    //let us take out the username now                
                    HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    string userID = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                    int userid = int.Parse(userID);
                    var role = "";
                    CustomPrincipal newUser = new CustomPrincipal(authCookie.Value);
                    using (var db = new Student_AttendanceEntities())
                    {
                        role = db.Tb_User.Where(r => r.UserID == userid).Select(r => r.Role).FirstOrDefault();
                    }
                    newUser.UserID = userid;
                    newUser.Role = role;
                    newUser.ExpDate = DateTime.Now.AddMinutes(1);
                    HttpContext.Current.User = newUser;
                }
                catch (Exception)
                {
                    //something went wrong
                }
            }
        }

        /* protected void Application_BeginRequest()
        {
           Response.Cache.SetCacheability(HttpCacheability.NoCache);
           Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
           Response.Cache.SetNoStore();
        } */
    }
}
