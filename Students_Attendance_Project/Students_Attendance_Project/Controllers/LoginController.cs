using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Students_Attendance_Project.Models;
using WebGrease.Css.Extensions;
using PagedList;
using Students_Attendance_Project.Common;
using System.Web.Security;
using OneLogin;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;

namespace Students_Attendance_Project.Controllers
{
    public class LoginController : BaseController
    {
        //acs
        /// <summary>
        /// Response form SSO after login successful
        /// </summary>
        /// <returns></returns>
        public ActionResult acs()
        {
            AppSettings appSettings = new AppSettings();
            OneLogin.Auth auth = new Auth(appSettings);
            var listData = new List<DataLogin>();
            auth.ProcessResponse();
            var res = string.Empty;
            var name = string.Empty;
            var ssoValue = string.Empty;
            var valid = false;
            var username = string.Empty;
            var gidNumber = string.Empty;
            var firstname = string.Empty;
            var lastname = string.Empty;
            var deptName = string.Empty;
            var nameENFull = string.Empty;
            var email = string.Empty;
            if (auth.Response.IsValid())
            {
                // Login successful

                // Save SSO Name ID
                HttpContext.Session["ssoNameID"] = auth.Response.GetNameID();

                // Save SSO Session Index
                HttpContext.Session["ssoSessionIndex"] = auth.Response.GetSessionIndex();

                // Save Text of XML of local User Data to Session 
                HttpContext.Session["ssoUserData"] = auth.Response.GetAttributes();

                // Redirect to requested <URL> -- /?sso&redirect=<URL>
                if (Request.Form["RelayState"] != null)
                {
                    res = Request.Form["RelayState"].ToString();
                }
                if (HttpContext.Session["ssoUserData"] != null)
                {
                    XmlDocument userXmlDoc = new XmlDocument();
                    userXmlDoc.PreserveWhitespace = true;
                    userXmlDoc.XmlResolver = null;
                    userXmlDoc.LoadXml((string)HttpContext.Session["ssoUserData"]);

                    foreach (XmlNode node in userXmlDoc.FirstChild.ChildNodes)
                    {
                        name = node.Attributes["Name"].Value;
                        ssoValue = node.FirstChild.InnerText;
                        listData.Add(new DataLogin
                        {
                            Name = name,
                            Value = ssoValue
                        });
                        switch (name)
                        {
                            case "uid":
                                username = ssoValue;
                                break;
                            case "gidNumber":
                                gidNumber = ssoValue;
                                break;
                            case "firstNameThai":
                                firstname = ssoValue;
                                break;
                            case "lasttNameThai":
                                lastname = ssoValue;
                                break;
                            case "program":
                                deptName = ssoValue;
                                break;
                            case "gecos":
                                nameENFull = ssoValue;
                                break;
                            case "mail":
                                email = ssoValue;
                                break;
                        }
                    }
                }
            }
            else
            {
                // Login success but got invalid SAML information
                res = "SAML information is invalid!";
                valid = false;
            }
            string identityId = HttpContext.Request.Url.Host + HttpContext.Request.Url.AbsolutePath;
            identityId = identityId.Substring(0, identityId.Length - 10);
            string url = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + HttpContext.Request.Url.AbsolutePath;
            url = url.Substring(0, url.Length - 4);
            ViewBag.DataLogin = listData.ToList();
            ViewBag.Res = res;
            ViewBag.identityId = identityId;
            ViewBag.url = url;
            ViewBag.ssoValid = valid;
            //gidNumber = "2500";
            //username = "somsin";//
            //firstname = "สมสิน";
            //lastname = "วางขุนทด";
            //deptName = "";
            if (gidNumber == "2500")
            {
                // insert to user table & login table
                var nameth = firstname + " " + lastname;
                using (var db = new Student_AttendanceEntities())
                {
                    var datauser = db.Tb_User.Where(r => r.Username == username).FirstOrDefault();
                    var dataname = db.Tb_User.Where(r => r.Name == nameth).FirstOrDefault();
                    var deptcode = db.Tb_Department.Where(r => r.DeptName == deptName).Select(r => r.DeptCode).SingleOrDefault();
                    if (datauser == null)
                    {
                        if (dataname != null)
                        {
                            db.Tb_User.Where(r => r.UserID == dataname.UserID).ForEach(r =>
                            {
                                r.Username = username;
                                if (deptcode != 0)
                                {
                                    r.DeptCode = deptcode;
                                }
                            });
                        }
                        else
                        {
                            if (deptcode != 0)
                            {
                                var user = new Tb_User()
                                {
                                    Username = username,
                                    Name = nameth,
                                    DeptCode = deptcode,
                                    Password = "12345678",
                                    Role = "user",
                                    Email = email
                                };
                                db.Tb_User.Add(user);
                            }
                            else
                            {
                                var user = new Tb_User()
                                {
                                    Username = username,
                                    Name = nameth,
                                    Password = "12345678",
                                    Role = "user",
                                    Email = email
                                };
                                db.Tb_User.Add(user);
                            }
                        }
                        db.SaveChanges();
                    }
                    var data = db.Tb_User.Where(r => r.Username == username).FirstOrDefault();
                    if (data != null)
                    {
                        Session.Timeout = 20;
                        Session["sessionID"] = HttpContext.Session.SessionID;
                        var sessionid = Session["sessionID"].ToString();
                        var userLogin = db.Tb_Login.Where(r => r.UserID == data.UserID).FirstOrDefault();
                        if (userLogin == null)
                        {
                            var addUser = new Tb_Login()
                            {
                                UserID = data.UserID,
                                sessionID = sessionid,
                                LoginTime = DateTime.Now
                            };
                            db.Tb_Login.Add(addUser);
                        }
                        else
                        {
                            db.Tb_Login.Where(r => r.LoginID == userLogin.LoginID).ForEach(r =>
                            {
                                r.sessionID = sessionid; // ให้คนมาทีหลัง เข้าใช้ คนเก่า ดีดออก
                                r.LoginTime = DateTime.Now;
                            });
                        }
                        db.SaveChanges();
                        System.Web.Security.FormsAuthentication.SetAuthCookie(data.UserID.ToString(), false);
                    }
                }
                return RedirectToAction("redirectIndex", "Login");
                //return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.notUse = "ขออภัยบัญชีผู้ใช้ " + username + " นี้ไม่สามารถใช้งานระบบนี้ได้ เนื่องจากไม่ใช่ของอาจารย์ ";
                return View();
            }
            //return View();
        }

        //sso
        /// <summary>
        /// Make a request to SSO for login
        /// </summary>
        /// <returns></returns>
        public ActionResult redirectsso()
        {
            AppSettings appSettings = new AppSettings();
            OneLogin.Auth auth = new Auth(appSettings);
            Log.Error(Request.QueryString["redirect"].ToString());
            string redirect = "";
            if (Request.QueryString["redirect"] != null)
            {
                redirect = Request.QueryString["redirect"];
            }
            else if (Request.UrlReferrer != null)
            {
                redirect = Request.UrlReferrer.ToString();
            }
            if (redirect != "")
            {
                return Redirect(auth.Login(redirect));
            }
            else
            {
                return Redirect(auth.Login(""));
            }

        } // redirect to หน้า Login เข้าสู่ระบบ sso

        //slo
        /// <summary>
        /// Make a request to SSO for logout
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout() // ออกจากระบบ slo
        {
            if (UserLogon != null)
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var user = db.Tb_Login.Where(r => r.UserID == UserLogon.UserID).FirstOrDefault();
                    if (user != null)
                    {
                        db.Tb_Login.Remove(user);
                        db.SaveChanges();
                    }
                }

                Session.RemoveAll();
                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();
            }
            AppSettings appSettings = new AppSettings();
            OneLogin.Auth auth = new Auth(appSettings);
            HttpContext.Session["ssoUserData"] = null;

            string nameId = (string)HttpContext.Session["ssoNameID"];
            string sessionIndex = (string)HttpContext.Session["ssoSessionIndex"];

            string redirect = "";
            if (Request.QueryString["redirect"] != null)
            {
                redirect = Request.QueryString["redirect"];
            }
            else if (Request.UrlReferrer != null)
            {
                redirect = Request.UrlReferrer.ToString();
            }

            if (redirect != "")
            {
                return Redirect(auth.Logout(redirect, nameId, sessionIndex));
            }
            else
            {
                return Redirect(auth.Logout("", nameId, sessionIndex));
            }
        }

        //sls
        /// <summary>
        /// Response form SSO after logout
        /// </summary>
        /// <returns></returns>
        public ActionResult sls()
        {
            AppSettings appSettings = new AppSettings();
            OneLogin.Auth auth = new Auth(appSettings);
            if (Request.Form["SAMLResponse"] != null)
            {
                auth.ProcessResponse();

                if (auth.Response.IsValid())
                {
                    // Sucessfully logged out
                    // Destroy SSO Name ID
                    HttpContext.Session["ssoNameID"] = null;

                    // Destroy SSO Session Index
                    HttpContext.Session["ssoSessionIndex"] = null;

                    if (Request.Form["RelayState"] != null)
                    {
                        return Redirect(Request.Form["RelayState"]);
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //login

        /// <summary>
        /// Request view Login Internal System
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            if (UserLogon != null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                //using (var db = new Student_AttendanceEntities())
                //{
                //    ViewBag.Department = db.Tb_Department.Select(r => new DepartmentModel { DeptCode = r.DeptCode, DeptName = r.DeptName }).ToList();
                //}
                return View();
            }
        }

        //sso
        /// <summary>
        /// Make a request to SSO for login
        /// </summary>
        /// <returns></returns>
        //public ActionResult Login()
        //{
        //    AppSettings appSettings = new AppSettings();
        //    OneLogin.Auth auth = new Auth(appSettings);

        //    string redirect = "";
        //    if (Request.QueryString["redirect"] != null)
        //    {
        //        redirect = Request.QueryString["redirect"];
        //    }
        //    else if (Request.UrlReferrer != null)
        //    {
        //        redirect = Request.UrlReferrer.ToString();
        //    }
        //    if (redirect != "")
        //    {
        //        return Redirect(auth.Login(redirect));
        //    }
        //    else
        //    {
        //        return Redirect(auth.Login(""));
        //    }
        //} // หน้า Login เข้าสู่ระบบ sso

        /// <summary>
        /// Response form SSO after logout
        /// </summary>
        /// <returns></returns>

        public ActionResult redirectIndex()
        {
            if (UserLogon == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else if (UserLogon != null && (UserLogon.Role.ToLower() == "admin" || UserLogon.Role.ToLower() == "superadmin"))
            {
                if (CheckInternetConnectByPingGoogle() == true)
                {
                    LineNotify(UserLogon.Name);
                }
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                if (CheckInternetConnectByPingGoogle() == true)
                {
                    LineNotify(UserLogon.Name);
                }
                return RedirectToAction("Index", "Home");
            }
        }

        public JsonResult LoginLoad(string username, string password) // เช็ค email password ในการ Login
        {
            var jsonReturn = new JsonResponse();
            //var sessionid = string.Empty;
            using (var db = new Student_AttendanceEntities())
            {
                var data = db.Tb_User.Where(r => r.Email == username && r.Password == password).SingleOrDefault();
                if (data != null)
                {
                    Session.Timeout = 20;
                    Session["sessionID"] = HttpContext.Session.SessionID;
                    var sessionid = Session["sessionID"].ToString();
                    var userLogin = db.Tb_Login.Where(r => r.UserID == data.UserID).FirstOrDefault();
                    if (userLogin == null)
                    {
                        var addUser = new Tb_Login()
                        {
                            UserID = data.UserID,
                            sessionID = sessionid,
                            LoginTime = DateTime.Now
                        };
                        db.Tb_Login.Add(addUser);
                    }
                    else
                    {
                        var user = db.Tb_Login.Where(r => r.UserID == data.UserID).FirstOrDefault();
                        if (user != null)
                        {
                            db.Tb_Login.Where(r => r.LoginID == user.LoginID).ForEach(r =>
                            {
                                r.sessionID = sessionid; // ให้คนมาทีหลัง เข้าใช้ คนเก่า ดีดออก
                                r.LoginTime = DateTime.Now;
                            });
                        }

                    }
                    jsonReturn = new JsonResponse { status = true, data = "success" };
                    db.SaveChanges();
                    System.Web.Security.FormsAuthentication.SetAuthCookie(data.UserID.ToString(), false);
                }
                else
                {
                    jsonReturn = new JsonResponse { status = false, data = "fail" };
                }
            }
            return Json(jsonReturn);
        }

        public void LineNotify(string name)
        {
            var access_token = "8khk7Db7ygUyNnNF7hDoYSOQmSNF8eraGpbuU0eubJQ";
            var time = DateTime.Now.ToString("H:mm:ss");
            var date = DateTime.Now.ToString("dd-MM-yyyy", Shared.CultureInfoTh);
            var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
            var postData = string.Format("message={0}", "แจ้งเตือน!! อาจารย์" + name + " เข้าสู่ระบบเช็คชื่อการเข้าเรียนของนักศึกษาออนไลน์ เมื่อวันที่ " + date + " เวลา " + time + " น.");
            var data = Encoding.UTF8.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.Headers.Add("Authorization", "Bearer " + access_token);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        private static bool CheckInternetConnectByPingGoogle()
        {
            const int timeout = 3000; // 5 sec
            const string host = "google.com";

            var ping = new Ping();
            var buffer = new byte[32];
            var pingOptions = new PingOptions();

            try
            {
                var reply = ping.Send(host, timeout, buffer, pingOptions);
                return (reply != null && reply.Status == IPStatus.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
