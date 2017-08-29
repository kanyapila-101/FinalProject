using Students_Attendance_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace Students_Attendance_Project.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        public int PageItemLimit = 30;
        public Tb_User UserLogon = null;
        public Tb_Login UserIsLogin = null;
        public string MainUrl = string.Empty;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            MainUrl = ConfigurationManager.AppSettings["MainUrl"];
            base.Initialize(requestContext);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userReq = System.Web.HttpContext.Current.User as CustomPrincipal;
            int userid = userReq != null ? userReq.UserID : 0;
            string controller = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString().ToLower();
            string action = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString().ToLower();
            var actionlist = new string[] { "login", "loginload", "registersave", "acs", "sls", "redirectsso" };
            if (!actionlist.Contains(action))
            {
                using (var db = new Student_AttendanceEntities())
                {
                    UserLogon = db.Tb_User.Where(r => r.UserID == userid).FirstOrDefault();
                    UserIsLogin = db.Tb_Login.Where(r => r.UserID == userid).FirstOrDefault();
                }
                
                Session.Timeout = 20;
                Session["sessionID"] = HttpContext.Session.SessionID;
                var sessionid = Session["sessionID"].ToString();
                if (UserLogon == null || UserIsLogin.sessionID != sessionid)
                {
                    //string urlLogout = "http://cpe.rmuti.ac.th/project/StudentAttendance/Login/Logout";
                    //filterContext.Result = new RedirectResult("~/Login/Login"); 
                    filterContext.Result = new RedirectResult(MainUrl);
                }
                else
                {
                    if (UserLogon.Role == "user" && controller.ToLower() == "admin")
                    {
                        filterContext.Result = new RedirectResult(MainUrl + "/Home/index");
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }

    }
}