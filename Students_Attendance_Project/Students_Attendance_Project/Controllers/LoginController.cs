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

namespace Students_Attendance_Project.Controllers
{
    public class LoginController : Controller
    {
        //acs
        public ActionResult ssoLogin() // acs
        {
            AppSettings appSettings = new AppSettings();
            OneLogin.Auth auth = new Auth(appSettings);
            var listData = new List<DataLogin>();
            auth.ProcessResponse();
            var res = string.Empty;
            var name = string.Empty;
            var value = string.Empty;

            var username = string.Empty;
            var gidNumber = string.Empty;
            var nameTHFull = string.Empty;
            var deptName = string.Empty;
            var nameENFull = string.Empty;
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
                        value = node.FirstChild.InnerText;
                        listData.Add(new DataLogin
                        {
                            Name = name,
                            Value = value
                        });
                        switch (name)
                        {
                            case "uid":
                                username = value;
                                break;
                            case "gidNumber":
                                gidNumber = value;
                                break;
                            case "firstNameThai":
                                nameTHFull = value;
                                break;
                            case "lasttNameThai":
                                nameTHFull += " " + value;
                                break;
                            case "program":
                                deptName = value;
                                break;
                            case "gecos":
                                nameENFull = value;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                // Login success but got invalid SAML information
                res = "SAML information is invalid!";
            }
            string identityId = HttpContext.Request.Url.Host + HttpContext.Request.Url.AbsolutePath;
            identityId = identityId.Substring(0, identityId.Length - 17);
            string url = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + HttpContext.Request.Url.AbsolutePath;
            url = url.Substring(0, url.Length - 13);
            ViewBag.DataLogin = listData.ToList();
            ViewBag.Res = res;
            ViewBag.identityId = identityId;
            ViewBag.url = url;
            //if (gidNumber == "2500")
            //{
            //    // insert to user table & login table
            //    using (var db = new Student_AttendanceEntities())
            //    {
            //        var datauser = db.Tb_User.Where(r => r.Username == username).FirstOrDefault();
            //        var deptcode = db.Tb_Department.Where(r => r.DeptName == deptName).Select(r => r.DeptCode).SingleOrDefault();
            //        if (datauser == null)
            //        {
            //            var user = new Tb_User()
            //            {
            //                Username = username,
            //                Name = nameTHFull,
            //                NameEN = nameENFull,
            //                DeptCode = deptcode,
            //                Password = null,
            //                Role = "user"
            //            };
            //            db.Tb_User.Add(user);
            //            db.SaveChanges();
            //        }
            //        else
            //        {
            //            var data = db.Tb_User.Where(r => r.Username == username).FirstOrDefault();
            //            if (data != null)
            //            {
            //                Session.Timeout = 15;
            //                Session["sessionID"] = HttpContext.Session.SessionID;
            //                var sessionid = Session["sessionID"].ToString();
            //                var userLogin = db.Tb_Login.Where(r => r.UserID == data.UserID).FirstOrDefault();
            //                if (userLogin == null)
            //                {
            //                    var addUser = new Tb_Login()
            //                    {
            //                        UserID = data.UserID,
            //                        sessionID = sessionid,
            //                        LoginTime = DateTime.Now
            //                    };
            //                    db.Tb_Login.Add(addUser);
            //                }
            //                else
            //                {
            //                    var user = db.Tb_Login.Where(r => r.UserID == data.UserID).FirstOrDefault();
            //                    if (user != null)
            //                    {
            //                        db.Tb_Login.Where(r => r.LoginID == user.LoginID).ForEach(r =>
            //                        {
            //                            r.sessionID = sessionid; // ให้คนมาทีหลัง เข้าใช้ คนเก่า ดีดออก
            //                            r.LoginTime = DateTime.Now;
            //                        });
            //                    }

            //                }
            //                db.SaveChanges();
            //                System.Web.Security.FormsAuthentication.SetAuthCookie(data.UserID.ToString(), false);
            //            }
            //        }
            //    }
            //    //return RedirectToAction("redirectIndex", "Login");
            //    return RedirectToAction("Index", "Home");
            //}
            //else
            //{
            //    ViewBag.notUse = "ขออภัย Username นี้ไม่สามารถใช้งานระบบนี้ได้";
            //    return View();
            //}
            return View();
        }

        //public ActionResult redirectIndex()
        //{
        //    if (UserLogon == null)
        //    {
        //        return RedirectToAction("Login", "Login");
        //    }
        //    else if (UserLogon != null && (UserLogon.Role.ToLower() == "admin" || UserLogon.Role.ToLower() == "superadmin"))
        //    {
        //        return RedirectToAction("Index", "Admin");
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //}

        public JsonResult LoginLoad(string username, string password) // เช็ค username password ในการ Login
        {
            var jsonReturn = new JsonResponse();
            //var sessionid = string.Empty;
            using (var db = new Student_AttendanceEntities())
            {
                var data = db.Tb_User.Where(r => r.Username == username && r.Password == password).SingleOrDefault();
                if (data != null)
                {
                    Session.Timeout = 15;
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

        //sso
        public ActionResult Login()
        {
            AppSettings appSettings = new AppSettings();
            OneLogin.Auth auth = new Auth(appSettings);

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

        } // หน้า Login เข้าสู่ระบบ sso

        //slo
        public ActionResult Logout() // ออกจากระบบ slo
        {
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
    }
}