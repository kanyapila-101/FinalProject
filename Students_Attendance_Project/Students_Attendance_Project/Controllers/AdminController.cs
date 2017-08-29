using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Students_Attendance_Project.Models;
using WebGrease.Css.Extensions;
using PagedList;
using Students_Attendance_Project.Common;


namespace Students_Attendance_Project.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _Profile()
        {
            return View();
        }

        // User Management CRUD LINQ TO SQL

        public ActionResult _User(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {   
                // inner join
                //var data = (from r in db.Tb_User
                //            join d in db.Tb_Department on r.DeptCode.Value equals d.DeptCode
                //            where (string.IsNullOrEmpty(model.Query) || r.Username.Contains(model.Query) || r.Name.Contains(model.Query) || r.Email.Contains(model.Query))
                //            orderby r.UserID
                //            select new UserMedel
                //            {
                //                UserID = r.UserID,
                //                Username = r.Username,
                //                Password = r.Password,
                //                Name = r.Name,
                //                NameEN = r.NameEN,
                //                DeptCode = r.DeptCode.Value,
                //                DeptName = d.DeptName,
                //                Role = r.Role,
                //                Email = r.Email
                //            }).ToPagedList(model.page, 20);

                // left outer join
                var data = (from r in db.Tb_User
                            join d in db.Tb_Department on r.DeptCode equals d.DeptCode into left
                            from j in left.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(model.Query) || r.Username.Contains(model.Query) || r.Name.Contains(model.Query) || r.Email.Contains(model.Query))
                            orderby r.UserID
                            select new UserMedel
                            {
                                UserID = r.UserID,
                                Username = r.Username,
                                Password = r.Password,
                                Name = r.Name,
                                Role = r.Role,
                                Email = r.Email,
                                DeptCode = j.DeptCode,
                                DeptName = j.DeptName
                            }).ToPagedList(model.page, 30);
                ViewBag.DetailUser = data;

                ViewBag.department = (from r in db.Tb_Department
                                      orderby r.DeptCode
                                      select new DepartmentModel
                                      {
                                          DeptCode = r.DeptCode,
                                          DeptName = r.DeptName
                                      }).ToList();
            }
            return View(model);
        }

        public JsonResult SaveUser(RegisterModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.UserID == 0) //เมื่อคลิก ปุ่มเพิ่มผู้ใช้ ID = 0 คือจะเป็น default ID ของตารางจะไม่ถูกเก็บจนกว่าจะ submit จะทำการเพิ่มลงตารางในเงื่อนไขนี้ เช็คเพื่อ เพิ่ม หรือ แก้ไข
                    {
                        var user1 = db.Tb_User.Where(r => r.Username == model.Username).FirstOrDefault();
                        if (user1 == null)
                        {
                            db.Tb_User.Add(new Tb_User()
                            {
                                Username = model.Username,
                                Password = model.Password,
                                Name = model.Name,
                                DeptCode = model.DeptCode,
                                Role = model.Role,
                                Email = model.Email
                            });
                            db.SaveChanges();
                            jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเเล้ว" };
                        }
                        else
                        {
                            jsonReturn = new JsonResponse { status = false, message = "Username ซ้ำ!!" };
                        }

                    }
                    else // เมื่อคลิกปุ่มแก้ไข จะเลือกเอา ID นั้นมา แก้ไข ใน esle นี้
                    {
                        var data = db.Tb_User.Where(r => r.UserID == model.UserID).FirstOrDefault();
                        if (data != null)
                        {
                            //var id = db.Tb_UserRole.Where(r => r.UserRoleName == "Admin").Select(r => r.UserRoleID).FirstOrDefault();
                            db.Tb_User.Where(r => r.UserID == model.UserID).ForEach(r =>
                            {
                                r.DeptCode = model.DeptCode;
                                r.Name = model.Name;
                                r.Role = model.Role;
                            });
                            db.SaveChanges();
                            jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเเล้ว" };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // error do something
                var str = (string.IsNullOrEmpty(ex.InnerException.ToString())) ? ex.Message : ex.InnerException.ToString();
                jsonReturn = new JsonResponse { status = false, message = "เกิดข้อผิดพลาด : " + str };
            }

            return Json(jsonReturn);
        }

        public JsonResult UpdateUser(int id = 0)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_User.Where(r => r.UserID == id).Select(r => new
                    {
                        r.Username,
                        r.Password,
                        r.Name,
                        r.NameEN,
                        r.DeptCode,
                        r.Role,
                        r.Email
                    }).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = "success", data = data };
                    }

                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }

            return Json(jsonReturn);
        }

        public JsonResult DeleteUser(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_User.Where(r => r.UserID == id).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        db.Tb_User.Remove(data);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "success" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        // SchoolYear Management CRUD LINQ TO SQL

        public JsonResult SaveSchoolYear(SchoolYearModel model)
        {
            var jsonReturn = new JsonResponse();
            model.dateStart = DateTime.ParseExact(model.StartDate, "dd/MM/yyyy", Shared.CultureInfoTh);
            model.dateEnd = DateTime.ParseExact(model.EndDate, "dd/MM/yyyy", Shared.CultureInfoTh);
            model.dateStartMidterm = DateTime.ParseExact(model.StartMidterm, "dd/MM/yyyy", Shared.CultureInfoTh);
            model.dateEndMidterm = DateTime.ParseExact(model.EndMidterm, "dd/MM/yyyy", Shared.CultureInfoTh);
            model.dateStartFinal = DateTime.ParseExact(model.StartFinal, "dd/MM/yyyy", Shared.CultureInfoTh);
            model.dateEndFinal = DateTime.ParseExact(model.EndFinal, "dd/MM/yyyy", Shared.CultureInfoTh);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.SchYearID == 0)  //เมื่อคลิก ปุ่มเพิ่มผู้ใช้ ID = 0 คือจะเป็น default ID ของตารางจะไม่ถูกเก็บจนกว่าจะ submit จะทำการเพิ่มลงตารางในเงื่อนไขนี้ เช็คเพื่อ เพิ่ม หรือ แก้ไข
                    {
                        var data = new Tb_SchoolYear()
                        {
                            Term = model.Term,
                            Year = model.Year,
                            StartDate = model.dateStart,
                            EndDate = model.dateEnd,
                            StartMidterm = model.dateStartMidterm,
                            EndMidterm = model.dateEndMidterm,
                            StartFinal = model.dateStartFinal,
                            EndFinal = model.dateEndFinal
                        };
                        db.Tb_SchoolYear.Add(data);
                        db.SaveChanges();
                    }
                    else // เมื่อคลิกปุ่มแก้ไข จะเลือกเอา ID นั้นมา แก้ไข ใน else นี้
                    {
                        var data = db.Tb_SchoolYear.Where(r => r.SchYearID == model.SchYearID).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_SchoolYear.Where(r => r.SchYearID == model.SchYearID).ForEach(r =>
                            {
                                r.Term = model.Term;
                                r.Year = model.Year;
                                r.StartDate = model.dateStart;
                                r.EndDate = model.dateEnd;
                                r.StartMidterm = model.dateStartMidterm;
                                r.EndMidterm = model.dateEndMidterm;
                                r.StartFinal = model.dateStartFinal;
                                r.EndFinal = model.dateEndFinal;
                            });
                            db.SaveChanges();
                        }
                    }
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                }
            }
            catch (Exception ex)
            {
                var str = (string.IsNullOrEmpty(ex.InnerException.ToString())) ? ex.Message : ex.InnerException.ToString();
                jsonReturn = new JsonResponse { status = false, message = str };
            }

            return Json(jsonReturn);
        }

        public JsonResult UpdateSchYear(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from t in db.Tb_SchoolYear
                                where (t.SchYearID == id)
                                select new SchoolYearModel
                                {
                                    SchYearID = t.SchYearID,
                                    Term = t.Term,
                                    Year = t.Year,
                                    StartDate = "",
                                    EndDate = "",
                                    dateStart = t.StartDate,
                                    dateEnd = t.EndDate,
                                    StartMidterm = "",
                                    EndMidterm = "",
                                    dateStartMidterm = t.StartMidterm,
                                    dateEndMidterm = t.EndMidterm,
                                    StartFinal = "",
                                    EndFinal = "",
                                    dateStartFinal = t.StartFinal,
                                    dateEndFinal = t.EndFinal
                                }).FirstOrDefault();
                    data.StartDate = data.dateStart.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                    data.EndDate = data.dateEnd.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                    data.StartMidterm = data.dateStartMidterm.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                    data.EndMidterm = data.dateEndMidterm.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                    data.StartFinal = data.dateStartFinal.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                    data.EndFinal = data.dateEndFinal.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = "success", data = data };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteSchoolYear(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_SchoolYear.Where(r => r.SchYearID == id).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        db.Tb_SchoolYear.Remove(data);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "success" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public ActionResult SchoolYear(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from r in db.Tb_SchoolYear
                            where (string.IsNullOrEmpty(model.Query) || r.Term.Contains(model.Query) || r.Year.Contains(model.Query))
                            orderby r.Year descending, r.Term ascending
                            select new SchoolYearModel()
                            {
                                SchYearID = r.SchYearID,
                                Term = r.Term,
                                Year = r.Year,
                                dateStart = r.StartDate,
                                dateEnd = r.EndDate,
                                dateStartMidterm = r.StartMidterm,
                                dateEndMidterm = r.EndMidterm,
                                dateStartFinal = r.StartFinal,
                                dateEndFinal = r.EndFinal
                            }).ToPagedList(model.page, 10);
                ViewBag.SchoolYear = data;
            }
            return View(model);
        }

        public ActionResult SchoolYearTest(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from r in db.Tb_SchoolYear
                            where (string.IsNullOrEmpty(model.Query) || r.Term.Contains(model.Query) || r.Year.Contains(model.Query))
                            orderby r.Year, r.Term
                            select new SchoolYearModel()
                            {
                                SchYearID = r.SchYearID,
                                Term = r.Term,
                                Year = r.Year,
                                dateStart = r.StartDate,
                                dateEnd = r.EndDate
                            }).ToPagedList(model.page, 10);
                ViewBag.SchoolYear = data;
            }
            return View(model);
        }

        // Faculty Management CRUD LINQ TO SQL

        public ActionResult Faculty(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from t in db.Tb_Faculty
                            where (string.IsNullOrEmpty(model.Query) || t.FacultyName.Contains(model.Query))
                            orderby t.FacultyCode, t.FacultyName
                            select new FacultyModel()
                            {
                                FacultyCode = t.FacultyCode,
                                FacultyName = t.FacultyName
                            }).ToPagedList(model.page, 10);
                ViewBag.Faculty = data;
            }
            return View(model);
        }

        public JsonResult SaveFaculty(FacultyModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.CheckAddORupdate != 1)
                    {
                        db.Tb_Faculty.Add(new Tb_Faculty()
                        {
                            FacultyCode = model.FacultyCode,
                            FacultyName = model.FacultyName
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        var data = db.Tb_Faculty.Where(r => r.FacultyCode == model.FacultyCode).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Faculty.Where(r => r.FacultyCode == model.FacultyCode).ForEach(r =>
                            {
                                r.FacultyName = model.FacultyName;
                            });
                            db.SaveChanges();
                        }
                    }
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateFaculty(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Faculty.Where(r => r.FacultyCode == id).Select(r => new
                    {
                        r.FacultyCode,
                        r.FacultyName
                    }).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = "success", data = data };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteFaculty(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Faculty.Where(r => r.FacultyCode == id).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        db.Tb_Faculty.Remove(data);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "success" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        // Department Management CRUD LINQ TO SQL

        public ActionResult Department(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from t in db.Tb_Department
                            where (string.IsNullOrEmpty(model.Query) || t.DeptName.Contains(model.Query) || t.ShortName.Contains(model.Query))
                            orderby t.FacultyCode, t.DeptCode
                            select new DepartmentModel()
                            {
                                DeptCode = t.DeptCode,
                                DeptName = t.DeptName,
                                ShortName = t.ShortName,
                                FacultyCode = t.FacultyCode
                            }).ToPagedList(model.page, 20);
                ViewBag.Department = data;

                var dataFac = (from tf in db.Tb_Faculty
                                   //join tf in db.Tb_Faculty on td.FacultyID equals tf.FacultyID
                               select new DepartmentModel()
                               {
                                   FacultyCode = tf.FacultyCode,
                                   FacultyName = tf.FacultyName
                               }).ToList();
                ViewBag.dataFaculty = dataFac;
            }
            return View(model);
        }

        public JsonResult SaveDepartment(DepartmentModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.DeptID != 1)
                    {
                        db.Tb_Department.Add(new Tb_Department
                        {
                            DeptCode = model.DeptCode,
                            DeptName = model.DeptName,
                            ShortName = model.ShortName,
                            FacultyCode = model.FacultyCode
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        var data = db.Tb_Department.Where(r => r.DeptCode == model.DeptCode).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Department.Where(r => r.DeptCode == model.DeptCode).ForEach(r =>
                            {
                                r.DeptName = model.DeptName;
                                r.ShortName = model.ShortName;
                            });
                            db.SaveChanges();
                        }
                    }
                    jsonReturn = new JsonResponse { status = true, message = "success" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message.ToString() };
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateDepartment(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Department.Where(r => r.DeptCode == id).Select(r => new
                    {
                        r.DeptCode,
                        r.DeptName,
                        r.ShortName,
                        r.FacultyCode
                    }).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = "success", data = data };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteDepartment(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Department.Where(r => r.DeptCode == id).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        db.Tb_Department.Remove(data);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "success" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        // Subject Management CRUD LINQ TO SQL

        public ActionResult Subject(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from t in db.Tb_Subject
                            where (string.IsNullOrEmpty(model.Query) || t.SubjectCode.Contains(model.Query) || t.SubjectName.Contains(model.Query) || t.SubjectNameEN.Contains(model.Query))
                            orderby t.SubjectCode, t.Course
                            select new SubjectModel
                            {
                                SubjectCode = t.SubjectCode,
                                Course = t.Course,
                                SubjectName = t.SubjectName,
                                SubjectNameEN = t.SubjectNameEN,
                                Condition = t.Condition,
                                SubjectTheory = (int)(t.SubjectTheory),
                                TimeTheory = (int)(t.TimeTheory),
                                SubjectPractice = (int)(t.SubjectPractice),
                                TimePractice = (int)(t.TimePractice)
                            }).ToPagedList(model.page, 20);
                ViewBag.Subject = data;
            }
            return View(model);
        }

        public JsonResult UpdateSubject(string id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Subject.Where(r => r.SubjectCode == id).Select(r => new
                    {
                        r.SubjectCode,
                        r.Course,
                        r.SubjectName,
                        r.SubjectNameEN,
                        r.Condition,
                        r.SubjectTheory,
                        r.TimeTheory,
                        r.SubjectPractice,
                        r.TimePractice
                    }).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = true, message = "fail" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = "success", data = data };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "เกิดข้อผิดพลาด " + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult SaveSubject(SubjectModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.CheckAddORupdate != 1)
                    {
                        db.Tb_Subject.Add(new Tb_Subject
                        {
                            SubjectCode = model.SubjectCode,
                            Course = model.Course,
                            SubjectName = model.SubjectName,
                            SubjectNameEN = model.SubjectNameEN,
                            Condition = (double)model.Condition,
                            SubjectTheory = model.SubjectTheory,
                            TimeTheory = model.SubjectTheory * 1, // ชม. ทฤษฎี จำนวนหน่วยกิต * 1
                            SubjectPractice = model.SubjectPractice,
                            TimePractice = model.SubjectPractice * 3 // ชม. ปฏิบัติ จำนวนหน่วยกิต * 3
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        var data = db.Tb_Subject.Where(r => r.SubjectCode == model.SubjectCode).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Subject.Where(r => r.SubjectCode == model.SubjectCode).ForEach(r =>
                            {
                                r.SubjectCode = model.SubjectCode;
                                r.Course = model.Course;
                                r.SubjectName = model.SubjectName;
                                r.SubjectNameEN = model.SubjectNameEN;
                                r.Condition = model.Condition;
                                r.SubjectTheory = model.SubjectTheory;
                                r.TimeTheory = model.SubjectTheory * 1;
                                r.SubjectPractice = model.SubjectPractice;
                                r.TimePractice = model.SubjectPractice * 3;
                            });
                            db.SaveChanges();
                        }
                    }
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเเล้ว" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "เกิดข้อผิดพลาด " + ex.Message };
            }

            return Json(jsonReturn);
        }

        public JsonResult DeleteSubject(string id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    //var data = from r in db.Tb_Subject where r.SubjectID == id && r.Course == course1
                    //           select r;
                    var query = db.Tb_Subject.Where(r => r.SubjectCode == id).FirstOrDefault();
                    if (query == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        db.Tb_Subject.Remove(query);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "success" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "fail" + ex.Message };
            }
            return Json(jsonReturn);
        }

        // Building Management CRUD LINQ TO SQL

        public ActionResult Building(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from b in db.Tb_Building
                            where (string.IsNullOrEmpty(model.Query) || b.BuildingCode.Contains(model.Query) || b.BuildingName.Contains(model.Query))
                            orderby b.BuildingCode
                            select new BuildingModel
                            {
                                BuildingCode = b.BuildingCode,
                                BuildingName = b.BuildingName
                            }).ToPagedList(model.page, 50);
                ViewBag.Building = data;
            }
            return View(model);
        }

        public JsonResult SaveBuilding(BuildingModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.NewBuilding != 1)
                    {
                        var data = new Tb_Building()
                        {
                            BuildingCode = model.BuildingCode,
                            BuildingName = model.BuildingName
                        };
                        db.Tb_Building.Add(data);
                        db.SaveChanges();
                    }
                    else
                    {
                        var data = db.Tb_Building.Where(r => r.BuildingCode == model.BuildingCode).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Building.Where(r => r.BuildingCode == model.BuildingCode).ForEach(r =>
                            {
                                r.BuildingCode = model.BuildingCode;
                                r.BuildingName = model.BuildingName;
                            });
                            db.SaveChanges();
                        }
                    }
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเเล้ว" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateBuilding(string _id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Building.Where(r => r.BuildingCode == _id).Select(r => new
                    {
                        r.BuildingCode,
                        r.BuildingName
                    }).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, data = data };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteBuilding(string _id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var query = db.Tb_Building.Where(r => r.BuildingCode == _id).FirstOrDefault();
                    if (query == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        db.Tb_Building.Remove(query);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "success" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckBuildingCode(string _id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Building.Where(r => r.BuildingCode == _id).Select(r => r.BuildingCode).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult getBuildingRoom(FilterModel model, string id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from b in db.Tb_Building
                                join r in db.Tb_Room on b.BuildingCode equals r.BuildingCode
                                where b.BuildingCode == id
                                orderby b.BuildingCode
                                select new BuildingModel()
                                {
                                    BuildingCode = b.BuildingCode,
                                    BuildingName = b.BuildingName,
                                    RoomNo = r.RoomNo
                                }).ToPagedList(model.page, 60);

                    if (data != null)
                    {
                        int d = data.Count;
                        jsonReturn = new JsonResponse { status = true, data = data };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }

            return Json(jsonReturn);
        }

        // Room Management CRUD LINQ TO SQL

        public ActionResult Room(FilterModel model, string id)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data1 = (from b in db.Tb_Building
                             select new BuildingModel
                             {
                                 BuildingCode = b.BuildingCode,
                                 BuildingName = b.BuildingName
                             }).ToList();
                ViewBag.Building = data1;

                if (model.Query == null)
                {
                    var data2 = (from r in db.Tb_Room
                                 join b in db.Tb_Building on r.BuildingCode equals b.BuildingCode
                                 //where (string.IsNullOrEmpty(model.Query) || b.BuildingCode.Contains(model.Query))
                                 orderby b.BuildingCode, r.RoomNo
                                 select new RoomModel
                                 {
                                     BuildingCode = b.BuildingCode,
                                     BuildingName = b.BuildingName,
                                     RoomNo = r.RoomNo
                                 }).ToPagedList(model.page, 30);
                    ViewBag.BuildingRoom = data2;

                    ViewBag.NameBuilding = data2.ToList();
                }
                else
                {
                    var data2 = (from r in db.Tb_Room
                                 join b in db.Tb_Building on r.BuildingCode equals b.BuildingCode
                                 where b.BuildingCode == model.Query
                                 orderby b.BuildingCode, r.RoomNo
                                 select new RoomModel
                                 {
                                     BuildingCode = b.BuildingCode,
                                     BuildingName = b.BuildingName,
                                     RoomNo = r.RoomNo
                                 }).ToPagedList(model.page, 30);
                    ViewBag.BuildingRoom = data2;

                    ViewBag.NameBuilding = data2.ToList();
                }
            }
            return View(model);
        }

        public JsonResult SaveRoom(RoomModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.NewRoom != 1)
                    {
                        var data = new Tb_Room()
                        {
                            RoomNo = model.RoomNo,
                            BuildingCode = model.BuildingCode
                        };
                        db.Tb_Room.Add(data);
                        db.SaveChanges();
                    }
                    else
                    {
                        var data = db.Tb_Room.Where(r => r.RoomNo == model.RoomNo).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Room.Where(r => r.RoomNo == model.RoomNo).ForEach(r =>
                            {
                                r.RoomNo = model.RoomNo;
                                r.BuildingCode = model.BuildingCode;
                            });
                            db.SaveChanges();
                        }
                    }
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้ลมูลเรียบร้อยแล้ว" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateRoom(string _id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Room.Where(r => r.RoomNo == _id).Select(r => new
                    {
                        r.RoomNo,
                        r.BuildingCode
                    }).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, data = data };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteRoom(string _id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var query = db.Tb_Room.Where(r => r.RoomNo == _id).FirstOrDefault();
                    if (query == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail Data is null" };
                    }
                    else
                    {
                        db.Tb_Room.Remove(query);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "ลบข้อมูลเรียบร้อยแล้ว" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckRoomNo(string _data)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _data.Split(',');
                string roomNo = str[0];
                string buildingCode = str[1];
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Room.Where(r => r.RoomNo == roomNo && r.BuildingCode == buildingCode).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public ActionResult HolidayCalendar()
        {
            return View();
        }

        // Holiday Management CRUD LINQ TO SQL

        public ActionResult Holiday(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from r in db.Tb_Holiday
                            where (string.IsNullOrEmpty(model.Query) || r.HolidayName.Contains(model.Query))
                            orderby r.HolidayDate
                            select new HolidayModel()
                            {
                                HolidayID = r.HolidayID,
                                HolidayDate = r.HolidayDate,
                                HolidayName = r.HolidayName
                            }).ToPagedList(model.page, 20);
                ViewBag.Holiday = data;
            }
            return View(model);
        }

        public JsonResult CheckHolidayDate(string _date)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                DateTime date = DateTime.ParseExact(_date, "dd/MM/yyyy", Shared.CultureInfoTh);
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Holiday.Where(r => r.HolidayDate == date).Select(r => r.HolidayDate).FirstOrDefault();
                    if (data.Date.ToShortDateString() != date.ToShortDateString())
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult SaveHoliday(HolidayModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                model.HolidayDate = DateTime.ParseExact(model.dataDate, "dd/MM/yyyy", Shared.CultureInfoTh);
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.HolidayID == 0)
                    {
                        var data = new Tb_Holiday
                        {
                            HolidayDate = model.HolidayDate,
                            HolidayName = model.HolidayName
                        };
                        db.Tb_Holiday.Add(data);
                    }
                    else
                    {
                        var data = db.Tb_Holiday.Where(r => r.HolidayID == model.HolidayID).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Holiday.Where(r => r.HolidayID == model.HolidayID).ForEach(r =>
                            {
                                r.HolidayID = model.HolidayID;
                                r.HolidayDate = model.HolidayDate;
                                r.HolidayName = model.HolidayName;
                            });
                        }
                    }
                    db.SaveChanges();
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateHoliday(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Holiday.Where(r => r.HolidayID == id).Select(r => new HolidayModel()
                    {
                        HolidayID = r.HolidayID,
                        HolidayDate = r.HolidayDate,
                        dataDate = "",
                        HolidayName = r.HolidayName
                    }).FirstOrDefault();
                    data.dataDate = data.HolidayDate.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = true, data = data };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ไม่พบข้อมูล" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteHoliday(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Holiday.Where(r => r.HolidayID == id).FirstOrDefault();
                    if (data != null)
                    {
                        db.Tb_Holiday.Remove(data);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "ลบข้อมูลเรียบร้อยเเล้ว" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ไม่พบข้อมูลที่ต้องการลบ" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        // Check Validation Form Input

        public JsonResult CheckUsername(string _username)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_User.Where(r => r.Username == _username).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = _username + "นี้ มีผู้ใช้งานแล้ว กรุณาป้อนใหม่" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = _username + " นี้ สามารถใช้งานได้" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckEmail(string _email)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_User.Where(r => r.Email == _email).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = _email + "นี้ มีผู้ใช้งานแล้ว กรุณาป้อนใหม่" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = _email + " นี้ สามารถใช้งานได้" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckFacultyCode(string _facultyCode)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                int _facultyCodeInt = int.Parse(_facultyCode);
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Faculty.Where(r => r.FacultyCode == _facultyCodeInt).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = _facultyCodeInt + "นี้ มีผู้ใช้งานแล้ว กรุณาป้อนใหม่" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = _facultyCodeInt + " นี้ สามารถใช้งานได้" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckDeptCode(string _deptCode)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                int id = int.Parse(_deptCode);
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Department.Where(r => r.DeptCode == id).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = _deptCode + "นี้ มีผู้ใช้งานแล้ว กรุณาป้อนใหม่" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, message = _deptCode + " นี้ สามารถใช้งานได้" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckSubjectCode(string _SubCourse)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _SubCourse.Split(',');
                string subCode = str[0];
                string course = str[1];
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from s in db.Tb_Subject
                                where s.SubjectCode == subCode && s.Course == course
                                select new
                                {
                                    s.SubjectCode,
                                    s.Course
                                }).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckTermYear(string _termyear)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _termyear.Split(',');
                string term = str[0];
                string year = str[1];
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from s in db.Tb_SchoolYear
                                where s.Term == term && s.Year == year
                                select new
                                {
                                    s.Term,
                                    s.Year
                                }).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckEndTerm(string _enddate, string _startdate)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                if (_startdate.ToString() != "")
                {
                    DateTime Startdate = DateTime.ParseExact(_startdate, "dd/MM/yyyy", Shared.CultureInfoTh);
                    DateTime Enddate = DateTime.ParseExact(_enddate, "dd/MM/yyyy", Shared.CultureInfoTh);
                    if (Startdate >= Enddate)
                    {
                        jsonReturn = new JsonResponse { status = false };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                }
                else
                {
                    jsonReturn = new JsonResponse { status = false };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckStartMidterm(string _reDate)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _reDate.Split(',');
                string startdate = str[0];
                string enddate = str[1];
                string midterm = str[2];
                DateTime Startdate = DateTime.ParseExact(startdate, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime Enddate = DateTime.ParseExact(enddate, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime Midterm = DateTime.ParseExact(midterm, "dd/MM/yyyy", Shared.CultureInfoTh);
                if (Midterm > Startdate && Midterm < Enddate)
                {
                    jsonReturn = new JsonResponse { status = true };
                }
                else
                {
                    jsonReturn = new JsonResponse { status = false };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckEndMidterm(string _reDate)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _reDate.Split(',');
                string midterm = str[0];
                string enddate = str[1];
                string endmidterm = str[2];
                DateTime Endmidterm = DateTime.ParseExact(endmidterm, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime Enddate = DateTime.ParseExact(enddate, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime Midterm = DateTime.ParseExact(midterm, "dd/MM/yyyy", Shared.CultureInfoTh);
                if (Midterm < Endmidterm && Endmidterm < Enddate)
                {
                    jsonReturn = new JsonResponse { status = true };
                }
                else
                {
                    jsonReturn = new JsonResponse { status = false };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckStartFinal(string _reDate)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _reDate.Split(',');
                string enddate = str[0];
                string endmidterm = str[1];
                string fianal = str[2];
                DateTime Enddate = DateTime.ParseExact(enddate, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime Endmidterm = DateTime.ParseExact(endmidterm, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime Fianal = DateTime.ParseExact(fianal, "dd/MM/yyyy", Shared.CultureInfoTh);
                if (Fianal > Endmidterm && Fianal < Enddate)
                {
                    jsonReturn = new JsonResponse { status = true };
                }
                else
                {
                    jsonReturn = new JsonResponse { status = false };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckEndFinal(string _reDate)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _reDate.Split(',');
                string enddate = str[0];
                string startfinal = str[1];
                string endfianal = str[2];
                DateTime Enddate = DateTime.ParseExact(enddate, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime startFinal = DateTime.ParseExact(startfinal, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime endFianal = DateTime.ParseExact(endfianal, "dd/MM/yyyy", Shared.CultureInfoTh);
                if (startFinal < endFianal && endFianal < Enddate)
                {
                    jsonReturn = new JsonResponse { status = true };
                }
                else
                {
                    jsonReturn = new JsonResponse { status = false };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        }
    }
}
