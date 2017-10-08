using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Students_Attendance_Project.Models;
using WebGrease.Css.Extensions;
using System.Web.Security;
using PagedList;
using Students_Attendance_Project.Common;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Collections;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Students_Attendance_Project.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult _Profile()
        {
            using (var db = new Student_AttendanceEntities())
            {
                ViewBag.Dept = db.Tb_Department.Select(r => new DepartmentModel { DeptCode = r.DeptCode, DeptName = r.DeptName }).ToList();
            }
            return View();
        } // แสดงข้อมลส่วนตัวผู้ใช้

        public JsonResult SaveUserUpdate(UserMedel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_User.Where(r => r.UserID == model.UserID).FirstOrDefault();
                    if (data != null)
                    {
                        db.Tb_User.Where(r => r.UserID == model.UserID).ForEach(r =>
                        {
                            r.Password = model.Password;
                            r.Name = model.Name;
                            r.Email = model.Email;
                        });
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเเล้ว" };
                    }
                }
            }
            catch (Exception ex)
            {
                // error do something
                jsonReturn = new JsonResponse { status = false, message = "เกิดข้อผิดพลาด : " + ex.Message };
            }
            return Json(jsonReturn);
        }

        public ActionResult Index()
        {
            return View();
        } // หน้าเริ่นต้นระบบ สำหรับ อาจารย์ผู้สอน

        // Subject Management User search and Read only

        public ActionResult Subject(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from t in db.Tb_Subject
                            where (string.IsNullOrEmpty(model.Query) || t.SubjectCode.Contains(model.Query)
                            || t.SubjectName.Contains(model.Query) || t.SubjectNameEN.Contains(model.Query))
                            orderby t.SubjectCode, t.Course
                            select new SubjectModel
                            {
                                SubjectCode = t.SubjectCode,
                                Course = t.Course,
                                SubjectName = t.SubjectName,
                                SubjectNameEN = t.SubjectNameEN,
                                SubjectTheory = (int)(t.SubjectTheory),
                                TimeTheory = (int)(t.TimeTheory),
                                SubjectPractice = (int)(t.SubjectPractice),
                                TimePractice = (int)(t.TimePractice)
                            }).ToPagedList(model.page, 20);
                ViewBag.Subject = data;
            }
            return View(model);
        } // ค้นหา และแสดงรายวิชาทั้งหมด

        // Study Grop Management CRUD LINQ TO SQL

        public ActionResult studyGroup(FilterModel model /*, int UserID*/)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from c in db.Tb_StudyGroup
                            join d in db.Tb_Department on c.DeptCode equals d.DeptCode
                            join sy in db.Tb_SchoolYear on c.SchYearID equals sy.SchYearID
                            join s in db.Tb_Subject on new { c.SubjectCode, c.Course } equals new { s.SubjectCode, s.Course }
                            where (string.IsNullOrEmpty(model.Query) || c.StudyGroupCode.Contains(model.Query) || sy.Year.Contains(model.Query) || s.SubjectCode.Contains(model.Query) || s.SubjectName.Contains(model.Query))
                            where c.UserID == UserLogon.UserID
                            orderby c.StudyGroupID descending, sy.Year descending
                            select new StudyGroupModel()
                            {
                                Term = sy.Term,
                                Year = sy.Year,
                                SubjectCode = s.SubjectCode,
                                SubjectName = s.SubjectName,
                                Course = s.Course,
                                StudyGroupID = c.StudyGroupID,
                                DeptName = d.DeptName,
                                StudyGroupCode = c.StudyGroupCode,
                                //UserID = c.UserID.Value
                            }).ToPagedList(model.page, 20);
                ViewBag.studyGroup = data;

                var dataFac = (from tf in db.Tb_Faculty
                                   //join tf in db.Tb_Faculty on td.FacultyID equals tf.FacultyID
                               select new StudyGroupModel()
                               {
                                   FacultyCode = tf.FacultyCode,
                                   FacultyName = tf.FacultyName
                               }).ToList();
                ViewBag.dataFaculty = dataFac;

                var dataschYear = (from sy in db.Tb_SchoolYear
                                   where DateTime.Now > sy.StartDate && DateTime.Now < sy.EndDate
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = sy.SchYearID,
                                       Term = sy.Term,
                                       Year = sy.Year
                                   }).ToList();
                ViewBag.SchoolYear = dataschYear;

                var dataSubj = (from s in db.Tb_Subject
                                select new StudyGroupModel()
                                {
                                    SubjectCode = s.SubjectCode,
                                    SubjectName = s.SubjectName,
                                    Course = s.Course
                                }).ToList();
                ViewBag.Subject = dataSubj;

            }
            return View(model);
        } // ค้นหา และแสดงข้อมูลกลุ่มเรียนของอาจารย์ผู้สอนแต่ละคน

        public JsonResult getDepartment(int id) // รับ FacultyID มา เพื่อ จะไป selete DeptName return list
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Department.Where(r => r.FacultyCode == id).Select(r => new
                    {
                        r.DeptCode,
                        r.DeptName
                    }).ToList();
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

        public JsonResult getShortDepartment(int id) // รับ Dept_PK มา เพื่อ จะไป selete ShortDeptName return list
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Department.Where(r => r.DeptCode == id).Select(r => new
                    {
                        r.ShortName
                    }).ToList();
                    if (data.Count <= 0)
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

        public JsonResult SaveStudyGroup(StudyGroupModel model, HttpPostedFileBase file)  // บันทึกข้อมูลกลุ่มเรียน และข้อมูลนักศึกษาที่ Upload from Excel
        {
            var jsonReturn = new JsonResponse();
            jsonReturn.status = true;
            using (var db = new Student_AttendanceEntities())
            {
                using (var tran = db.Database.BeginTransaction()) // transaction Scope
                {
                    try
                    {
                        DataSet ds = new DataSet();
                        bool isID = true;
                        int countStd = 0;
                        var path = "~/FileUpload/";
                        if (file != null)
                        {
                            if (Request.Files["file"].ContentLength > 0)
                            {
                                string fileExtension = Path.GetExtension(Request.Files["file"].FileName);
                                bool exists = System.IO.Directory.Exists(Server.MapPath(path)); // map path in Server by boolean
                                if (!exists)
                                {
                                    System.IO.Directory.CreateDirectory(Server.MapPath(path)); // Create folder if it does not exist.
                                }

                                if (fileExtension == ".xlsx") // if fileExtension = .xlsx true
                                {
                                    string fileLocation = Server.MapPath(path) + Request.Files["file"].FileName;
                                    if (System.IO.File.Exists(fileLocation))
                                    {
                                        System.IO.File.Delete(fileLocation);
                                    }
                                    Request.Files["file"].SaveAs(fileLocation);
                                    string excelConnectionString = string.Empty;
                                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";

                                    if (fileExtension == ".xlsx")
                                    {
                                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                    }
                                    //Create Connection to Excel work book and add oledb namespace
                                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                                    excelConnection.Open();
                                    DataTable dt = new DataTable();
                                    //string colHeader = string.Format("Select * from [Sheet1$A1:C]");

                                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                    if (dt == null)
                                    {
                                        return null;
                                    }

                                    String[] excelSheets = new String[dt.Rows.Count];
                                    int t = 0;
                                    //excel data saves in temp file here.
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        excelSheets[t] = row["TABLE_NAME"].ToString();
                                        t++;
                                    }
                                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);

                                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                                    {
                                        dataAdapter.Fill(ds);
                                    }
                                    excelConnection.Close();
                                    string col1 = ds.Tables[0].Columns[0].ToString();
                                    string col2 = ds.Tables[0].Columns[1].ToString();
                                    string col3 = ds.Tables[0].Columns[2].ToString();
                                    if (col1 == "STUDENT_NO" && col2 == "FULLNAME" && col3 == "FULLNAME_EN")
                                    {
                                        jsonReturn.status = true;
                                    }
                                    else
                                    {
                                        jsonReturn.status = false;
                                    }
                                }
                                else
                                {
                                    jsonReturn.status = false;
                                }
                            }
                            else
                            {
                                jsonReturn.status = false;
                            }
                        }
                        if (jsonReturn.status != false)
                        {
                            string[] subj = model.SubjectCode.Split(',');
                            model.SubjectCode = subj[0];
                            model.Course = subj[1];
                            if (model.StudyGroupID == 0)
                            {
                                var data = new Tb_StudyGroup()
                                {
                                    StudyGroupCode = model.StudyGroupCode,
                                    DeptCode = model.DeptCode,
                                    SchYearID = model.SchYearID,
                                    SubjectCode = model.SubjectCode,
                                    Course = model.Course,
                                    UserID = UserLogon.UserID
                                };
                                db.Tb_StudyGroup.Add(data);
                                db.SaveChanges();
                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    string id = ds.Tables[0].Rows[i][0].ToString();
                                    string nameTH = ds.Tables[0].Rows[i][1].ToString();
                                    string nameEN = ds.Tables[0].Rows[i][2].ToString();
                                    if (id.Length < 13 || id.Length > 14)
                                    {
                                        isID = false;
                                        break;
                                    }
                                    else
                                    {
                                        isID = true;
                                        db.Tb_Student.Add(new Tb_Student
                                        {
                                            StdCode = id,
                                            NameTH = nameTH,
                                            NameEN = nameEN,
                                            StatusID = 5, // 5 = สถานะลงทะเบียน
                                            StudyGroupID = data.StudyGroupID
                                        });
                                        countStd++;
                                    }
                                }
                                if (isID == true)
                                {
                                    db.SaveChanges();
                                    tran.Commit();
                                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเล้ว", data = new { countStd } };
                                }
                                else
                                {
                                    tran.Rollback();
                                    jsonReturn = new JsonResponse { status = false, message = "ไฟล์ไม่ถูกต้อง ! กรุณาดูตัวอย่างไฟล์ในการอัปโหลด" };
                                }
                            }
                            else
                            {
                                var data = db.Tb_StudyGroup.Where(r => r.StudyGroupID == model.StudyGroupID).FirstOrDefault();
                                if (data != null)
                                {
                                    db.Tb_StudyGroup.Where(r => r.StudyGroupID == model.StudyGroupID).ForEach(r =>
                                    {
                                        r.StudyGroupCode = model.StudyGroupCode;
                                        r.SubjectCode = model.SubjectCode;
                                        r.Course = model.Course;
                                    });
                                    db.SaveChanges();
                                    tran.Commit();
                                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                                }
                            }
                        }
                        else
                        {
                            tran.Rollback();
                            jsonReturn = new JsonResponse { status = false, message = "ไฟล์ไม่ถูกต้อง ! กรุณาดูตัวอย่างไฟล์ในการอัปโหลด" };
                        }
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
                    }
                }
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateStudyGroup(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from c in db.Tb_StudyGroup
                                join d in db.Tb_Department on c.DeptCode equals d.DeptCode
                                join f in db.Tb_Faculty on d.FacultyCode equals f.FacultyCode
                                join s in db.Tb_Subject on c.SubjectCode equals s.SubjectCode
                                where c.DeptCode == d.DeptCode && d.FacultyCode == f.FacultyCode && c.SubjectCode == s.SubjectCode && c.StudyGroupID == id
                                select new
                                {
                                    c.StudyGroupID,
                                    c.StudyGroupCode,
                                    d.DeptCode,
                                    d.DeptName,
                                    f.FacultyCode,
                                    s.SubjectCode,
                                    s.Course,
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
        } // ส่งข้อมูลจาก DB ไปหน้า View เพื่อทำการแก้ไขชุดข้อมูลใน record นั้น

        public JsonResult DeleteStudyGroup(int id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_StudyGroup.Where(r => r.StudyGroupID == id).FirstOrDefault();
                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "fail" };
                    }
                    else
                    {
                        db.Tb_StudyGroup.Remove(data);
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
        } // ลบข้อมูล record นั้น จากตาราง กลุ่มเรียน ระบบไม่อนุญาตให้ลบ

        // Student Management CRUD LINQ TO SQL

        public ActionResult Student(FilterModel model, int id)
        {

            using (var db = new Student_AttendanceEntities())
            {
                var data = (from sg in db.Tb_StudyGroup
                            join s in db.Tb_Student on sg.StudyGroupID equals s.StudyGroupID
                            join st in db.Tb_Status on s.StatusID equals st.StatusID
                            where sg.StudyGroupID == id
                            orderby s.StdCode, sg.StudyGroupID
                            select new StudentShow
                            {
                                StdID = s.StdID,
                                StdCode = s.StdCode,
                                NameEN = s.NameEN,
                                NameTH = s.NameTH,
                                StatusID = st.StatusID,
                                StatusName = st.StatusName,
                                StudyGroupID = sg.StudyGroupID
                            }).ToPagedList(model.page, 20);
                ViewBag.Student = data;

                var data1 = (from sg in db.Tb_StudyGroup
                             join sj in db.Tb_Subject on sg.SubjectCode equals sj.SubjectCode
                             join sy in db.Tb_SchoolYear on sg.SchYearID equals sy.SchYearID
                             where sg.StudyGroupID == id
                             select new StudentModel
                             {
                                 StudyGroupID = sg.StudyGroupID,
                                 StudyGroupCode = sg.StudyGroupCode,
                                 SchYearID = sy.SchYearID,
                                 Term = sy.Term,
                                 Year = sy.Year,
                                 SubjectCode = sj.SubjectCode,
                                 SubjectName = sj.SubjectName,
                                 Course = sj.Course
                             }).ToList();
                ViewBag.StudyGroup = data1;
            }
            return View(model);
        } // ค้นหา และแสดงข้อมูลนักศึกษาตามกลุ่มเรียน

        public JsonResult UpdateStudent(string id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] _id = id.Split(',');
                int stdID = int.Parse(_id[0]);
                int stdyID = int.Parse(_id[1]);
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from r in db.Tb_Student
                                where r.StdID == stdID && r.StudyGroupID == stdyID
                                select new StudentModel
                                {
                                    StdID = r.StdID,
                                    StudyGroupID = r.StudyGroupID,
                                    StdCode = r.StdCode,
                                    NameEN = r.NameEN,
                                    NameTH = r.NameTH,
                                    StatusID = r.StatusID
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
                jsonReturn = new JsonResponse { status = false, message = "fail" + ex.Message };
            }
            return Json(jsonReturn);
        } // ส่งข้อมูลจาก นศ.จาก  DB ไปหน้า View เพื่อทำการแก้ไขชุดข้อมูลใน record นั้น

        public JsonResult SaveStudent(StudentModel model, HttpPostedFileBase file)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (file == null)
                            {
                                if (model.StdID == 0)
                                {
                                    model.NameTH = model.Prefix + model.NameTH;
                                    model.NameEN = model.PrefixEN + model.NameEN;
                                    var data = new Tb_Student()
                                    {
                                        StdCode = model.StdCode,
                                        NameEN = model.NameEN,
                                        NameTH = model.NameTH,
                                        StatusID = 5,
                                        //UserID = model.UserID,
                                        StudyGroupID = model.StudyGroupID
                                    };
                                    db.Tb_Student.Add(data);
                                    db.SaveChanges();
                                    var query = db.Tb_Schedule.Where(r => r.StudyGroupID == model.StudyGroupID).ToList();
                                    if (query.Count > 0 && query != null)
                                    {
                                        foreach (var r in query)
                                        {
                                            var dataschedule = new GenerateDateModel
                                            {
                                                SchYearID = r.SchYearID,
                                                DayTeach = int.Parse(r.DayTeach),
                                                StudyGroupID = r.StudyGroupID != 0 ? r.StudyGroupID : model.StudyGroupID
                                            };
                                            List<DateTime> date = generateDate(dataschedule);
                                            if (date != null)
                                            {
                                                foreach (var c in date)
                                                {
                                                    var HaveDate = db.Tb_StudentCheck.Where(x => x.DateCheck == c && x.StdID == data.StdID).FirstOrDefault();
                                                    if (HaveDate != null)  // เช็คเพือดูว่า วันที่ query มา มีแล้วรึยัง ถ้ามีแล้ว ให้ออกจากลูป ไม่ต้องเพิ่มซ้ำ
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        var holiday = db.Tb_Holiday.Where(x => x.HolidayDate == c).FirstOrDefault();
                                                        db.Tb_StudentCheck.Add(new Tb_StudentCheck
                                                        {
                                                            DateCheck = c,
                                                            StdID = data.StdID,
                                                            StatusID = (holiday == null ? 9 : 8), // ถ้าไม่ตรงวันหยุด = 9 (รอเช็คชื่อ) มิฉะนั้นแล้ว = 8 (วันหยุด)
                                                            StudyGroupID = r.StudyGroupID,
                                                            Note = (holiday == null ? null : "หยุด " + holiday.HolidayName)  // ถ้าไม่ตรงวันหยุด = Null มิฉะนั้นแล้ว = วันหยุด วันนั้น
                                                        });
                                                    }
                                                }
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var data = db.Tb_Student.Where(r => r.StdID == model.StdID).FirstOrDefault();
                                    if (data != null)
                                    {
                                        db.Tb_Student.Where(r => r.StdID == model.StdID).ForEach(r =>
                                        {
                                            r.NameEN = model.NameEN;
                                            r.NameTH = model.NameTH;
                                            r.StatusID = model.StatusID;
                                        });
                                        db.SaveChanges();
                                    }
                                }
                                tran.Commit();
                                jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                            }
                            else
                            {
                                DataSet ds = new DataSet();
                                var list = new List<StudentModel>();
                                bool isFileCorrect = true;
                                int stdDuplicate = 0;
                                int stdNew = 0;
                                int stdTotal = 0;
                                var path = "~/FileUpload/";
                                if (Request.Files["file"].ContentLength > 0)
                                {
                                    bool exists = System.IO.Directory.Exists(Server.MapPath(path));
                                    if (!exists)
                                    {
                                        System.IO.Directory.CreateDirectory(Server.MapPath(path));
                                    }
                                    string fileExtension = Path.GetExtension(Request.Files["file"].FileName);

                                    if (fileExtension == ".xlsx")
                                    {
                                        string fileLocation = Server.MapPath(path) + Request.Files["file"].FileName;
                                        if (System.IO.File.Exists(fileLocation))
                                        {
                                            System.IO.File.Delete(fileLocation);
                                        }
                                        Request.Files["file"].SaveAs(fileLocation);
                                        string excelConnectionString = string.Empty;
                                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                        //connection String for xls file format.
                                        //if (fileExtension == ".xls")
                                        //{
                                        //    excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                        //    fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                                        //}
                                        //connection String for xlsx file format.
                                        if (fileExtension == ".xlsx")
                                        {
                                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                            fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                        }
                                        //Create Connection to Excel work book and add oledb namespace
                                        OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                                        excelConnection.Open();
                                        DataTable dt = new DataTable();

                                        dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                        if (dt == null)
                                        {
                                            return null;
                                        }

                                        String[] excelSheets = new String[dt.Rows.Count];
                                        int t = 0;
                                        //excel data saves in temp file here.
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            excelSheets[t] = row["TABLE_NAME"].ToString();
                                            t++;
                                        }
                                        OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                                        string query = string.Format("Select * from [{0}]", excelSheets[0]);
                                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                                        {
                                            dataAdapter.Fill(ds);
                                        }
                                        excelConnection.Close();
                                        string col1 = ds.Tables[0].Columns[0].ToString();
                                        string col2 = ds.Tables[0].Columns[1].ToString();
                                        string col3 = ds.Tables[0].Columns[2].ToString();
                                        if (col1 == "STUDENT_NO" && col2 == "FULLNAME" && col3 == "FULLNAME_EN")
                                        {
                                            jsonReturn.status = true;
                                        }
                                        else
                                        {
                                            jsonReturn.status = false;
                                        }
                                    }
                                    else
                                    {
                                        jsonReturn.status = false;
                                    }
                                }
                                else
                                {
                                    jsonReturn.status = false;
                                }
                                if (jsonReturn.status == true)
                                {
                                    var notstd = new List<string>();
                                    var datastd = new List<StudentModel>();
                                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                    {
                                        string id = ds.Tables[0].Rows[i][0].ToString();
                                        string nameTH = ds.Tables[0].Rows[i][1].ToString();
                                        string nameEN = ds.Tables[0].Rows[i][2].ToString();
                                        if (id.Length < 13 || id.Length > 14)
                                        {
                                            isFileCorrect = false;
                                            break;
                                        }
                                        else
                                        {
                                            var data = db.Tb_Student.Where(r => r.StdCode == id && r.StudyGroupID == model.StudyGroupID).Select(r => r.StdCode).FirstOrDefault();
                                            if (data != null)
                                            {
                                                stdDuplicate++;
                                                notstd.Add(id);
                                                continue;
                                            }
                                            else
                                            {
                                                var dataStd = new Tb_Student()
                                                {
                                                    StdCode = id,
                                                    NameTH = nameTH,
                                                    NameEN = nameEN,
                                                    StatusID = 5,
                                                    StudyGroupID = model.StudyGroupID
                                                };
                                                db.Tb_Student.Add(dataStd);
                                                db.SaveChanges();
                                                stdNew++;
                                                datastd.Add(new StudentModel
                                                {
                                                    StdID = dataStd.StdID,
                                                    StudyGroupID = dataStd.StudyGroupID
                                                });
                                                isFileCorrect = true;
                                            }
                                        }
                                    }
                                    var Scode = db.Tb_Student.Where(r => r.StudyGroupID == model.StudyGroupID).Select(r => r.StdCode).ToList();
                                    stdTotal = Scode.Count();
                                    var dataS = new object();
                                    if (stdDuplicate > 0 && stdDuplicate < stdTotal)
                                    {
                                        var stdcode = Scode.Where(r => !notstd.Contains(r)).ToList();
                                        dataS = db.Tb_Student.Where(r => stdcode.Contains(r.StdCode) && r.StudyGroupID == model.StudyGroupID).Select(r => new
                                        {
                                            r.StdID,
                                            r.NameTH,
                                            r.StdCode
                                        }).ToList();
                                    }
                                    if (stdNew > 0)
                                    {
                                        var query = db.Tb_Schedule.Where(r => r.StudyGroupID == model.StudyGroupID).ToList();
                                        if (query.Count > 0 && query != null)
                                        {
                                            foreach (var r in query)
                                            {
                                                var dataschedule = new GenerateDateModel
                                                {
                                                    SchYearID = r.SchYearID,
                                                    DayTeach = int.Parse(r.DayTeach),
                                                    StudyGroupID = r.StudyGroupID != 0 ? r.StudyGroupID : model.StudyGroupID
                                                };
                                                List<DateTime> date = generateDate(dataschedule);
                                                if (date != null)
                                                {
                                                    foreach (var c in date)
                                                    {
                                                        foreach (var e in datastd)
                                                        {
                                                            var HaveDate = db.Tb_StudentCheck.Where(x => x.DateCheck == c && x.StdID == e.StdID).FirstOrDefault();
                                                            if (HaveDate != null)  // เช็คเพือดูว่า วันที่ query มา มีแล้วรึยัง ถ้ามีแล้ว ให้ออกจากลูป ไม่ต้องเพิ่มซ้ำ
                                                            {
                                                                continue;
                                                            }
                                                            else
                                                            {
                                                                var holiday = db.Tb_Holiday.Where(x => x.HolidayDate == c).FirstOrDefault();
                                                                db.Tb_StudentCheck.Add(new Tb_StudentCheck
                                                                {
                                                                    DateCheck = c,
                                                                    StdID = e.StdID,
                                                                    StatusID = (holiday == null ? 9 : 8), // ถ้าไม่ตรงวันหยุด = 9 (รอเช็คชื่อ) มิฉะนั้นแล้ว = 8 (วันหยุด)
                                                                    StudyGroupID = e.StudyGroupID,
                                                                    Note = (holiday == null ? null : "หยุด " + holiday.HolidayName)  // ถ้าไม่ตรงวันหยุด = Null มิฉะนั้นแล้ว = วันหยุด วันนั้น
                                                                });
                                                            }
                                                        }
                                                    }
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                    if (isFileCorrect == true)
                                    {
                                        db.SaveChanges();
                                        tran.Commit();
                                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเล้ว", data = new { stdDuplicate, stdNew, stdTotal, dataS } };
                                    }
                                    else
                                    {
                                        tran.Rollback();
                                        jsonReturn = new JsonResponse { status = false, message = "ไฟล์ไม่ถูกต้อง ! กรุณาดูตัวอย่างไฟล์ในการอัปโหลด" };
                                    }
                                }
                                else
                                {
                                    tran.Rollback();
                                    jsonReturn = new JsonResponse { status = false, message = "ไฟล์ไม่ถูกต้อง ! กรุณาดูตัวอย่างไฟล์ในการอัปโหลด" };
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
            }
            return Json(jsonReturn);
        } // บันทึกข้อมูลข้อมูลนักศึกษาที่ Upload from Excel และ การเพิ่มที่ละคน

        public JsonResult SaveStdChangeStatus(List<StudentModel> model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model != null)
                    {
                        foreach (var r in model)
                        {
                            db.Tb_Student.Where(x => x.StudyGroupID == r.StudyGroupID && x.StdCode == r.StdCode).ForEach(x =>
                            {
                                x.StatusID = r.StatusID;
                            });
                        }
                        db.SaveChanges();
                    }
                }
                jsonReturn = new JsonResponse { status = true };
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }

            return Json(jsonReturn);
        } //บันทีกการเปลี่ยนแปลงสถานะถอนรายวิชา เมื่อ import file รายชื่อ < หรือไม่ตรง

        public JsonResult CheckStdCode(string _StdCode)
        {
            var jsonReturn = new JsonResponse();
            string[] str = _StdCode.Split(',');
            string StdCode = str[0];
            int sgID = int.Parse(str[1]);
            using (var db = new Student_AttendanceEntities())
            {
                var data = db.Tb_Student.Where(r => r.StdCode == StdCode && r.StudyGroupID == sgID).Select(r => r.StdCode).FirstOrDefault();
                if (data == null)
                {
                    jsonReturn = new JsonResponse { status = true };
                }
                else
                {
                    jsonReturn = new JsonResponse { status = false };
                }
            }
            return Json(jsonReturn);
        } // เช็คว่ารหัส นศ. ที่จะเพิ่มซ้ำหรือไม่

        public List<DateTime> generateDate(GenerateDateModel model)
        {
            var date = new List<DateTime>();
            string[] dateofweek = { "", "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };
            bool isDay = false;
            using (var db = new Student_AttendanceEntities())
            {
                var dataSchYear = db.Tb_SchoolYear.Where(r => r.SchYearID == model.SchYearID).FirstOrDefault();
                DateTime sdate = dataSchYear.StartDate;
                do
                {
                    if (dateofweek[model.DayTeach] == sdate.DayOfWeek.ToString().ToLower())
                    {
                        for (int i = 1; i <= 18; i++)
                        {
                            if (sdate.DayOfYear >= dataSchYear.StartFinal.DayOfYear) // ป้องกัน เมื่อ เทอมซัมเมอร์ ซึ่งการสอนไม่เกิน 7 สัปดาห์
                            {
                                break;
                            }
                            else
                            {
                                if (sdate.DayOfYear <= dataSchYear.StartMidterm.DayOfYear && sdate.DayOfYear <= dataSchYear.EndMidterm.DayOfYear && sdate.DayOfYear < dataSchYear.StartFinal.DayOfYear)
                                {
                                    date.Add(sdate);
                                }
                                else if (sdate.DayOfYear > dataSchYear.EndMidterm.DayOfYear && sdate.DayOfYear <= dataSchYear.StartFinal.DayOfYear && sdate.DayOfYear <= dataSchYear.EndFinal.DayOfYear)
                                {
                                    date.Add(sdate);
                                }
                                sdate = sdate.AddDays(7);
                            }

                        }
                        isDay = true;
                    }
                    else
                    {
                        isDay = false;
                        sdate = sdate.AddDays(+1);
                    }
                } while (isDay == false);
                date.Sort();
            }
            return date;
        }

        // Schedule Management CRUD LINQ TO SQL

        public ActionResult Schedule(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var dataSchYear = (from r in db.Tb_SchoolYear
                                   where DateTime.Now > r.StartDate && DateTime.Now < r.EndDate
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = r.SchYearID,
                                       Term = r.Term,
                                       Year = r.Year
                                   }).ToList();
                ViewBag.SchYear = dataSchYear; //ใช้ใน form Input

                var dataBuilding = (from b in db.Tb_Building
                                    select new BuildingModel { BuildingCode = b.BuildingCode, BuildingName = b.BuildingName }).ToList();
                ViewBag.Building = dataBuilding; //ใช้ใน form Input

                var yearID = (from r in db.Tb_SchoolYear
                              where DateTime.Now >= r.StartDate && DateTime.Now <= r.EndDate
                              select r.SchYearID).FirstOrDefault();

                var schYearData = (from r in db.Tb_SchoolYear
                                   orderby r.Year descending
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = r.SchYearID,
                                       Term = r.Term,
                                       Year = r.Year
                                   }).ToList();
                ViewBag.SearchYear = schYearData; // ใช้กลับ form-Search input select

                if (model.Query == null)
                {
                    var dataSubject = (from s in db.Tb_Subject
                                       join s1 in db.Tb_StudyGroup on new { s.SubjectCode, s.Course } equals new { s1.SubjectCode, s1.Course }
                                       join s2 in db.Tb_SchoolYear on s1.SchYearID equals s2.SchYearID
                                       //where s2.SchYearID == int.Parse(model.Query)
                                       where DateTime.Now >= s2.StartDate && DateTime.Now <= s2.EndDate && s1.UserID == UserLogon.UserID
                                       select new StudyGroupModel()
                                       {
                                           SubjectCode = s.SubjectCode,
                                           Course = s.Course,
                                           SubjectName = s.SubjectName,
                                           StudyGroupCode = s1.StudyGroupCode,
                                           StudyGroupID = s1.StudyGroupID,
                                           SchYearID = s2.SchYearID
                                       }).ToList();
                    ViewBag.Subject = dataSubject;

                    var Day = (from r in db.Tb_Schedule
                               where (r.SchYearID == yearID)
                               where (r.UserID == UserLogon.UserID)
                               orderby r.StartTimeInt
                               select r).ToList();
                    ViewBag.Day = Day; //ใช้ใน ตาราง
                }
                else
                {
                    int schYearID = int.Parse(model.Query);
                    var dataSubject1 = (from s in db.Tb_Subject
                                        join s1 in db.Tb_StudyGroup on new { s.SubjectCode, s.Course } equals new { s1.SubjectCode, s1.Course }
                                        join s2 in db.Tb_SchoolYear on s1.SchYearID equals s2.SchYearID
                                        where s2.SchYearID == schYearID && s1.UserID == UserLogon.UserID
                                        //where DateTime.Now > s2.StartDate && DateTime.Now < s2.EndDate
                                        select new StudyGroupModel()
                                        {
                                            SubjectCode = s.SubjectCode,
                                            Course = s.Course,
                                            SubjectName = s.SubjectName,
                                            StudyGroupCode = s1.StudyGroupCode,
                                            StudyGroupID = s1.StudyGroupID,
                                            SchYearID = s2.SchYearID
                                        }).ToList();
                    ViewBag.Subject = dataSubject1;

                    var Day = (from t in db.Tb_Schedule
                               where (t.SchYearID == schYearID)
                               where (t.UserID == UserLogon.UserID)
                               orderby t.StartTimeInt
                               select t).ToList();
                    ViewBag.Day = Day; //ใช้ใน ตาราง
                }
            }

            return View(model);
        } // แสดงข้อมูลตารางสอน และค้นหาได้จาก ภาคการศึกษา ของอาจารย์ผู้สอนแต่ละท่าน

        public JsonResult UpdateSchedule(string id)
        {
            var jsonReturn = new JsonResponse();
            int _id = int.Parse(id);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var dataSchedule = db.Tb_Schedule.Where(r => r.ScheduleID == _id).Select(r => new
                    {
                        r.ScheduleID,
                        r.SchYearID,
                        r.StartTimeInt,
                        r.EndTimeInt,
                        r.SubjectCode,
                        r.Course,
                        r.StudyGroupID,
                        r.BuildingCode,
                        r.TypeSubject,
                        r.RoomNo,
                        r.DayTeach,
                    }).FirstOrDefault();
                    var data = (from r in db.Tb_StudentCheck
                                where r.StudyGroupID == dataSchedule.StudyGroupID && (r.StatusID == 1 || r.StatusID == 2 || r.StatusID == 3 || r.StatusID == 4)
                                select r).ToList();

                    if (data.Count > 0)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ไม่สามารถแก้ไขข้อมูลได้ เนื่องจากมีการเช็คชื่อไปแล้ว" };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, data = dataSchedule };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        } // ส่งข้อมูลจาก ตารางสอน จาก DB ไปหน้า View เพื่อทำการแก้ไขชุดข้อมูลใน record นั้น

        public JsonResult SaveSchedule(ScheduleModel model)
        {
            var jsonReturn = new JsonResponse();
            var strTime = new string[] { "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00" };
            var strStart = strTime[model.StartTimeInt - 1];
            var strEnd = strTime[model.EndTimeInt - 1];
            bool isGen = false;
            bool isEdit = false;
            int gID = 0;
            using (var db = new Student_AttendanceEntities())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.ScheduleID == 0)
                        {
                            string[] str = model.SubjectCode.Split(',');
                            model.SubjectCode = str[0];
                            model.Course = str[1];
                            model.StudyGroupID = int.Parse(str[2]);
                            db.Tb_Schedule.Add(new Tb_Schedule()
                            {
                                DayTeach = model.DayTeach,
                                StudyGroupID = model.StudyGroupID,
                                SubjectCode = model.SubjectCode,
                                Course = model.Course,
                                EndTimeInt = model.EndTimeInt,
                                StartTimeInt = model.StartTimeInt,
                                StartTime = strStart,
                                EndTime = strEnd,
                                BuildingCode = model.BuildingCode,
                                TypeSubject = model.TypeSubject,
                                RoomNo = model.RoomNo,
                                SchYearID = model.SchYearID,
                                UserID = UserLogon.UserID, // UserID
                                TotalHour = (model.EndTimeInt - model.StartTimeInt)
                            });
                        }
                        else
                        {
                            var data = db.Tb_Schedule.Where(r => r.ScheduleID == model.ScheduleID).FirstOrDefault();
                            string day = data.DayTeach;
                            if (data != null)
                            {
                                db.Tb_Schedule.Where(r => r.ScheduleID == model.ScheduleID).ForEach(r =>
                                {
                                    r.DayTeach = model.DayTeach;
                                    r.EndTimeInt = model.EndTimeInt;
                                    r.StartTimeInt = model.StartTimeInt;
                                    r.StartTime = strStart;
                                    r.EndTime = strEnd;
                                    r.BuildingCode = model.BuildingCode;
                                    r.RoomNo = model.RoomNo;
                                    r.TotalHour = (model.EndTimeInt - model.StartTimeInt);
                                });
                                isEdit = true;
                                gID = data.StudyGroupID;

                                if (isEdit == true && model.DayTeach != day)
                                {
                                    var delData = db.Tb_StudentCheck.Where(r => r.StudyGroupID == data.StudyGroupID).ToList();
                                    if (delData.Count > 0)
                                    {
                                        foreach (var r in delData)
                                        {
                                            db.Tb_StudentCheck.Remove(r);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                isEdit = false;
                            }
                        }
                        var listGen = new GenerateDateModel
                        {
                            SchYearID = model.SchYearID,
                            DayTeach = int.Parse(model.DayTeach),
                            StudyGroupID = model.StudyGroupID != 0 ? model.StudyGroupID : gID
                        };
                        isGen = GenerateDate(listGen);
                        if (isGen == true || isEdit == true)
                        {
                            db.SaveChanges();
                            tran.Commit();
                            jsonReturn = new JsonResponse { status = true };
                        }
                        else
                        {
                            tran.Rollback();
                            jsonReturn = new JsonResponse { status = false, message = "ไม่สามารถบันทึกตารางสอนได้ กรุณาเพิ่มข้อมูลนักศึกษาในกลุ่มเรียนก่อน" };
                        }
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        jsonReturn = new JsonResponse { status = false, message = ex.Message };
                    }
                }
            }
            return Json(jsonReturn);
        } // บันทึกข้อมูลข้อมูลตารางสอนของอาจารย์ผู้สอนแต่ละท่าน

        private bool GenerateDate(GenerateDateModel listGen)
        {
            bool isDay = false;
            string[] dateofweek = { "", "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };
            var date = new List<DateTime>();
            using (var db = new Student_AttendanceEntities())
            {
                db.SaveChanges();
                var dataSchYear = db.Tb_SchoolYear.Where(r => r.SchYearID == listGen.SchYearID).FirstOrDefault();
                var dataStd = (from r in db.Tb_Student
                               where r.StudyGroupID == listGen.StudyGroupID
                               orderby r.StdCode
                               select new GenStudentModel()
                               {
                                   StdID = r.StdID,
                                   StdCode = r.StdCode,
                                   StudyGroupID = listGen.StudyGroupID
                               }).ToList();

                DateTime sdate = dataSchYear.StartDate;
                do
                {
                    if (dateofweek[listGen.DayTeach] == sdate.DayOfWeek.ToString().ToLower())
                    {
                        for (int i = 1; i <= 18; i++)
                        {
                            if (sdate.DayOfYear >= dataSchYear.StartFinal.DayOfYear) // ป้องกัน เมื่อ เทอมซัมเมอร์ ซึ่งการสอนไม่เกิน 7 สัปดาห์
                            {
                                break;
                            }
                            else
                            {
                                if (sdate.DayOfYear <= dataSchYear.StartMidterm.DayOfYear && sdate.DayOfYear <= dataSchYear.EndMidterm.DayOfYear && sdate.DayOfYear < dataSchYear.StartFinal.DayOfYear)
                                {
                                    date.Add(sdate);
                                }
                                else if (sdate.DayOfYear > dataSchYear.EndMidterm.DayOfYear && sdate.DayOfYear <= dataSchYear.StartFinal.DayOfYear && sdate.DayOfYear <= dataSchYear.EndFinal.DayOfYear)
                                {
                                    date.Add(sdate);
                                }
                                sdate = sdate.AddDays(7);
                            }

                        }
                        isDay = true;
                    }
                    else
                    {
                        isDay = false;
                        sdate = sdate.AddDays(+1);
                    }
                } while (isDay == false);
                date.Sort();
                if (date != null && dataStd != null)
                {
                    foreach (var r in date)
                    {
                        var HaveDate = db.Tb_StudentCheck.Where(x => x.DateCheck == r && x.StudyGroupID == listGen.StudyGroupID).FirstOrDefault();
                        if (HaveDate != null)  // เช็คเพือดูว่า วันที่ query มา มีแล้วรึยัง ถ้ามีแล้ว ให้ออกจากลูป ไม่ต้องเพิ่มซ้ำ
                        {
                            continue;
                        }
                        else
                        {
                            var holiday = db.Tb_Holiday.Where(x => x.HolidayDate == r).FirstOrDefault();
                            foreach (var s in dataStd)
                            {
                                db.Tb_StudentCheck.Add(new Tb_StudentCheck
                                {
                                    DateCheck = r,
                                    StdID = s.StdID,
                                    StatusID = (holiday == null ? 9 : 8), // ถ้าไม่ตรงวันหยุด = 9 (รอเช็คชื่อ) มิฉะนั้นแล้ว = 8 (วันหยุด)
                                    StudyGroupID = s.StudyGroupID,
                                    Note = (holiday == null ? null : "หยุด " + holiday.HolidayName)  // ถ้าไม่ตรงวันหยุด = Null มิฉะนั้นแล้ว = วันหยุด วันนั้น
                                });
                            }
                        }
                    }
                    db.SaveChanges();
                    isDay = true;
                }
                else
                {
                    isDay = false;
                }
            }
            return isDay;
        } // Generate วันที่จะมีการเช็คชื่อหลังจากเพิ่มตาราง โดยจะ Gen วันที่สอน ตั้งแต่ เปิดเทอม - ปิดเทอม โดยข้ามช่วงสอบกลางภาค ปลายภาค

        public JsonResult getTypeSubject(string _id)
        {
            var jsonReturn = new JsonResponse();
            string[] str = _id.Split(',');
            string subj = str[0];
            string course = str[1];
            int sgID = int.Parse(str[2]);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Subject.Where(r => r.SubjectCode == subj && r.Course == course).Select(r => new
                    {
                        r.TimeTheory,
                        r.TimePractice
                    }).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = true, data = data };
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
        } // เช็ครายวิชาที่เลือกตอนเพิ่มตารางสอน ว่าเป็น ทฤษฎี หริอปฏิบัติ เพิ่มกำหนดเวลา เริ่มต้น-สิ้นสุด คายสอบ

        public JsonResult CheckDay(string _data)
        {
            var jsonReturn = new JsonResponse();
            string[] str = _data.Split(',');
            string _day = str[0];
            int schYearID = int.Parse(str[1]);
            //int UserID = int.Parse(str[2]);
            int Start = 1;
            using (var db = new Student_AttendanceEntities())
            {
                var data = db.Tb_Schedule.Where(r => r.DayTeach == _day && r.SchYearID == schYearID && r.UserID == UserLogon.UserID).Select(r => new
                {
                    r.EndTimeInt
                }).ToList();
                if (data.Count == 0)
                {
                    jsonReturn = new JsonResponse { status = true, data = Start };
                }
                else
                {
                    var Max = data.Max(r => r.EndTimeInt);
                    jsonReturn = new JsonResponse { status = true, data = Max };
                }
            }
            return Json(jsonReturn);
        } // เช็ควันนั้นมีคาบสอนสุดท้ายเวลาใด เพื่อจะเริ่มเวลาในการเพิ่มตารางสอนใหม่ หลังจากเวลานั้น

        public JsonResult getRoom(string _send)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _send.Split(',');
                string idBuilding = str[0];
                string day = str[1];
                int endTime = int.Parse(str[2]);
                int startTime = int.Parse(str[3]);
                using (var db = new Student_AttendanceEntities())
                {
                    var data1 = (from r in db.Tb_Room
                                 join s in db.Tb_Schedule on r.RoomNo equals s.RoomNo
                                 where s.DayTeach == day && (s.StartTimeInt <= startTime && s.EndTimeInt >= endTime)
                                 select r.RoomNo).FirstOrDefault();
                    var data = db.Tb_Room.Where(r => r.BuildingCode == idBuilding && r.RoomNo != data1).Select(r => new
                    {
                        r.RoomNo
                    }).ToList();

                    if (data == null)
                    {
                        jsonReturn = new JsonResponse { status = false };
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
        } // แสดงข้อมูลห้องเรียนหลังจากที่เลือก อาคาร 

        // Students Attendance Management CRUD LINQ TO SQL

        public ActionResult ScheduleCheck(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {

                var dataSchYear = (from r in db.Tb_SchoolYear
                                   where DateTime.Now > r.StartDate && DateTime.Now < r.EndDate
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = r.SchYearID,
                                       Term = r.Term,
                                       Year = r.Year
                                   }).ToList();
                ViewBag.SchYear = dataSchYear;

                var dataSubject = (from s in db.Tb_Subject
                                   join s1 in db.Tb_StudyGroup on new { s.SubjectCode, s.Course } equals new { s1.SubjectCode, s1.Course }
                                   join s2 in db.Tb_SchoolYear on s1.SchYearID equals s2.SchYearID
                                   where DateTime.Now > s2.StartDate && DateTime.Now < s2.EndDate && s1.UserID == UserLogon.UserID
                                   select new StudyGroupModel()
                                   {
                                       SubjectCode = s.SubjectCode,
                                       Course = s.Course,
                                       SubjectName = s.SubjectName,
                                       StudyGroupCode = s1.StudyGroupCode,
                                       StudyGroupID = s1.StudyGroupID
                                   }).ToList();
                ViewBag.Subject = dataSubject;

                var dataStudyGroup = (from s in db.Tb_StudyGroup
                                      select new StudyGroupModel
                                      {
                                          StudyGroupID = s.StudyGroupID,
                                          StudyGroupCode = s.StudyGroupCode
                                      }).ToList();
                ViewBag.StudyGroup = dataStudyGroup;

                var yearID = (from r in db.Tb_SchoolYear
                              where DateTime.Now > r.StartDate && DateTime.Now < r.EndDate
                              select r.SchYearID).FirstOrDefault();

                var Day = (from t in db.Tb_Schedule
                           where (t.SchYearID == yearID)
                           where (t.UserID == UserLogon.UserID)
                           orderby t.StartTimeInt
                           select t).ToList();
                ViewBag.Day = Day;
                var schID = dataSchYear.Select(x => x.SchYearID).SingleOrDefault();
                ViewBag.Haveschedule = db.Tb_Schedule.Where(r => r.UserID == UserLogon.UserID && r.SchYearID == schID).Count();
            }
            return View(model);
        }  // แสดงตารางการเช็คชื่อ ตามภาคการศึกษาปัจจุบันเท่านั้น

        public JsonResult getDatecheck(int id)  // ส่งค่าให้ modal Compendate เพื่อเลือกวันเรียนปกติ
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    // Query เอาข้อมูลวันที่เช็คเชื่อปกติ แต่ละวันจากเริ้ม-สุดสิ้นภาคเรียน ไปแสดงใน dropdownlist เพื่อจะทำการเลือกวันชดเชย
                    var dateCheck = (from r in db.Tb_StudentCheck
                                     join s in db.Tb_Student on r.StdID equals s.StdID
                                     where r.StudyGroupID == id && s.StatusID == 5 && r.StatusID == 9 && (r.Note == null || r.Note == "")
                                     orderby r.DateCheck
                                     select new DateModel { DateCheck = r.DateCheck }).Distinct().ToList();
                    // Query เอาข้อมูลวันที่ชดเชย แต่ละวันจากเริ้ม-สุดสิ้นภาคเรียน ไปแสดงใน ตารางเพื่อดู หรือลบวันชดเชย
                    var datacompensate = (from r in db.Tb_StudentCheck
                                          where r.StatusID == 7 && r.StudyGroupID == id
                                          orderby r.DateCheck
                                          select new CompensateModel
                                          {
                                              DateOrigin = r.DateCheck,
                                              ID = id,
                                              Note = r.Note

                                          }).Distinct().ToList();

                    List<CompensateModel1> compensate = new List<CompensateModel1>();
                    foreach (var r in datacompensate)  // ลูปเปลี่ยนชนิดข้อมูล DateTime to String เพื่อส่งไปแสดงหน้าบ้าน รวมถึงข้อมูลอื่นๆ ส่ง List ไป
                    {
                        compensate.Add(new Models.CompensateModel1 { DateOrigin = r.DateOrigin.ToString("dd/MM/yyyy", Shared.CultureInfoTh), Note = r.Note, ID = r.ID });
                    }

                    List<string> date = new List<string>();
                    foreach (var r in dateCheck)// ลูปเปลี่ยนชนิดข้อมูล DateTime to String เพื่อส่งไปแสดงหน้าบ้าน ส่ง List ไป
                    {
                        date.Add(r.DateCheck.ToString("dd/MM/yyyy", Shared.CultureInfoTh));
                    }
                    if (dateCheck.Count > 0 && dateCheck != null)
                    {
                        jsonReturn = new JsonResponse { status = true, data = new { date, compensate } };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ไม่พบข้อมูล" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteCompensate(string _data)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _data.Split(',');
                string dateCom = str[1];
                string dateorigin = str[2];
                var dateCompensate = DateTime.ParseExact(dateCom, "dd/MM/yyyy", Shared.CultureInfoTh);
                var dateOrigin = DateTime.ParseExact(dateorigin, "dd/MM/yyyy", Shared.CultureInfoTh);
                //DateTime dateCompensate = DateTime.Parse(dateCom1);
                //DateTime dateOrigin = DateTime.Parse(dateOri);
                int groupID = int.Parse(str[0]);
                using (var db = new Student_AttendanceEntities())
                {
                    var dataCompens = (from r in db.Tb_StudentCheck
                                       join s in db.Tb_Student on r.StdID equals s.StdID
                                       where r.StudyGroupID == groupID && r.DateCheck == dateCompensate && r.StatusID == 9 && s.StatusID == 5
                                       select r).ToList();
                    var DateOriginal = (from r in db.Tb_StudentCheck
                                        where r.StudyGroupID == groupID && r.DateCheck == dateOrigin
                                        select r).ToList();
                    if (dataCompens.Count > 0 && dataCompens != null)
                    {
                        var delete = db.Tb_StudentCheck.Where(c => c.DateCheck == dateCompensate && c.StudyGroupID == groupID).ToList();
                        foreach (var r in delete)
                        {
                            db.Tb_StudentCheck.Remove(r);
                        }
                        db.SaveChanges();
                        if (DateOriginal.Count > 0 && DateOriginal != null)
                        {
                            var holiday = db.Tb_Holiday.Where(r => r.HolidayDate == dateOrigin).FirstOrDefault();
                            foreach (var r in DateOriginal)
                            {
                                db.Tb_StudentCheck.Where(x => x.StdCheckID == r.StdCheckID).ForEach(x =>
                                {
                                    x.StatusID = (holiday == null ? 9 : 8);
                                    x.Note = (holiday == null ? "" : "หยุด " + holiday.HolidayName);
                                });
                            }
                        }
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ไม่สามารถลบวันชดเชยนี้ได้ เนื่องจากมีการเช็คชื่อไปแล้ว" };
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message?.ToString() ?? ex.InnerException.Message;
                Log.Error(err);
                jsonReturn = new JsonResponse { status = false, message = ex.Message };

            }
            return Json(jsonReturn);
        } // delete วันสอนชดเชย ลบออกจากตารางการเช็คชื่อ ตามกลุ่มเรียนนั้น และให้วันที่เรียนปกติ มีสถานะ รอการเช็คชื่อ(9) , Note = ""

        public ActionResult StdCheck(FilterModel model, int id, int id2)
        {   // id = studygroupID id2 = schoolyearID
            using (var db = new Student_AttendanceEntities())
            {
                try
                {
                    var data = (from r in db.Tb_SchoolYear
                                where r.SchYearID == id2
                                select new
                                {
                                    r
                                }).FirstOrDefault();

                    var today = DateTime.Now.ToShortDateString();
                    DateTime Today = DateTime.Parse(today);
                    var dataholiday = db.Tb_Holiday.Select(r => new
                    {
                        date = r.HolidayDate,
                        datename = r.HolidayName
                    }).ToList();
                    var holiday = dataholiday.Where(r => r.date.ToShortDateString() == today).FirstOrDefault();
                    var message = new object();
                    if (Today >= data.r.StartMidterm && Today <= data.r.EndMidterm)
                    {
                        message = "ขออภัย! ไม่สามารถเช็คชื่อได้ เนื่องจากอยู่ในช่วง สอบกลางภาค";
                        return RedirectToAction("Exceptions", "Home", new { message = message });
                    }
                    else if (Today >= data.r.StartFinal && Today <= data.r.EndFinal)
                    {
                        message = "ขออภัย! ไม่สามารถเช็คชื่อได้ เนื่องจากอยู่ในช่วง สอบปลายภาค";
                        return RedirectToAction("Exceptions", "Home", new { message = message });
                    }
                    else if (holiday != null)
                    {
                        message = "ขออภัย! ไม่สามารถเช็คชื่อได้ เนื่องจากตรงกับ " + holiday.datename;
                        return RedirectToAction("Exceptions", "Home", new { message = message });
                    }
                }
                catch (Exception ex)
                {
                    RedirectToAction("Exceptions", "Home", new { message = ex.Message });
                }
            }

            using (var db = new Student_AttendanceEntities())
            {
                try
                {
                    // Query เอาข้อมูลนักศึกษาทุกคนที่ยังไม่ได้เช็คชื่อวันนี้ เพิ่อเอาไปเช็คชื่อครั้งใหม่ ในวันที่นอกเหนือจาก วันที่สอนปกติ
                    var dataNoCheck = (from sg in db.Tb_StudyGroup
                                       join st in db.Tb_Student on sg.StudyGroupID equals st.StudyGroupID
                                       where sg.StudyGroupID == id && st.StatusID == 5  // 5 = สถานะลงทะเบียน เท่านั้นที่มีสิทธิ์เช็คชื่อ
                                       orderby st.StdCode
                                       select new StudentCheckModel
                                       {
                                           StudyGroupID = sg.StudyGroupID,
                                           StdID = st.StdID,
                                           StdCode = st.StdCode,
                                           NameTH = st.NameTH,
                                       }).ToList();
                    //var today = "06/29/2017";
                    //var today = DateTime.Now.ToString("MM/dd/yyyy");
                    var today = DateTime.Now.ToShortDateString();
                    DateTime date = DateTime.Parse(today);
                    // Query เอาข้อมูลการเช็คชื่อนักศึกษาทุกคนที่ได้เช็คชื่อวันนี้ เพิ่อเอาไปแก้การเช็คชื่อ ถ้ามีคนลงทะเบียนเพิ่มในวันนี้หลังจากเช็คชื่อไปแล้ว ไม่สามารถเช็คชื่อได้ ต้องรอให้พ้นวันนี้ไปก่อน
                    var dataIscheck = (from t1 in db.Tb_Student
                                       join t3 in db.Tb_StudentCheck on t1.StdID equals t3.StdID
                                       orderby t1.StdCode
                                       where t3.DateCheck == date && t3.StudyGroupID == id && t1.StatusID == 5
                                       select new StudentCheckModel()
                                       {
                                           StdCheckID = t3.StdCheckID,
                                           StudyGroupID = t3.StudyGroupID,
                                           StdID = t3.StdID,
                                           StdCode = t1.StdCode,
                                           NameTH = t1.NameTH,
                                           StatusID = t3.StatusID,
                                           Note = t3.Note
                                       }).ToList();

                    var datastatus = (from t1 in db.Tb_StudentCheck      // Query เอาสถานะการเช็คชื่อของวันนี้ ว่าเป็น มา สาย ลา ขาด ไหม เพิ่อเอาไปแก้การเช็คชื่อ
                                      join t2 in db.Tb_Student on t1.StdID equals t2.StdID
                                      where t1.StudyGroupID == id && t1.DateCheck == date
                                      select t1.StatusID).ToList();
                    if (datastatus.Count > 0) // ถ้าวันนี้มีการเช็คชื่อแล้วไปแล้วอย่างน้อย 1 ครั้ง
                    {
                        ViewBag.StdCheck = dataIscheck;             // ส่งข้อมูล Query dataIscheck ไปแก้ไขการเช็คชื่อ
                    }
                    else
                    {
                        ViewBag.StdCheck = dataNoCheck;             // มิฉะนั้นแล้ว ส่งข้อมูล Query dataNoCheck ไปการเช็คชื่อครั้งใหม่
                    }
                    //var dataHeader = (from sg in db.Tb_StudyGroup        // Query เอาข้อมูลกลุ่มเรียน ไปแสดง ใน Header
                    //                  join sj in db.Tb_Subject on new { sg.SubjectCode, sg.Course} equals new { sj.SubjectCode, sj.Course }
                    //                  join sy in db.Tb_SchoolYear on sg.SchYearID equals sy.SchYearID
                    //                  where sg.StudyGroupID == id
                    //                  select new StudyGroupModel()
                    //                  {
                    //                      StudyGroupID = sg.StudyGroupID,
                    //                      StudyGroupCode = sg.StudyGroupCode,
                    //                      SchYearID = sy.SchYearID,
                    //                      Term = sy.Term,
                    //                      Year = sy.Year,
                    //                      SubjectCode = sj.SubjectCode,
                    //                      SubjectName = sj.SubjectName,
                    //                      Course = sj.Course
                    //                  }).ToList();
                    var dataHeader = (from sg in db.Tb_StudyGroup        // Query เอาข้อมูลกลุ่มเรียน ไปแสดง ใน Header
                                      join sj in db.Tb_Subject on new { sg.SubjectCode, sg.Course } equals new { sj.SubjectCode, sj.Course }
                                      join sy in db.Tb_SchoolYear on sg.SchYearID equals sy.SchYearID
                                      where sg.StudyGroupID == id
                                      select new
                                      {
                                          StudyGroupID = sg.StudyGroupID,
                                          StudyGroupCode = sg.StudyGroupCode,
                                          SchYearID = sy.SchYearID,
                                          Term = sy.Term,
                                          Year = sy.Year,
                                          SubjectCode = sj.SubjectCode,
                                          SubjectName = sj.SubjectName,
                                          Course = sj.Course
                                      }).ToList();
                    ViewBag.StudyGroupID = dataHeader.Select(r => r.StudyGroupID).SingleOrDefault();
                    ViewBag.StudyGroupCode = dataHeader.Select(r => r.StudyGroupCode).SingleOrDefault();
                    ViewBag.Term = dataHeader.Select(r => r.Term).SingleOrDefault();
                    ViewBag.Year = dataHeader.Select(r => r.Year).SingleOrDefault();
                    ViewBag.SubjectCode = dataHeader.Select(r => r.SubjectCode).SingleOrDefault();
                    ViewBag.SubjectName = dataHeader.Select(r => r.SubjectName).SingleOrDefault();
                    ViewBag.Course = dataHeader.Select(r => r.Course).SingleOrDefault();
                    ViewBag.StudyGroup = dataHeader;
                }
                catch (Exception ex)
                {
                    RedirectToAction("Exceptions", "Home", new { message = ex.Message });
                }
            }
            return View(model);
        } // นำข้อมูลที่มีการเช็คชื่อแล้วในวันนี้ หรือ ยังไม่ได้เช็คชื่อในวันนี้ไปแสดง เพื่อทำการเช็คชื่อ ใหม่ หรือ แก้ไขการเช็คชื่อ

        public JsonResult SaveStdCheck(List<DataModel> model)
        {
            var jsonReturn = new JsonResponse();
            using (var db = new Student_AttendanceEntities())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (model != null)
                        {
                            foreach (var r in model)
                            {
                                if (r.StdCheckID == 0)
                                {
                                    var date = DateTime.Now.ToShortDateString();
                                    if (r.StatusID == null)
                                    {
                                        r.StatusID = r.StatusRe;
                                    }
                                    var data = new Tb_StudentCheck()
                                    {
                                        StdID = int.Parse(r.StdID),
                                        StatusID = int.Parse(r.StatusID),
                                        StudyGroupID = int.Parse(r.StudyGroupID),
                                        Note = r.Note,
                                        DateCheck = DateTime.Parse(date)
                                    };
                                    db.Tb_StudentCheck.Add(data);
                                }
                                else
                                {
                                    var data = db.Tb_StudentCheck.Where(x => x.StdCheckID == r.StdCheckID).FirstOrDefault();
                                    if (data != null)
                                    {
                                        r.StatusID = r.StatusID == null ? r.StatusRe : r.StatusID;
                                        db.Tb_StudentCheck.Where(x => x.StdCheckID == r.StdCheckID).ForEach(x =>
                                        {
                                            x.StdID = int.Parse(r.StdID);
                                            x.StatusID = int.Parse(r.StatusID);
                                            x.Note = r.Note;
                                        });
                                    }
                                }
                            }
                            db.SaveChanges();
                            tran.Commit();
                            jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                        }
                        else
                        {
                            jsonReturn = new JsonResponse { status = false, message = "เกิดข้อผิดพลาด" };
                        }
                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
                    }
                }
            }
            return Json(jsonReturn);
        } // บันทึกการเช็คชื่อตามรายวิชา-กลุ่มเรียน

        public JsonResult SaveCheckname(List<DataModel> model)
        {
            var jsonReturn = new JsonResponse();
            using (var db = new Student_AttendanceEntities())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (model != null)
                        {
                            foreach (var r in model)
                            {
                                if (r.StdCheckID == 0)
                                {
                                    var date = DateTime.Now.ToShortDateString();
                                    if (r.StatusID == null)
                                    {
                                        r.StatusID = r.StatusRe;
                                    }
                                    var data = new Tb_StudentCheck()
                                    {
                                        StdID = int.Parse(r.StdID),
                                        StatusID = int.Parse(r.StatusID),
                                        StudyGroupID = int.Parse(r.StudyGroupID),
                                        Note = r.Note,
                                        // DateCheck = r.DateCheck.Value
                                        DateCheck = DateTime.Parse(date)
                                    };
                                    db.Tb_StudentCheck.Add(data);
                                }
                                else
                                {
                                    var data = db.Tb_StudentCheck.Where(x => x.StdCheckID == r.StdCheckID).FirstOrDefault();
                                    if (data != null)
                                    {
                                        r.StatusID = r.StatusID == null ? r.StatusRe : r.StatusID;
                                        db.Tb_StudentCheck.Where(x => x.StdCheckID == r.StdCheckID).ForEach(x =>
                                        {
                                            x.StdID = int.Parse(r.StdID);
                                            x.StatusID = int.Parse(r.StatusID);
                                            x.Note = r.Note;
                                        });
                                    }
                                }
                            }
                            db.SaveChanges();
                            tran.Commit();
                            jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                        }
                        else
                        {
                            jsonReturn = new JsonResponse { status = false, message = "เกิดข้อผิดพลาด" };
                        }
                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        jsonReturn = new JsonResponse { status = false, message = "error" + ex.Message };
                    }
                }
            }
            return Json(jsonReturn);
        } // บันทึกการเช็คชื่อตามรายวิชา-กลุ่มเรียน

        public JsonResult SaveCompensate(string _data) // insert วันสอนชดเชย เพิ่มในตารางการเช็คชื่อ ตามกลุ่มเรียนนั้น และให้วันที่เรียนปกติ มีสถานะ เป็นชดเชย , Note = ชดเชยวันเดิมนั้นๆ
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _data.Split(',');
                string date = str[0];
                string dateorigin = str[2];
                DateTime dateTeach = DateTime.ParseExact(date, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime dateDefault = DateTime.ParseExact(dateorigin, "dd/MM/yyyy", Shared.CultureInfoTh);
                int groupID = int.Parse(str[1]);
                bool isSave = false;
                using (var db = new Student_AttendanceEntities())
                {
                    var dataStd = (from r in db.Tb_Student
                                   where r.StudyGroupID == groupID
                                   orderby r.StdCode
                                   select new GenStudentModel()
                                   {
                                       StdID = r.StdID,
                                       StdCode = r.StdCode,
                                       StudyGroupID = groupID
                                   }).ToList();
                    if (dataStd != null)
                    {
                        foreach (var r in dataStd)
                        {
                            db.Tb_StudentCheck.Add(new Tb_StudentCheck
                            {
                                DateCheck = dateTeach,
                                StdID = r.StdID,
                                StatusID = 9, // 9 = รอเช็คชื่อ
                                StudyGroupID = r.StudyGroupID,
                                Note = null
                            });
                        }
                    }
                    db.SaveChanges();

                    //var today = DateTime.Now.ToString("MM/dd/yyyy");
                    //var today = "06/26/2017";
                    //DateTime Today = DateTime.Parse(today);

                    var dataIscheck = (from t1 in db.Tb_Student
                                       join t3 in db.Tb_StudentCheck on t1.StdID equals t3.StdID
                                       orderby t1.StdCode
                                       where t3.DateCheck == dateDefault && t3.StudyGroupID == groupID
                                       select new StudentCheckModel()
                                       {
                                           StdCheckID = t3.StdCheckID,
                                           StudyGroupID = t3.StudyGroupID,
                                           StdID = t3.StdID,
                                           StdCode = t1.StdCode,

                                       }).ToList();
                    if (dataIscheck.Count > 0)
                    {
                        foreach (var r in dataIscheck)
                        {
                            db.Tb_StudentCheck.Where(x => x.StdCheckID == r.StdCheckID).ForEach(x =>
                            {
                                x.StatusID = 7;
                                x.Note = "ชดเชย วันที่ " + date;
                            });
                            isSave = true;
                        }
                    }
                    if (isSave == true)
                    {
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ไม่สามารถบันทึกสถานะการเช็คชื่อของวันนี้ได้" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult CheckHoliday(int _id)
        {
            var jsonReturn = new JsonResponse();
            var error = "ก่อนquery";
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from r in db.Tb_SchoolYear
                                where r.SchYearID == _id
                                select new
                                {
                                    r
                                }).FirstOrDefault();

                    //var today = "07/10/2017";

                    var today = DateTime.Now.ToShortDateString();
                    DateTime Today = DateTime.Parse(today);
                    //var Today = DateTime.ParseExact(today, "dd/MM/yyyy", Shared.CultureInfoTh);
                    //DateTime Today = DateTime.Now;
                    var dataholiday = db.Tb_Holiday.Select(r => new
                    {
                        date = r.HolidayDate,
                        datename = r.HolidayName
                    }).ToList();
                    var holiday = dataholiday.Where(r => r.date.ToShortDateString() == today).FirstOrDefault();

                    error = "หลังquery";
                    if (Today >= data.r.StartMidterm && Today <= data.r.EndMidterm)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "อยู่ในช่วง สอบกลางภาค" };
                    }
                    else if (Today >= data.r.StartFinal && Today <= data.r.EndFinal)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "อยู่ในช่วง สอบปลายภาค" };
                    }
                    else if (holiday != null /*|| dataholiday.HolidayDate == Today*/)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "เป็นวัน " + holiday.datename };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message + error };
            }
            return Json(jsonReturn);
        } // รับค่า เพื่อเช็คว่าวันนี้ เป็นวันหยุด หรือ เป็นวันเรียนปกติที่มีการชดเชยไปแล้ว เพื่อจะไม่ให้มรการเช็คชื่อในวันนี้

        public JsonResult CheckDateTeach(string _date)
        {
            var jsonReturn = new JsonResponse();
            string[] str = _date.Split(',');
            string date = str[0];
            DateTime dateTeach = DateTime.ParseExact(date, "dd/MM/yyyy", Shared.CultureInfoTh);
            var dateToday = DateTime.Now.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
            DateTime today = DateTime.ParseExact(dateToday, "dd/MM/yyyy", Shared.CultureInfoTh);
            int groupID = int.Parse(str[1]);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_StudentCheck.Where(r => r.DateCheck == dateTeach && r.StudyGroupID == groupID).FirstOrDefault();
                    var data1 = db.Tb_Holiday.Where(r => r.HolidayDate == dateTeach).FirstOrDefault();
                    if (data != null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "มีข้อมูลวันที่นี้แล้ว" };
                    }
                    else if (dateTeach.DayOfYear < today.DayOfYear)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ควรเลือกวันที่ชดเชยตั้งแต่วันนี้เป็นต้นไป " };
                    }
                    else if (data1 != null)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ตรงกับวันหยุด " + data1.HolidayName };
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
        } // รับค่าวันที่จะชดชเย แล้วเช็คว่ามีวันนี้ในตารางการเช็คชื่อหรือยัง ตรงกับวันหยุดหรือไม่

        public ActionResult CheckSchedule()
        {
            using (var db = new Student_AttendanceEntities())
            {
                var dataSchYear = (from r in db.Tb_SchoolYear
                                   where DateTime.Now > r.StartDate && DateTime.Now < r.EndDate
                                   select new
                                   {
                                       SchYearID = r.SchYearID,
                                   }).SingleOrDefault();
                ViewBag.Haveschedule = db.Tb_Schedule.Where(r => r.UserID == UserLogon.UserID && r.SchYearID == dataSchYear.SchYearID).Count();
            }
            return View();
        }

        public JsonResult getScheduleCheck()
        {
            var jsonReturn = new JsonResponse();
            var dataEvent = new List<CheckScheduleModel>();
            var dataHoliday = new List<showHolidayModel>();
            var dataStartYear = new List<showSchoolYearModel>();
            var dataEndYear = new List<showSchoolYearModel>();
            var dataMidterm = new List<showSchoolYearModel>();
            var dataFinal = new List<showSchoolYearModel>();
            // List<ShowNoteModel> dataH = new List<ShowNoteModel>();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var dataevent = (from r in db.Tb_StudyGroup
                                     join x in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { x.SubjectCode, x.Course }
                                     join s in db.Tb_StudentCheck on r.StudyGroupID equals s.StudyGroupID
                                     join c in db.Tb_Schedule on r.StudyGroupID equals c.StudyGroupID
                                     where r.UserID == UserLogon.UserID
                                     orderby s.DateCheck
                                     select new
                                     {
                                         r.StudyGroupID,
                                         r.StudyGroupCode,
                                         r.SchYearID,
                                         x.SubjectName,
                                         x.SubjectCode,
                                         s.DateCheck,
                                         c.StartTime,
                                         c.EndTime,
                                         c.TypeSubject,
                                         c.TotalHour,
                                         c.RoomNo
                                     }).OrderBy(r => r.DateCheck).Distinct().ToList();
                    if (dataevent.Count > 0 && dataevent != null)
                    {
                        foreach (var r in dataevent)
                        {
                            dataEvent.Add(new CheckScheduleModel
                            {
                                StudyGroupID = r.StudyGroupID.ToString(),
                                SchYearID = r.SchYearID.ToString(),
                                id = r.StudyGroupID + "," + r.SchYearID + "," + r.DateCheck.ToShortDateString(),
                                title = r.SubjectName + " กลุ่มเรียน " + r.StudyGroupCode,
                                start = r.DateCheck.ToString("yyyy-MM-dd", Shared.CultureInfo) + "T" + r.StartTime,
                                end = r.DateCheck.ToString("yyyy-MM-dd", Shared.CultureInfo) + "T" + r.EndTime,
                                description = "<strong>วันที่ " + r.DateCheck.ToString("dd/MM/yyyy", Shared.CultureInfoTh) + " เวลา " + r.StartTime + " น. - " + r.EndTime + " น.</strong></br>" +
                                "[" + r.SubjectCode + "] " + r.SubjectName + "</br> กลุ่มเรียน " + r.StudyGroupCode +
                                "</br> ห้อง " + r.RoomNo + "</br> " + (r.TypeSubject == 1 ? "(ท." + r.TotalHour / 1 + ")" : "(ป." + r.TotalHour / 3 + ")"),
                            });
                        }
                    }

                    var dataholiday = db.Tb_Holiday.Select(r => new
                    {
                        r.HolidayDate,
                        r.HolidayName
                    }).OrderBy(r => r.HolidayDate).ToList();

                    if (dataholiday.Count > 0 && dataholiday != null)
                    {
                        foreach (var r in dataholiday)
                        {
                            dataHoliday.Add(new showHolidayModel
                            {
                                HolidayDate = r.HolidayDate.ToString("yyyy-MM-dd", Shared.CultureInfo),
                                description = r.HolidayDate.ToString("dd-MM-yyyy") + " " + r.HolidayName,
                                HolidayName = r.HolidayName
                            });
                        }
                    }

                    var dataschoolyear = db.Tb_SchoolYear.OrderBy(r => r.SchYearID).ToList();
                    if (dataschoolyear.Count > 0 && dataschoolyear != null)
                    {
                        foreach (var r in dataschoolyear)
                        {
                            dataStartYear.Add(new showSchoolYearModel
                            {
                                title = "เปิดภาคการศึกษา " + r.Term + "/" + r.Year + "",
                                description = "<b>เปืดภาคการศึกษา " + r.Term + "/" + r.Year + "</b>",
                                start = r.StartDate.ToString("yyyy-MM-dd", Shared.CultureInfo),
                            });
                            dataEndYear.Add(new showSchoolYearModel
                            {
                                title = "สิ้นสุดภาคการศึกษา " + r.Term + "/" + r.Year + "",
                                description = "<b>สิ้นสุดภาคการศึกษา " + r.Term + "/" + r.Year + "</b>",
                                start = r.EndDate.ToString("yyyy-MM-dd", Shared.CultureInfo),
                            });
                            dataMidterm.Add(new showSchoolYearModel
                            {
                                title = "สอบกลางภาค " + r.Term + "/" + r.Year + "",
                                description = "<b>วันที่ " + r.StartMidterm.ToString("dd-MM-yyyy", Shared.CultureInfoTh) + " - " + r.EndMidterm.ToString("dd/MM/yyyy", Shared.CultureInfoTh) + "</br>สอบกลางภาค " + r.Term + "/" + r.Year + "</b>",
                                start = r.StartMidterm.ToString("yyyy-MM-dd", Shared.CultureInfo),
                                end = r.EndMidterm.AddDays(1).ToString("yyyy-MM-dd", Shared.CultureInfo),
                            });
                            dataFinal.Add(new showSchoolYearModel
                            {
                                title = "สอบปลายภาค " + r.Term + "/" + r.Year + "",
                                description = "<b>วันที่ " + r.StartFinal.ToString("dd-MM-yyyy", Shared.CultureInfoTh) + " - " + r.EndFinal.ToString("dd/MM/yyyy", Shared.CultureInfoTh) + "</br>สอบปลายภาค " + r.Term + "/" + r.Year + "</b>",
                                start = r.StartFinal.ToString("yyyy-MM-dd", Shared.CultureInfo),
                                end = r.EndFinal.AddDays(1).ToString("yyyy-MM-dd", Shared.CultureInfo),
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            if (dataEvent != null || dataHoliday != null || dataStartYear != null || dataEndYear != null || dataMidterm != null || dataEvent != null)
            {
                jsonReturn = new JsonResponse { status = true, data = new { dataEvent, dataHoliday, dataStartYear, dataEndYear, dataMidterm, dataFinal } };
            }
            else
            {
                jsonReturn = new JsonResponse { status = false };
            }
            return Json(jsonReturn, JsonRequestBehavior.AllowGet);
            //return new JsonResult { Data = new { dataEvent, dataHoliday }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult CheckDateDefault(string date, string studygID)
        {
            var jsonReturn = new JsonResponse();
            DateTime dateTeach = DateTime.ParseExact(date, "dd/MM/yyyy", Shared.CultureInfoTh);
            int groupID = int.Parse(studygID);
            using (var db = new Student_AttendanceEntities())
            {
                var data = db.Tb_StudentCheck.Where(r => r.StudyGroupID == groupID && r.DateCheck == dateTeach && (r.StatusID == 1 || r.StatusID == 2 || r.StatusID == 3 || r.StatusID == 4)).ToList();
                if (data.Count > 0 && data != null)
                {
                    jsonReturn = new JsonResponse { status = false };
                }
                else
                {
                    jsonReturn = new JsonResponse { status = true };
                }
            }
            return Json(jsonReturn);
        }

        public ActionResult Checkname(int id, int id2, string _date)
        {   // id = studygroupID id2 = schoolyearID
            using (var db = new Student_AttendanceEntities())
            {
                try
                {
                    var data = (from r in db.Tb_SchoolYear
                                where DateTime.Now > r.StartDate && DateTime.Now < r.EndDate && r.SchYearID == id2
                                select new
                                {
                                    r
                                }).FirstOrDefault();
                    var today = DateTime.Now.ToShortDateString();
                    DateTime Today = DateTime.Parse(_date);
                    var dataholiday = db.Tb_Holiday.Select(r => new { date = r.HolidayDate, datename = r.HolidayName }).ToList();
                    var groupstudy = db.Tb_StudyGroup.Where(r => r.StudyGroupID == id && r.SchYearID == id2).FirstOrDefault();
                    var holiday = dataholiday.Where(r => r.date.ToShortDateString() == _date).FirstOrDefault();
                    var message = new object();
                    if (groupstudy == null)
                    {
                        message = "ขออภัย! ไม่สามารถเช็คชื่อได้ เนื่องจากไม่อยู่ในช่วงภาคการศึกษาปัจจุบัน";
                        return RedirectToAction("Exceptions", "Home", new { message = message });
                    }
                    if (Today >= data.r.StartMidterm && Today <= data.r.EndMidterm)
                    {
                        message = "ขออภัย! ไม่สามารถเช็คชื่อได้ เนื่องจากอยู่ในช่วง สอบกลางภาค";
                        return RedirectToAction("Exceptions", "Home", new { message = message });
                    }
                    else if (Today >= data.r.StartFinal && Today <= data.r.EndFinal)
                    {
                        message = "ขออภัย! ไม่สามารถเช็คชื่อได้ เนื่องจากอยู่ในช่วง สอบปลายภาค";
                        return RedirectToAction("Exceptions", "Home", new { message = message });
                    }
                    else if (holiday != null)
                    {
                        message = "ขออภัย! ไม่สามารถเช็คชื่อได้ เนื่องจากตรงกับ " + holiday.datename;
                        return RedirectToAction("Exceptions", "Home", new { message = message });
                    }
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Exceptions", "Home", new { message = ex.Message });
                }
            }

            using (var db = new Student_AttendanceEntities())
            {
                try
                {
                    // Query เอาข้อมูลนักศึกษาทุกคนที่ยังไม่ได้เช็คชื่อวันนี้ เพิ่อเอาไปเช็คชื่อครั้งใหม่ ในวันที่นอกเหนือจาก วันที่สอนปกติ
                    var dataNoCheck = (from sg in db.Tb_StudyGroup
                                       join st in db.Tb_Student on sg.StudyGroupID equals st.StudyGroupID
                                       where sg.StudyGroupID == id && st.StatusID == 5  // 5 = สถานะลงทะเบียน เท่านั้นที่มีสิทธิ์เช็คชื่อ
                                       orderby st.StdCode
                                       select new StudentCheckModel
                                       {
                                           StudyGroupID = sg.StudyGroupID,
                                           StdID = st.StdID,
                                           StdCode = st.StdCode,
                                           NameTH = st.NameTH,
                                       }).ToList();
                    //var today = "06/29/2017";
                    //var today = DateTime.Now.ToString("MM/dd/yyyy");
                    var today = DateTime.Now.ToShortDateString();
                    DateTime date = DateTime.Parse(_date);
                    // Query เอาข้อมูลการเช็คชื่อนักศึกษาทุกคนที่ได้เช็คชื่อวันนี้ เพิ่อเอาไปแก้การเช็คชื่อ ถ้ามีคนลงทะเบียนเพิ่มในวันนี้หลังจากเช็คชื่อไปแล้ว ไม่สามารถเช็คชื่อได้ ต้องรอให้พ้นวันนี้ไปก่อน
                    var dataIscheck = (from t1 in db.Tb_Student
                                       join t3 in db.Tb_StudentCheck on t1.StdID equals t3.StdID
                                       orderby t1.StdCode
                                       where t3.DateCheck == date && t3.StudyGroupID == id && t1.StatusID == 5
                                       select new StudentCheckModel()
                                       {
                                           StdCheckID = t3.StdCheckID,
                                           StudyGroupID = t3.StudyGroupID,
                                           StdID = t3.StdID,
                                           StdCode = t1.StdCode,
                                           NameTH = t1.NameTH,
                                           StatusID = t3.StatusID,
                                           Note = t3.Note
                                       }).ToList();

                    var datastatus = (from t1 in db.Tb_StudentCheck      // Query เอาสถานะการเช็คชื่อของวันนี้ ว่าเป็น มา สาย ลา ขาด ไหม เพิ่อเอาไปแก้การเช็คชื่อ
                                      join t2 in db.Tb_Student on t1.StdID equals t2.StdID
                                      where t1.StudyGroupID == id && t1.DateCheck == date
                                      select t1.StatusID).ToList();
                    if (datastatus.Count > 0) // ถ้าวันนี้มีการเช็คชื่อแล้วไปแล้วอย่างน้อย 1 ครั้ง
                    {
                        ViewBag.StdCheck = dataIscheck;             // ส่งข้อมูล Query dataIscheck ไปแก้ไขการเช็คชื่อ
                    }
                    else
                    {
                        ViewBag.StdCheck = dataNoCheck;             // มิฉะนั้นแล้ว ส่งข้อมูล Query dataNoCheck ไปการเช็คชื่อครั้งใหม่
                    }
                    var dataHeader = (from sg in db.Tb_StudyGroup        // Query เอาข้อมูลกลุ่มเรียน ไปแสดง ใน Header
                                      join sj in db.Tb_Subject on new { sg.SubjectCode, sg.Course } equals new { sj.SubjectCode, sj.Course }
                                      join sy in db.Tb_SchoolYear on sg.SchYearID equals sy.SchYearID
                                      where sg.StudyGroupID == id
                                      select new
                                      {
                                          StudyGroupID = sg.StudyGroupID,
                                          StudyGroupCode = sg.StudyGroupCode,
                                          SchYearID = sy.SchYearID,
                                          Term = sy.Term,
                                          Year = sy.Year,
                                          SubjectCode = sj.SubjectCode,
                                          SubjectName = sj.SubjectName,
                                          Course = sj.Course
                                      }).ToList();
                    ViewBag.StudyGroupID = dataHeader.Select(r => r.StudyGroupID).SingleOrDefault();
                    ViewBag.StudyGroupCode = dataHeader.Select(r => r.StudyGroupCode).SingleOrDefault();
                    ViewBag.Term = dataHeader.Select(r => r.Term).SingleOrDefault();
                    ViewBag.Year = dataHeader.Select(r => r.Year).SingleOrDefault();
                    ViewBag.SubjectCode = dataHeader.Select(r => r.SubjectCode).SingleOrDefault();
                    ViewBag.SubjectName = dataHeader.Select(r => r.SubjectName).SingleOrDefault();
                    ViewBag.Course = dataHeader.Select(r => r.Course).SingleOrDefault();
                    ViewBag.StudyGroup = dataHeader;
                    ViewBag.date = _date;
                    ViewBag.datetime = date.ToString("dd/MM/yyyy", Shared.CultureInfoTh);
                }
                catch (Exception ex)
                {
                    RedirectToAction("Exceptions", "Home", new { message = ex.Message });
                }
            }
            return View();
        } // นำข้อมูลที่มีการเช็คชื่อแล้วในวันนี้ หรือ ยังไม่ได้เช็คชื่อในวันนี้ไปแสดง เพื่อทำการเช็คชื่อ ใหม่ หรือ แก้ไขการเช็คชื่อ

        // NoteTeach Management CRUD LINQ TO SQL

        public JsonResult getNoteTeach(string id)
        {
            var jsonReturn = new JsonResponse();
            int groupID = int.Parse(id);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from r in db.Tb_NoteTeach
                                where r.StudyGroupID == groupID
                                orderby r.NoteID descending
                                select new
                                {
                                    r.NoteID,
                                    r.DateNote,
                                    r.NoteName,
                                    r.DetailNote,
                                    r.StudyGroupID
                                }).ToList();
                    List<ShowNoteModel> dataNote = new List<ShowNoteModel>();
                    foreach (var r in data)
                    {
                        dataNote.Add(new ShowNoteModel
                        {
                            NoteID = r.NoteID,
                            dateNote = r.DateNote.ToString("dd/MM/yyyy", Shared.CultureInfoTh),
                            NoteName = r.NoteName,
                            DetailNote = r.DetailNote,
                            StudyGroupID = r.StudyGroupID
                        });
                    }
                    if (data.Count > 0 && data != null)
                    {
                        jsonReturn = new JsonResponse { status = true, data = dataNote };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true, data = null };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }

            return Json(jsonReturn);
        } // แสดงข้อมูลบันทึกการสอนทั้งหมดของรายวิชา-กลุ่มเรียนนั้น จาก ล่าสุด >> ครั้งแรก

        public JsonResult SaveNoteTeach(NoteTeachModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.NoteID == 0)
                    {
                        var data = new Tb_NoteTeach()
                        {
                            DateNote = DateTime.Now,
                            NoteName = model.NoteName,
                            DetailNote = model.DetailNote,
                            StudyGroupID = model.StudyGroupID
                        };
                        db.Tb_NoteTeach.Add(data);
                    }
                    else
                    {
                        var data = db.Tb_NoteTeach.Where(r => r.NoteID == model.NoteID).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_NoteTeach.Where(r => r.NoteID == model.NoteID).ForEach(r =>
                            {
                                r.NoteName = model.NoteName;
                                r.DetailNote = model.DetailNote;
                            });
                        }
                    }
                    db.SaveChanges();
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อบแล้ว" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        } // บันทึกข้อมูล บันทึกการสอน

        public JsonResult UpdateNoteTeach(string _data)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                string[] str = _data.Split(',');
                int noteID = int.Parse(str[0]);
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_NoteTeach.Where(r => r.NoteID == noteID).Select(r => new
                    {
                        r.NoteID,
                        r.NoteName,
                        r.DetailNote
                    }).FirstOrDefault();
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
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        } // ส่งข้อมูลจาก บันทึกการสอน จาก DB ไปหน้า View เพื่อทำการแก้ไขชุดข้อมูลใน record นั้น

        public JsonResult DeleteNoteTeach(int _id)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_NoteTeach.Where(r => r.NoteID == _id).FirstOrDefault();
                    if (data != null)
                    {
                        db.Tb_NoteTeach.Remove(data);
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "ลบข้อมูลเรียบร้อยเแล้ว" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        } // ลบข้อมูลบันทึกการสอนใน record นั้น จากตาราง NoteTeach 

        public ActionResult STask(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var schYeardata = (from s in db.Tb_SchoolYear
                                   orderby s.Year descending
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = s.SchYearID,
                                       Term = s.Term,
                                       Year = s.Year
                                   }).ToList();
                ViewBag.SchYear = schYeardata;

                if (model.Query == null)
                {
                    var dataStdgroup = (from r in db.Tb_StudyGroup
                                        join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                        join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                        where (string.IsNullOrEmpty(model.Query) || y.SchYearID.ToString().Contains(model.Query))
                                        where (DateTime.Now >= y.StartDate && DateTime.Now <= y.EndDate) && r.UserID == UserLogon.UserID
                                        orderby r.StudyGroupCode ascending
                                        select new StudyGroupModel()
                                        {
                                            SchYearID = y.SchYearID,
                                            StudyGroupID = r.StudyGroupID,
                                            StudyGroupCode = r.StudyGroupCode,
                                            SubjectCode = s.SubjectCode,
                                            Course = s.Course,
                                            SubjectName = s.SubjectName,
                                        }).ToPagedList(model.page, 10);
                    ViewBag.studyGroup = dataStdgroup;
                    ViewBag.studyGroup1 = dataStdgroup.ToList();
                }
                else
                {
                    int schYear = int.Parse(model.Query);
                    var dataStdgroup = (from r in db.Tb_StudyGroup
                                        join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                        join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                        orderby r.StudyGroupCode ascending
                                        where r.SchYearID == schYear && r.UserID == UserLogon.UserID
                                        select new StudyGroupModel()
                                        {
                                            SchYearID = y.SchYearID,
                                            StudyGroupID = r.StudyGroupID,
                                            StudyGroupCode = r.StudyGroupCode,
                                            SubjectCode = s.SubjectCode,
                                            Course = s.Course,
                                            SubjectName = s.SubjectName,
                                        }).ToPagedList(model.page, 10);
                    ViewBag.studyGroup = dataStdgroup;
                    ViewBag.studyGroup1 = dataStdgroup.ToList();
                }

            }
            return View(model);
        }

        public ActionResult SingleTask(int id)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var dataHeader = (from r in db.Tb_StudyGroup
                                  join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                  where r.StudyGroupID == id
                                  select new StudyGroupModel
                                  {
                                      StudyGroupID = r.StudyGroupID,
                                      StudyGroupCode = r.StudyGroupCode,
                                      SubjectCode = s.SubjectCode,
                                      SubjectName = s.SubjectName,
                                      Course = s.Course
                                  }).FirstOrDefault();

                var dataSTask = (from t in db.Tb_Task
                                 join r in db.Tb_StudyGroup on t.StudyGroupID equals r.StudyGroupID
                                 join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                 where t.StudyGroupID == id && t.Type == 1
                                 orderby t.TaskID
                                 select new TaskModel()
                                 {
                                     TaskID = t.TaskID,
                                     TaskName = t.TaskName,
                                     FullScore = t.FullScore.Value,
                                     Note = t.Note,
                                     StudyGroupID = t.StudyGroupID.Value,
                                 }).ToList();
                ViewBag.SingleTask = dataSTask;
                string stdGrupCode = dataHeader.StudyGroupCode;
                string SubjCode = dataHeader.SubjectCode;
                string SubjName = dataHeader.SubjectName;
                string Course = dataHeader.Course;
                ViewBag.Header = "กลุ่มเรียน " + stdGrupCode + "  " + SubjCode + " " + SubjName + " (" + Course + ")";
                ViewBag.StudyGroupID = dataHeader.StudyGroupID;
            }
            return View();
        }

        public JsonResult SaveSingleTask(TaskModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.TaskID == 0)
                    {
                        var data = new Tb_Task()
                        {
                            Type = 1,
                            TaskName = model.TaskName,
                            FullScore = model.FullScore,
                            Note = model.Note,
                            StudyGroupID = model.StudyGroupID
                        };
                        db.Tb_Task.Add(data);
                        db.SaveChanges();
                        var dataStd = db.Tb_Student.Where(r => r.StudyGroupID == model.StudyGroupID).Select(r => r.StdID).ToList();
                        if (dataStd.Count > 0 && dataStd != null)
                        {
                            foreach (var r in dataStd)
                            {
                                var dataST = new Tb_SingleTask()
                                {
                                    TaskID = data.TaskID,
                                    StdID = r,
                                    Score = 0
                                };
                                db.Tb_SingleTask.Add(dataST);
                            }
                        }
                    }
                    else
                    {
                        var data = db.Tb_Task.Where(r => r.TaskID == model.TaskID).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Task.Where(r => r.TaskID == model.TaskID).ForEach(r =>
                            {
                                r.TaskName = model.TaskName;
                                r.FullScore = model.FullScore;
                                r.Note = model.Note;
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

        public JsonResult UpdateSingleTask(int taskid)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Task.Where(r => r.TaskID == taskid).Select(r => new TaskModel()
                    {
                        TaskID = r.TaskID,
                        TaskName = r.TaskName,
                        FullScore = r.FullScore.Value,
                        StudyGroupID = r.StudyGroupID.Value,
                        Note = r.Note
                    }).FirstOrDefault();
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
                jsonReturn = new JsonResponse { status = false, message = ex.Message };

            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteSingleTask(int taskid)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data1 = db.Tb_SingleTask.Where(r => r.TaskID == taskid).ToList();
                    if (data1.Count > 0)
                    {
                        foreach (var r in data1)
                        {
                            db.Tb_SingleTask.Remove(r);
                        }
                    }
                    db.SaveChanges();
                    var data = db.Tb_Task.Where(r => r.TaskID == taskid).FirstOrDefault();
                    if (data != null)
                    {
                        db.Tb_Task.Remove(data);
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

        public ActionResult STaskScore(int Gid, int taskID)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var dataTask = (from r in db.Tb_Task
                                join s in db.Tb_StudyGroup on r.StudyGroupID equals s.StudyGroupID
                                where r.TaskID == taskID && s.StudyGroupID == Gid
                                select new TaskModel
                                {
                                    TaskName = r.TaskName,
                                    StudyGroupCode = s.StudyGroupCode
                                }).FirstOrDefault();
                ViewBag.Header = "ชื่องาน : " + dataTask.TaskName;
                ViewBag.Header1 = "กลุ่มเรียน : " + dataTask.StudyGroupCode;
                var data = (from r in db.Tb_Student
                            join s in db.Tb_StudyGroup on r.StudyGroupID equals s.StudyGroupID
                            join st in db.Tb_SingleTask on r.StdID equals st.StdID
                            join t in db.Tb_Task on st.TaskID equals t.TaskID
                            where s.StudyGroupID == Gid && st.TaskID == taskID && r.StatusID == 5
                            select new SingleTaskModel
                            {
                                TaskID = st.TaskID,
                                StdID = r.StdID,
                                StdCode = r.StdCode,
                                NameTH = r.NameTH,
                                Score = st.Score,
                                FullScore = (int)t.FullScore
                            }).ToList();
                ViewBag.singleTaskScore = data;
                ViewBag.studyGroupid = Gid;
                ViewBag.FullScore = data.Select(r => r.FullScore).FirstOrDefault();
            }
            return View();
        }

        public JsonResult getSingleTaskScore(string _data)
        {
            var jsonReturn = new JsonResponse();
            string[] str = _data.Split(',');
            int taskID = int.Parse(str[0]);
            int Gid = int.Parse(str[1]);
            using (var db = new Student_AttendanceEntities())
            {
                var data = (from r in db.Tb_Student
                            join s in db.Tb_StudyGroup on r.StudyGroupID equals s.StudyGroupID
                            join st in db.Tb_SingleTask on r.StdID equals st.StdID
                            join t in db.Tb_Task on st.TaskID equals t.TaskID
                            where s.StudyGroupID == Gid && st.TaskID == taskID && r.StatusID == 5
                            select new SingleTaskModel
                            {
                                TaskID = st.TaskID,
                                StdID = r.StdID,
                                StdCode = r.StdCode,
                                NameTH = r.NameTH,
                                Score = st.Score,
                                FullScore = (int)t.FullScore
                            }).ToList();
                if (data != null)
                {
                    jsonReturn = new JsonResponse { status = true, data = data };
                }
                else
                {
                    jsonReturn = new JsonResponse { status = true, message = "ไม่พบข้อมูล" };
                }
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateSingleTaskScore(string _data)
        {
            var jsonReturn = new JsonResponse();
            string[] str = _data.Split(',');
            int stdID = int.Parse(str[0]);
            int taskID = int.Parse(str[1]);
            int fullScore = int.Parse(str[2]);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = (from st in db.Tb_SingleTask
                                join t in db.Tb_Task on st.TaskID equals t.TaskID
                                where st.StdID == stdID && st.TaskID == taskID
                                select new SingleTaskModel
                                {
                                    TaskID = st.TaskID,
                                    StdID = st.StdID,
                                    Score = st.Score,
                                    FullScore = (int)t.FullScore
                                }).FirstOrDefault();
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
                jsonReturn = new JsonResponse { status = false, message = ex.Message };

            }
            return Json(jsonReturn);
        }

        public JsonResult SaveSingleTaskScore(SingleTaskModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model != null)
                    {
                        var data = db.Tb_SingleTask.Where(r => r.TaskID == model.TaskID && r.StdID == model.StdID).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_SingleTask.Where(r => r.TaskID == model.TaskID && r.StdID == model.StdID).ForEach(r =>
                            {
                                r.Score = model.Score;
                            });
                            db.SaveChanges();
                            jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                        }
                        else
                        {
                            jsonReturn = new JsonResponse { status = false, message = "ไม่พบข้อมูล" };
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public ActionResult GTask(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var schYeardata = (from s in db.Tb_SchoolYear
                                   orderby s.Year descending
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = s.SchYearID,
                                       Term = s.Term,
                                       Year = s.Year
                                   }).ToList();
                ViewBag.SchYear = schYeardata;

                if (model.Query == null)
                {
                    var dataStdgroup = (from r in db.Tb_StudyGroup
                                        join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                        join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                        where (string.IsNullOrEmpty(model.Query) || y.SchYearID.ToString().Contains(model.Query))
                                        where (DateTime.Now >= y.StartDate && DateTime.Now <= y.EndDate) && r.UserID == UserLogon.UserID
                                        orderby r.StudyGroupCode ascending
                                        select new StudyGroupModel()
                                        {
                                            SchYearID = y.SchYearID,
                                            StudyGroupID = r.StudyGroupID,
                                            StudyGroupCode = r.StudyGroupCode,
                                            SubjectCode = s.SubjectCode,
                                            Course = s.Course,
                                            SubjectName = s.SubjectName,
                                        }).ToPagedList(model.page, 10);
                    ViewBag.studyGroup = dataStdgroup;
                    ViewBag.studyGroup1 = dataStdgroup.ToList();
                }
                else
                {
                    int schYear = int.Parse(model.Query);
                    var dataStdgroup = (from r in db.Tb_StudyGroup
                                        join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                        join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                        orderby r.StudyGroupCode ascending
                                        where r.SchYearID == schYear && r.UserID == UserLogon.UserID
                                        select new StudyGroupModel()
                                        {
                                            SchYearID = y.SchYearID,
                                            StudyGroupID = r.StudyGroupID,
                                            StudyGroupCode = r.StudyGroupCode,
                                            SubjectCode = s.SubjectCode,
                                            Course = s.Course,
                                            SubjectName = s.SubjectName,
                                        }).ToPagedList(model.page, 10);
                    ViewBag.studyGroup = dataStdgroup;
                    ViewBag.studyGroup1 = dataStdgroup.ToList();
                }

            }
            return View(model);
        }

        public ActionResult GroupTask(int id)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var dataHeader = (from r in db.Tb_StudyGroup
                                  join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                  where r.StudyGroupID == id
                                  select new StudyGroupModel
                                  {
                                      StudyGroupID = r.StudyGroupID,
                                      StudyGroupCode = r.StudyGroupCode,
                                      SubjectCode = s.SubjectCode,
                                      SubjectName = s.SubjectName,
                                      Course = s.Course
                                  }).FirstOrDefault();

                var dataGTask = (from t in db.Tb_Task
                                 join r in db.Tb_StudyGroup on t.StudyGroupID equals r.StudyGroupID
                                 join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                 where t.StudyGroupID == id && t.Type == 2
                                 orderby t.TaskID
                                 select new TaskModel()
                                 {
                                     TaskID = t.TaskID,
                                     TaskName = t.TaskName,
                                     FullScore = t.FullScore.Value,
                                     Note = t.Note,
                                     StudyGroupID = t.StudyGroupID.Value,
                                 }).ToList();
                ViewBag.GroupTask = dataGTask;
                string stdGrupCode = dataHeader.StudyGroupCode;
                string SubjCode = dataHeader.SubjectCode;
                string SubjName = dataHeader.SubjectName;
                string Course = dataHeader.Course;
                ViewBag.Header = "กลุ่มเรียน " + stdGrupCode + "  " + SubjCode + " " + SubjName + " (" + Course + ")";
                ViewBag.StudyGroupID = dataHeader.StudyGroupID;
            }
            return View();
        }

        public JsonResult SaveGroupTask(TaskModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.TaskID == 0)
                    {
                        var data = new Tb_Task()
                        {
                            Type = 2,
                            TaskName = model.TaskName,
                            FullScore = model.FullScore,
                            Note = model.Note,
                            StudyGroupID = model.StudyGroupID
                        };
                        db.Tb_Task.Add(data);
                        db.SaveChanges();
                        for (int i = 1; i <= model.quantityGroup; i++)
                        {
                            var dataGroup = new Tb_Group()
                            {
                                GroupName = "กลุ่มที่ " + i,
                                TaskID = data.TaskID,
                                StudyGroupID = model.StudyGroupID
                            };
                            db.Tb_Group.Add(dataGroup);
                        }
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                    }
                    else
                    {
                        var data = db.Tb_Task.Where(r => r.TaskID == model.TaskID).FirstOrDefault();
                        if (data != null)
                        {
                            db.Tb_Task.Where(r => r.TaskID == model.TaskID).ForEach(r =>
                            {
                                r.TaskName = model.TaskName;
                                r.FullScore = model.FullScore;
                                r.Note = model.Note;
                            });
                            db.SaveChanges();
                            jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                            var dataGroup = db.Tb_Group.Where(r => r.TaskID == model.TaskID).ToArray();
                            if (model.quantityGroup > dataGroup.Length)
                            {
                                for (int i = dataGroup.Length + 1; i <= model.quantityGroup; i++)
                                {
                                    var datanew = new Tb_Group()
                                    {
                                        GroupName = "กลุ่มที่ " + i,
                                        TaskID = data.TaskID,
                                        StudyGroupID = model.StudyGroupID
                                    };
                                    db.Tb_Group.Add(datanew);
                                }
                                db.SaveChanges();
                                jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                            }
                            else if (model.quantityGroup < dataGroup.Length)
                            {
                                for (int i = dataGroup.Length; i > model.quantityGroup; i--)
                                {
                                    int groupID = dataGroup[i - 1].GroupID;
                                    var dataGT = db.Tb_GroupTask.Where(r => r.GroupID == groupID).ToList();
                                    if (dataGT.Count > 0 && dataGT != null)
                                    {
                                        foreach (var r in dataGT)
                                        {
                                            db.Tb_GroupTask.Remove(r);
                                        }
                                    }
                                }
                                db.SaveChanges();
                                for (int i = dataGroup.Length; i > model.quantityGroup; i--)
                                {
                                    int groupID = dataGroup[i - 1].GroupID;
                                    var dataG = db.Tb_Group.Where(r => r.GroupID == groupID).FirstOrDefault();
                                    if (dataG != null)
                                    {
                                        db.Tb_Group.Remove(dataG);
                                    }
                                }
                                db.SaveChanges();
                                jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult UpdateGroupTask(int taskid)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var countGroup = db.Tb_Group.Where(r => r.TaskID == taskid).Count();
                    var data = db.Tb_Task.Where(r => r.TaskID == taskid).Select(r => new TaskModel()
                    {
                        TaskID = r.TaskID,
                        TaskName = r.TaskName,
                        FullScore = r.FullScore.Value,
                        StudyGroupID = r.StudyGroupID.Value,
                        quantityGroup = countGroup,
                        Note = r.Note
                    }).FirstOrDefault();
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
                jsonReturn = new JsonResponse { status = false, message = ex.Message };

            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteGroupTask(int taskid)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var dataGID = db.Tb_Group.Where(r => r.TaskID == taskid).Select(r => r.GroupID).ToList();
                    if (dataGID.Count > 0)
                    {
                        foreach (var r in dataGID)
                        {
                            var gtask = db.Tb_GroupTask.Where(x => x.GroupID == r).FirstOrDefault();
                            if (gtask != null)
                            {
                                db.Tb_GroupTask.Remove(gtask);
                            }
                        }
                        db.SaveChanges();
                    }
                    var dataG = db.Tb_Group.Where(r => r.TaskID == taskid).ToList();
                    if (dataG.Count > 0)
                    {
                        foreach (var r in dataG)
                        {
                            db.Tb_Group.Remove(r);
                        }
                    }
                    db.SaveChanges();
                    var data = db.Tb_Task.Where(r => r.TaskID == taskid).FirstOrDefault();
                    if (data != null)
                    {
                        db.Tb_Task.Remove(data);
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

        public ActionResult GTaskScore(int Gid, int taskID)
        {
            using (var db = new Student_AttendanceEntities())
            {
                ViewBag.key = Gid + "," + taskID;
                var task = db.Tb_Task.Where(r => r.TaskID == taskID).FirstOrDefault();
                ViewBag.TaskName = task.TaskName;
                ViewBag.TaskNote = task.Note;
                var data1 = (from r in db.Tb_Group
                             join t in db.Tb_Task on r.TaskID equals t.TaskID
                             where r.TaskID == taskID && r.StudyGroupID == Gid
                             orderby r.GroupID
                             select new GroupTaskModel
                             {
                                 GroupID = r.GroupID,
                                 GroupName = r.GroupName,
                                 StudyGroupID = (int)r.StudyGroupID,
                                 FullScore = (int)t.FullScore,
                                 TaskID = taskID
                             }).OrderBy(r => r.GroupID).Distinct().ToList();
                ViewBag.Group = data1;
                int fullScore = data1.Select(r => r.FullScore).FirstOrDefault();
                var data3 = new List<GroupTaskModel>();
                if (data1.Count > 0)
                {
                    foreach (var r in data1)
                    {
                        var a = (from x in db.Tb_GroupTask
                                 join s in db.Tb_Student on x.StdID equals s.StdID
                                 where x.GroupID == r.GroupID && s.StatusID == 5
                                 orderby x.GroupID
                                 select new GroupTaskModel
                                 {
                                     GroupID = x.GroupID,
                                     StdID = x.StdID,
                                     NameTH = s.NameTH,
                                     Score = (double)x.Score,
                                     FullScore = fullScore
                                 }).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var c in a)
                            {
                                data3.Add(new GroupTaskModel
                                {
                                    GroupID = c.GroupID,
                                    StdID = c.StdID,
                                    NameTH = c.NameTH,
                                    Score = (double)c.Score,
                                    FullScore = c.FullScore
                                });
                            }
                        }
                    }
                }
                ViewBag.GroupTask = data3;
            }
            return View();
        }

        public JsonResult SaveAddGroup(string valueKey, string groupName)
        {
            var jsonReturn = new JsonResponse();
            string[] str = valueKey.Split(',');
            int studyGroupID = int.Parse(str[0]);
            int taskID = int.Parse(str[1]);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = new Tb_Group()
                    {
                        GroupName = groupName,
                        StudyGroupID = studyGroupID,
                        TaskID = taskID
                    };
                    db.Tb_Group.Add(data);
                    db.SaveChanges();
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult getStudentToGroupTask(string valueKey)
        {
            var jsonReturn = new JsonResponse();
            string[] str = valueKey.Split(',');
            int groupID = int.Parse(str[0]);
            int studyGroupID = int.Parse(str[1]);
            int taskID = int.Parse(str[2]);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data1 = db.Tb_Student.Where(r => r.StudyGroupID == studyGroupID && r.StatusID == 5).Select(r => r.StdID).ToList();
                    var data2 = (from r in db.Tb_Group
                                 join g in db.Tb_GroupTask on r.GroupID equals g.GroupID
                                 where r.TaskID == taskID && r.StudyGroupID == studyGroupID
                                 select g.StdID).ToList();
                    var data3 = data1.Where(r => !data2.Contains(r)).ToList();
                    var data4 = db.Tb_Student.Where(r => data3.Contains(r.StdID)).Select(r => new
                    {
                        GroupID = groupID,
                        StdID = r.StdID,
                        StdCode = r.StdCode,
                        NameTH = r.NameTH
                    }).ToList();
                    jsonReturn = new JsonResponse { status = true, data = data4 };
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }

            return Json(jsonReturn);
        }

        public JsonResult SaveAddStdToGroupTask(List<GroupTaskModel> model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model.Count > 0)
                    {
                        foreach (var r in model)
                        {
                            var data = new Tb_GroupTask()
                            {
                                GroupID = r.GroupID,
                                StdID = r.StdID,
                                Score = 0
                            };
                            db.Tb_GroupTask.Add(data);
                        }
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อย" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult getGroupName(int groupID)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var groupName = db.Tb_Group.Where(r => r.GroupID == groupID).Select(r => r.GroupName).FirstOrDefault();
                    if (groupName != null)
                    {
                        jsonReturn = new JsonResponse { status = true, data = groupName };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = false, message = "ไม่พบข้อมูล" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult SaveGroupName(GroupModel model)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data = db.Tb_Group.Where(r => r.GroupID == model.GroupID).FirstOrDefault();
                    if (data != null)
                    {
                        db.Tb_Group.Where(r => r.GroupID == model.GroupID).ForEach(r =>
                        {
                            r.GroupName = model.GroupName;
                        });
                        db.SaveChanges();
                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult DeleteGroup(int groupID)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var data1 = db.Tb_GroupTask.Where(r => r.GroupID == groupID).ToList();
                    if (data1.Count > 0)
                    {
                        foreach (var r in data1)
                        {
                            db.Tb_GroupTask.Remove(r);
                        }
                        db.SaveChanges();
                    }
                    var data = db.Tb_Group.Where(r => r.GroupID == groupID).FirstOrDefault();
                    if (data != null)
                    {
                        db.Tb_Group.Remove(data);
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

        public JsonResult getGroupTask(string key)
        {
            var jsonReturn = new JsonResponse();
            string[] str = key.Split(',');
            int groupID = int.Parse(str[0]);
            int taskID = int.Parse(str[1]);
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    var fullscore = db.Tb_Task.Where(r => r.TaskID == taskID).Select(r => r.FullScore).FirstOrDefault();
                    var data = (from r in db.Tb_GroupTask
                                join s in db.Tb_Student on r.StdID equals s.StdID
                                where r.GroupID == groupID && s.StatusID == 5
                                orderby r.StdID
                                select new GroupTaskModel
                                {
                                    GroupID = r.GroupID,
                                    StdID = r.StdID,
                                    NameTH = s.NameTH,
                                    Score = (double)r.Score,
                                    FullScore = (int)fullscore
                                }).ToList();
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
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        public JsonResult SaveGroupTaskScore(List<GroupTaskModel> model)
        {
            var jsonReturn = new JsonResponse();
            var valid = false;
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    if (model != null)
                    {
                        foreach (var s in model)
                        {
                            if (s.Score <= s.FullScore)
                            {
                                valid = true;
                                var data = db.Tb_GroupTask.Where(r => r.GroupID == s.GroupID && r.StdID == s.StdID).ToList();
                                if (data != null)
                                {
                                    db.Tb_GroupTask.Where(r => r.GroupID == s.GroupID && r.StdID == s.StdID).ForEach(r =>
                                    {
                                        r.Score = s.Score;
                                    });
                                }
                            }
                            else
                            {
                                valid = false;
                                break;
                            }
                        }
                        if (valid)
                        {
                            db.SaveChanges();
                            jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยแล้ว" };
                        }
                        else
                        {
                            jsonReturn = new JsonResponse { status = false, message = "คะแนนเกิน! คะแนนเต็ม" };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        }

        // summary No Exam And summary list of Check 

        public ActionResult SummaryNoExam(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var schYeardata = (from s in db.Tb_SchoolYear
                                   orderby s.Year descending
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = s.SchYearID,
                                       Term = s.Term,
                                       Year = s.Year
                                   }).ToList();
                ViewBag.SchYear = schYeardata;

                var endDate = db.Tb_SchoolYear.Where(r => r.StartDate <= DateTime.Now && DateTime.Now <= r.EndDate).Select(r => r.EndDate).FirstOrDefault();
                endDate = endDate.AddDays(7);
                if (model.Query == null)
                {
                    var dataSummaryNoExam = (from r in db.Tb_StudyGroup
                                             join d in db.Tb_Department on r.DeptCode equals d.DeptCode
                                             join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                             join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                             where (string.IsNullOrEmpty(model.Query) || y.SchYearID.ToString().Contains(model.Query))
                                             where (DateTime.Now >= y.StartDate && DateTime.Now <= endDate) && r.UserID == UserLogon.UserID
                                             orderby r.StudyGroupCode ascending
                                             select new SummaryModel()
                                             {
                                                 SchYearID = y.SchYearID,
                                                 Term = y.Term,
                                                 Year = y.Year,
                                                 StudyGroupID = r.StudyGroupID,
                                                 StudyGroupCode = r.StudyGroupCode,
                                                 SubjectCode = s.SubjectCode,
                                                 Course = s.Course,
                                                 SubjectName = s.SubjectName,
                                                 SubjectPractice = s.SubjectPractice,
                                                 SubjectTheory = s.SubjectTheory,
                                                 DeptName = d.DeptName
                                             }).ToList();
                    ViewBag.Summary = dataSummaryNoExam;
                }
                else
                {
                    int schYear = int.Parse(model.Query);
                    var dataSummaryNoExam = (from r in db.Tb_StudyGroup
                                             join d in db.Tb_Department on r.DeptCode equals d.DeptCode
                                             join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                             join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                             orderby r.StudyGroupCode ascending
                                             where r.SchYearID == schYear && r.UserID == UserLogon.UserID
                                             select new SummaryModel()
                                             {
                                                 SchYearID = y.SchYearID,
                                                 Term = y.Term,
                                                 Year = y.Year,
                                                 StudyGroupID = r.StudyGroupID,
                                                 StudyGroupCode = r.StudyGroupCode,
                                                 SubjectCode = s.SubjectCode,
                                                 Course = s.Course,
                                                 SubjectName = s.SubjectName,
                                                 DeptName = d.DeptName
                                             }).ToList();
                    ViewBag.Summary = dataSummaryNoExam;
                }

            }
            return View(model);
        } // ข้อมูล สรุปรายชื่อนักศึกษาที่ไม่มีสิทธิ์สอบ ตามกลุ่มเรียน ปีการศึกาษา

        public ActionResult SummaryListCheck(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var schYeardata = (from s in db.Tb_SchoolYear
                                   orderby s.Year descending
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = s.SchYearID,
                                       Term = s.Term,
                                       Year = s.Year
                                   }).ToList();
                ViewBag.SchYear = schYeardata;
                var endDate = db.Tb_SchoolYear.Where(r => r.StartDate <= DateTime.Now && DateTime.Now <= r.EndDate).Select(r => r.EndDate).FirstOrDefault();
                endDate = endDate.AddDays(7);
                if (model.Query == null)
                {
                    var dataSummaryCheck = (from r in db.Tb_StudyGroup
                                            join d in db.Tb_Department on r.DeptCode equals d.DeptCode
                                            join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                            join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                            where (string.IsNullOrEmpty(model.Query) || y.SchYearID.ToString().Contains(model.Query))
                                            where (DateTime.Now >= y.StartDate && DateTime.Now <= endDate) && r.UserID == UserLogon.UserID
                                            orderby r.StudyGroupCode ascending
                                            select new SummaryModel()
                                            {
                                                SchYearID = y.SchYearID,
                                                Term = y.Term,
                                                Year = y.Year,
                                                StudyGroupID = r.StudyGroupID,
                                                StudyGroupCode = r.StudyGroupCode,
                                                SubjectCode = s.SubjectCode,
                                                Course = s.Course,
                                                SubjectName = s.SubjectName,
                                                SubjectPractice = s.SubjectPractice,
                                                SubjectTheory = s.SubjectTheory,
                                                DeptName = d.DeptName
                                            }).ToList();
                    ViewBag.SummaryCheck = dataSummaryCheck;
                }
                else
                {
                    int schYear = int.Parse(model.Query);
                    var dataSummaryCheck = (from r in db.Tb_StudyGroup
                                            join d in db.Tb_Department on r.DeptCode equals d.DeptCode
                                            join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                            join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                            orderby r.StudyGroupCode ascending
                                            where r.SchYearID == schYear && r.UserID == UserLogon.UserID
                                            select new SummaryModel()
                                            {
                                                SchYearID = y.SchYearID,
                                                Term = y.Term,
                                                Year = y.Year,
                                                StudyGroupID = r.StudyGroupID,
                                                StudyGroupCode = r.StudyGroupCode,
                                                SubjectCode = s.SubjectCode,
                                                Course = s.Course,
                                                SubjectName = s.SubjectName,
                                                DeptName = d.DeptName
                                            }).ToList();
                    ViewBag.SummaryCheck = dataSummaryCheck;
                }
            }
            return View(model);
        }  // ข้อมูล สรุปผลการเช็คชื่อ ตามกลุ่มเรียน ปีการศึกษา

        public ActionResult SummaryTask(FilterModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var schYeardata = (from s in db.Tb_SchoolYear
                                   orderby s.Year descending
                                   select new SchoolYearModel()
                                   {
                                       SchYearID = s.SchYearID,
                                       Term = s.Term,
                                       Year = s.Year
                                   }).ToList();
                ViewBag.SchYear = schYeardata;
                var endDate = db.Tb_SchoolYear.Where(r => r.StartDate <= DateTime.Now && DateTime.Now <= r.EndDate).Select(r => r.EndDate).FirstOrDefault();
                endDate = endDate.AddDays(7);
                if (model.Query == null)
                {
                    var dataTask = (from r in db.Tb_StudyGroup
                                    join d in db.Tb_Department on r.DeptCode equals d.DeptCode
                                    join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                    join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                    where (string.IsNullOrEmpty(model.Query) || y.SchYearID.ToString().Contains(model.Query))
                                    where (DateTime.Now >= y.StartDate && DateTime.Now <= endDate) && r.UserID == UserLogon.UserID
                                    orderby r.StudyGroupCode ascending
                                    select new SummaryModel()
                                    {
                                        SchYearID = y.SchYearID,
                                        Term = y.Term,
                                        Year = y.Year,
                                        StudyGroupID = r.StudyGroupID,
                                        StudyGroupCode = r.StudyGroupCode,
                                        SubjectCode = s.SubjectCode,
                                        Course = s.Course,
                                        SubjectName = s.SubjectName,
                                        SubjectPractice = s.SubjectPractice,
                                        SubjectTheory = s.SubjectTheory,
                                        DeptName = d.DeptName
                                    }).ToList();
                    ViewBag.SummaryTask = dataTask;
                }
                else
                {
                    int schYear = int.Parse(model.Query);
                    var dataTask = (from r in db.Tb_StudyGroup
                                    join d in db.Tb_Department on r.DeptCode equals d.DeptCode
                                    join s in db.Tb_Subject on new { r.SubjectCode, r.Course } equals new { s.SubjectCode, s.Course }
                                    join y in db.Tb_SchoolYear on r.SchYearID equals y.SchYearID
                                    orderby r.StudyGroupCode ascending
                                    where r.SchYearID == schYear && r.UserID == UserLogon.UserID
                                    select new SummaryModel()
                                    {
                                        SchYearID = y.SchYearID,
                                        Term = y.Term,
                                        Year = y.Year,
                                        StudyGroupID = r.StudyGroupID,
                                        StudyGroupCode = r.StudyGroupCode,
                                        SubjectCode = s.SubjectCode,
                                        Course = s.Course,
                                        SubjectName = s.SubjectName,
                                        DeptName = d.DeptName
                                    }).ToList();
                    ViewBag.SummaryTask = dataTask;
                }
            }
            return View(model);
        }  // ข้อมูล สรุปคะแนนการมอบมายงาน

        public ActionResult DetailSummaryListCheck(SummaryModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var dataHeader = (from t1 in db.Tb_SchoolYear
                                  join t2 in db.Tb_StudyGroup on t1.SchYearID equals t2.SchYearID
                                  join t3 in db.Tb_Subject on new { t2.SubjectCode, t2.Course } equals new { t3.SubjectCode, t3.Course }
                                  join t4 in db.Tb_Student on t2.StudyGroupID equals t4.StudyGroupID
                                  join t5 in db.Tb_Department on t2.DeptCode equals t5.DeptCode
                                  where t1.SchYearID == model.SchYearID && t2.StudyGroupID == model.StudyGroupID && t3.SubjectCode == model.SubjectCode && t3.Course == model.Course
                                  orderby t4.StdCode
                                  select new SummaryModel
                                  {
                                      Term = t1.Term,
                                      Year = t1.Year,
                                      DeptName = t5.DeptName,
                                      SubjectCode = t3.SubjectCode,
                                      SubjectName = t3.SubjectName,
                                      Course = t3.Course,
                                      StudyGroupCode = t2.StudyGroupCode,
                                      StudyGroupID = t2.StudyGroupID
                                  }).ToList();
                ViewBag.Header = dataHeader;

                var dataDate = (from r in db.Tb_StudentCheck   // วันที่ทั้งหมดที่เช็คชื่อ
                                where r.StudyGroupID == model.StudyGroupID
                                orderby r.DateCheck ascending
                                select new DisplaySumModel()
                                {
                                    DateCheck = r.DateCheck,
                                }).OrderBy(r => r.DateCheck).Distinct().ToList();
                ViewBag.DateCheck = dataDate;

                var dataStudent = (from r in db.Tb_Student  // รายชื่อนักศึกษาในกลุ่มเรียน 
                                   where r.StudyGroupID == model.StudyGroupID && r.StatusID == 5 // 5 = สถานะลงทะเบียนเรียน
                                   orderby r.StdCode
                                   select new StudentModel()
                                   {
                                       StdID = r.StdID,
                                       StdCode = r.StdCode,
                                       NameTH = r.NameTH,
                                       StudyGroupID = r.StudyGroupID
                                   }).ToList(); // ชุดแรก
                ViewBag.Student = dataStudent;

                var status = (from r in db.Tb_StudentCheck // สถานะของแต่รายชื่อ
                              where r.StudyGroupID == model.StudyGroupID
                              orderby r.StdID, r.DateCheck
                              select new StudentCheckModel
                              {
                                  StdID = r.StdID,
                                  StatusID = r.StatusID,
                                  DateCheck = r.DateCheck
                              }).ToList();
                ViewBag.DataStatus = status;

                var dateHoliday = (from r in db.Tb_StudentCheck   // วันที่หยุด
                                   where r.StudyGroupID == model.StudyGroupID && (r.StatusID == 7 || r.StatusID == 8)
                                   orderby r.DateCheck ascending
                                   select new DisplaySumModel()
                                   {
                                       DateCheck = r.DateCheck,
                                       Note = r.Note
                                   }).Distinct().ToList();
                ViewBag.DateHoliday = dateHoliday;
            }
            return View();
        } // แสดงรายละเอียดกสรุปผลารเช็คชื่อตามรายวิชา-กลุ่มเรียน

        public ActionResult ExportSummaryListCheck(StudyGroupModel model)
        {
            string[] strGroup = model.StudyGroupCode.Split('.');
            string fileName = "SummaryListCheck_" + model.SubjectCode + "-" + strGroup[0] + strGroup[1] + ".xlsx";
            var dataHeader = new object();
            int countDate = 0;
            int countStudent = 0;

            using (var db = new Student_AttendanceEntities())
            {
                dataHeader = (from t1 in db.Tb_SchoolYear
                              join t2 in db.Tb_StudyGroup on t1.SchYearID equals t2.SchYearID
                              join t3 in db.Tb_Subject on new { t2.SubjectCode, t2.Course } equals new { t3.SubjectCode, t3.Course }
                              join t4 in db.Tb_Student on t2.StudyGroupID equals t4.StudyGroupID
                              join t5 in db.Tb_Department on t2.DeptCode equals t5.DeptCode
                              where t1.SchYearID == model.SchYearID && t2.StudyGroupID == model.StudyGroupID && t3.SubjectCode == model.SubjectCode && t3.Course == model.Course && t2.UserID == UserLogon.UserID
                              orderby t4.StdCode
                              select new SummaryModel
                              {
                                  Term = t1.Term,
                                  Year = t1.Year,
                                  DeptName = t5.DeptName,
                                  SubjectCode = t3.SubjectCode,
                                  SubjectName = t3.SubjectName,
                                  Course = t3.Course,
                                  StudyGroupCode = t2.StudyGroupCode
                              }).ToList();
            }

            using (var package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("Sheet1");
                ExcelWorksheet ws = package.Workbook.Worksheets[1];
                // ws.Name = "Test"; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 14; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "TH SarabunPSK"; //Default Font name for whole sheet

                //ws.Cells[1, 1].Value = "Sample DataTable Export"; // Heading Name
                //ws.Cells[1, 1, 1, 10].Merge = true; //Merge columns start and end range
                //ws.Cells[1, 1, 1, 10].Style.Font.Bold = true; //Font should be bold
                //ws.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center
                var imgPath = Server.MapPath("~/icon/Logo_RMUTI_128X235.png");
                Bitmap image = new Bitmap(imgPath);
                ExcelPicture pic = ws.Drawings.AddPicture("Logo_RMUTI", image);
                pic.SetPosition(5, 5);
                pic.SetSize(48, (int)(235 * 0.347));


                //Header Page 
                ws.Cells["A1:I1"].Value = "                มหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A1:I1"].Merge = true;                                                 // Merge = ผสานเซลล์และจัดกึ่งกลาง
                ws.Cells["A1:I1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;    // จัดตำแหน่งอักษรแนวนอน 

                ws.Cells["A2:I2"].Value = "                ศูนย์กลางมหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A2:I2"].Merge = true;
                ws.Cells["A2:I2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                ws.Cells["A4:L4"].Value = "รายชื่อนักศึกษาตามรายวิชา - กลุ่มเรียน";
                ws.Cells["A4:L4"].Merge = true;
                ws.Cells["A4:L4"].Style.Font.Bold = true;
                ws.Cells["A4:L4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                foreach (var item in (List<SummaryModel>)dataHeader)
                {
                    ws.Cells["A6:B6"].Value = "ประจำภาคการศึกษา " + item.Term + "/" + item.Year;
                    ws.Cells["A6:B6"].Merge = true;
                    ws.Cells["A6:B6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["D6:L6"].Value = item.DeptName; // สาขาวิชา
                    ws.Cells["D6:L6"].Merge = true;
                    ws.Cells["D6:L6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["A7:E7"].Value = "รายวิชา " + item.SubjectCode + " | " + item.SubjectName;
                    ws.Cells["A7:E7"].Merge = true;
                    ws.Cells["A7:E7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["G7:L7"].Value = "กลุ่มเรียน " + item.StudyGroupCode;
                    ws.Cells["G7:L7"].Merge = true;
                    ws.Cells["G7:L7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    break;
                }
                //End Header Page 

                // Haeder Table

                int rowIndex = 9;  // หัวตาราง เริ่มบรรทัดที่ 9
                string[] statusText = { "มา", "สาย", "ลา", "ขาด", "รวม" };
                int j = 0;
                int k = 0;
                using (var db = new Student_AttendanceEntities())
                {
                    var dataDate = (from r in db.Tb_StudentCheck
                                    where r.StudyGroupID == model.StudyGroupID
                                    orderby r.DateCheck ascending
                                    select r.DateCheck).Distinct().ToArray();
                    countDate = dataDate.Length;

                    for (int col = 1; col <= 2 + countDate + 5; col++)
                    {
                        ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[rowIndex, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //ws.Cells[rowIndex, col].Style.Font.Bold = false;
                        //ws.Cells[rowIndex, col].AutoFitColumns();
                        switch (col)
                        {
                            case 1:
                                ws.Cells[rowIndex, col].Value = "รหัสนักศึกษา";
                                break;
                            case 2:
                                ws.Cells[rowIndex, col].Value = "ชื่อ-สกุล";
                                break;
                            default:
                                {
                                    if (col > 2 && col <= countDate + 2)  // แสดงวันที่มีการเช็คชื่อในแต่ละคอลัมน์ 
                                    {
                                        ws.Cells[rowIndex, col].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;    // จัดตำแหน่งอักษรแนวตั้ง
                                        ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[rowIndex, col].AutoFitColumns(4);                                          // จัดความกว้างคอลัมน์ parameter หน่วย = picxel
                                        ws.Cells[rowIndex, col].Style.TextRotation = 90;                                    // จัดทิศทางอักษร 90 องศา
                                        ws.Cells[rowIndex, col].Value = dataDate[k].ToString("d/MM/yyyy", Shared.CultureInfoTh);
                                        k++;
                                    }
                                    else if (col > countDate + 2)
                                    {
                                        ws.Cells[rowIndex, col].AutoFitColumns(4.00);
                                        ws.Cells[rowIndex, col].Value = statusText[j];
                                        j++;
                                    }
                                    break;
                                }
                        }
                    }
                }

                // Detail Table
                using (var db = new Student_AttendanceEntities())
                {
                    var dataStudent = (from r in db.Tb_Student
                                       where r.StudyGroupID == model.StudyGroupID && r.StatusID == 5 // 5 = สถานะลงทะเบียนเรียน
                                       orderby r.StdCode
                                       select new
                                       {
                                           StdID = r.StdID,
                                           StdCode = r.StdCode,
                                           NameTH = r.NameTH
                                       }).ToList(); // ชุดแรก
                    countStudent = dataStudent.Count();

                    var dataStatus = (from r in db.Tb_StudentCheck
                                      where r.StudyGroupID == model.StudyGroupID
                                      group r by new { r.DateCheck } into g
                                      select new
                                      {
                                          g.Key,
                                          g,
                                      }).ToList();
                    int rows = 10;
                    foreach (var x in dataStudent)
                    {
                        int cols = 3;
                        ws.Cells[rows, 1].Value = x.StdCode;
                        ws.Cells[rows, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[rows, 1].AutoFitColumns(16);
                        ws.Cells[rows, 2].Value = x.NameTH;
                        ws.Cells[rows, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[rows, 2].AutoFitColumns(28);
                        foreach (var r in dataStatus)
                        {
                            var status = (from t in db.Tb_StudentCheck
                                          where (x.StdID == t.StdID && model.StudyGroupID == t.StudyGroupID && t.DateCheck == r.Key.DateCheck)
                                          select t.StatusID).FirstOrDefault();
                            if (status == 7 || status == 8)
                            {
                                if (rows == 10)
                                {
                                    ws.Cells[10, cols, 10 + countStudent - 1, cols].Merge = true; //Merge columns start and end range
                                    ws.Cells[10, cols, 10 + countStudent - 1, cols].Style.VerticalAlignment = ExcelVerticalAlignment.Center;    // จัดตำแหน่งอักษรแนวตั้ง
                                    ws.Cells[10, cols, 10 + countStudent - 1, cols].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[10, cols, 10 + countStudent - 1, cols].AutoFitColumns(4);                                          // จัดความกว้างคอลัมน์ parameter หน่วย = picxel
                                    ws.Cells[10, cols, 10 + countStudent - 1, cols].Style.TextRotation = 90;                                    // จัดทิศทางอักษร 90 องศา
                                    ws.Cells[10, cols, 10 + countStudent - 1, cols].Value = (status == 7) ? r.g.Select(n => n.Note).FirstOrDefault() : r.g.Select(n => n.Note).FirstOrDefault();
                                }
                                cols += 1;
                                continue;
                            }
                            else
                            {
                                ws.Cells[rows, cols].Value = (status == 1) ? "/" : (status == 2) ? "ส" : (status == 3) ? "ล" : (status == 4) ? "X" : "";
                                ws.Cells[rows, cols].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[rows, cols].AutoFitColumns(4);
                            }
                            cols += 1;
                        }
                        rows += 1;
                    }

                    rows = 10;
                    for (rows = 10; rows < countStudent + 10; rows++)
                    {
                        for (int col = dataStatus.Count + 3; col <= dataStatus.Count + 7; col++)
                        {
                            string header = ws.Cells[rowIndex, col].Value.ToString();
                            switch (header)
                            {
                                case "มา":
                                    ws.Cells[rows, col].Formula = "COUNTIF(" + ws.Cells[rows, 3].Address + ":" + ws.Cells[rows, 2 + countDate].Address + ",\"/\")";  // สูตรคำนวณ COUNTIF
                                    break;
                                case "สาย":
                                    ws.Cells[rows, col].Formula = "COUNTIF(" + ws.Cells[rows, 3].Address + ":" + ws.Cells[rows, 2 + countDate].Address + ",\"ส\")";  // สูตรคำนวณ COUNTIF
                                    break;
                                case "ลา":
                                    ws.Cells[rows, col].Formula = "COUNTIF(" + ws.Cells[rows, 3].Address + ":" + ws.Cells[rows, 2 + countDate].Address + ",\"ล\")";  // สูตรคำนวณ COUNTIF
                                    break;
                                case "ขาด":
                                    ws.Cells[rows, col].Formula = "COUNTIF(" + ws.Cells[rows, 3].Address + ":" + ws.Cells[rows, 2 + countDate].Address + ",\"X\")";  // สูตรคำนวณ COUNTIF
                                    break;
                                case "รวม":
                                    ws.Cells[rows, col].Formula = "SUM(" + ws.Cells[rows, 2 + countDate + 1].Address + ":" + ws.Cells[rows, countDate + 6].Address + ")";  // สูตรคำนวณ SUM
                                    break;
                            }
                        }
                    }
                }
                // Border Table ใส่เส้นขอบช่องตาราง
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, 7 + countDate].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, 7 + countDate].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, 7 + countDate].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, 7 + countDate].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                var memoryStream = package.GetAsByteArray();
                // mimetype from http://stackoverflow.com/questions/4212861/what-is-a-correct-mime-type-for-docx-pptx-etc
                return base.File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName); // save file .xlsx
            }
        } // ส่งออกรายงานสรุปผลการเช็คชื่อ ตามรายวิชา-กลุ่มเรียน ในรูปแบบไฟล์ .xlsx (Excel)

        public ActionResult ExportSummaryNoExam(StudyGroupModel model)
        {
            string[] strGroup = model.StudyGroupCode.Split('.');
            string fileName = "SummaryListNoExam_" + model.SubjectCode + "-" + strGroup[0] + strGroup[1] + ".xlsx"; // ชื่อไฟล์ = SummaryListNoExam_รหัสวิชา-รหัสกลุ่มเรียน
            var dataHeader = new object();
            int countDate = 0;
            using (var db = new Student_AttendanceEntities())
            {
                dataHeader = (from t1 in db.Tb_SchoolYear
                              join t2 in db.Tb_StudyGroup on t1.SchYearID equals t2.SchYearID
                              join t3 in db.Tb_Subject on new { t2.SubjectCode, t2.Course } equals new { t3.SubjectCode, t3.Course }
                              join t4 in db.Tb_Student on t2.StudyGroupID equals t4.StudyGroupID
                              join t5 in db.Tb_Department on t2.DeptCode equals t5.DeptCode
                              where t1.SchYearID == model.SchYearID && t2.StudyGroupID == model.StudyGroupID && t3.SubjectCode == model.SubjectCode && t3.Course == model.Course
                              orderby t4.StdCode
                              select new SummaryModel
                              {
                                  Term = t1.Term,
                                  Year = t1.Year,
                                  DeptName = t5.DeptName,
                                  SubjectCode = t3.SubjectCode,
                                  SubjectName = t3.SubjectName,
                                  Course = t3.Course,
                                  StudyGroupCode = t2.StudyGroupCode
                              }).ToList();
            }

            using (var package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("Sheet1");
                ExcelWorksheet ws = package.Workbook.Worksheets[1];
                // ws.Name = "Test"; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 14; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "TH SarabunPSK"; //Default Font name for whole sheet

                //ws.Cells[1, 1].Value = "Sample DataTable Export"; // Heading Name
                //ws.Cells[1, 1, 1, 10].Merge = true; //Merge columns start and end range
                //ws.Cells[1, 1, 1, 10].Style.Font.Bold = true; //Font should be bold
                //ws.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center
                var imgPath = Server.MapPath("~/icon/Logo_RMUTI_128X235.png");
                Bitmap image = new Bitmap(imgPath);
                ExcelPicture pic = ws.Drawings.AddPicture("Logo_RMUTI", image);
                pic.SetPosition(5, 5);
                pic.SetSize(48, (int)(235 * 0.347));

                //Header Page 
                ws.Cells["A1:E1"].Value = "                มหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A1:E1"].Merge = true;                                                 // Merge = ผสานเซลล์และจัดกึ่งกลาง
                ws.Cells["A1:E1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;    // จัดตำแหน่งอักษรแนวนอน 

                ws.Cells["A2:E2"].Value = "                ศูนย์กลางมหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A2:E2"].Merge = true;
                ws.Cells["A2:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                ws.Cells["A4:F4"].Value = "สรุปรายชื่อนักศึกษาที่ไม่มีสิทธิ์สอบ";
                ws.Cells["A4:F4"].Merge = true;
                ws.Cells["A4:F4"].Style.Font.Bold = true;
                ws.Cells["A4:F4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                foreach (var item in (List<SummaryModel>)dataHeader)
                {
                    ws.Cells["A6:B6"].Value = "ประจำภาคการศึกษา " + item.Term + "/" + item.Year;
                    ws.Cells["A6:B6"].Merge = true;
                    ws.Cells["A6:B6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["C6:F6"].Value = item.DeptName; // สาขาวิชา
                    ws.Cells["C6:F6"].Merge = true;
                    ws.Cells["C6:F6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["A7:C7"].Value = "รายวิชา " + item.SubjectCode + " | " + item.SubjectName;
                    ws.Cells["A7:C7"].Merge = true;
                    ws.Cells["A7:C7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["D7:E7"].Value = "กลุ่มเรียน " + item.StudyGroupCode;
                    ws.Cells["D7:E7"].Merge = true;
                    ws.Cells["D7:E7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    break;
                }
                //End Header Page 

                // Haeder Table
                ws.Cells.AutoFitColumns(); // จัดความกว้างของคอลัมน์ 
                int rowIndex = 9;  // หัวตาราง เริ่มบรรทัดที่ 9
                int endRow = 0; // บรรทัดสุดท้ายของข้อมูล
                using (var db = new Student_AttendanceEntities())
                {
                    var dataDate = (from r in db.Tb_StudentCheck
                                    where r.StudyGroupID == model.StudyGroupID
                                    orderby r.DateCheck ascending
                                    select r.DateCheck).Distinct().ToArray();
                    countDate = dataDate.Length;

                    for (int col = 1; col <= 3 + countDate + 5; col++)
                    {
                        ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[rowIndex, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //ws.Cells[rowIndex, col].Style.Font.Bold = true;
                        //ws.Cells[rowIndex, col].AutoFitColumns();
                        switch (col)
                        {
                            case 1:
                                ws.Cells[rowIndex, col].Value = "รหัสนักศึกษา";
                                break;
                            case 2:
                                ws.Cells[rowIndex, col].Value = "ชื่อ-สกุล";
                                break;
                            case 3:
                                ws.Cells[rowIndex, col].Value = "ขาด";
                                break;
                        }
                    }
                }

                // Detail Table
                using (var db = new Student_AttendanceEntities())
                {
                    var dataStudent = (from r in db.Tb_StudentCheck
                                       join s in db.Tb_Student on r.StdID equals s.StdID
                                       where r.StudyGroupID == model.StudyGroupID
                                       orderby s.StdCode, r.StdID
                                       group r by new { r.StdID, s.StdCode } into g
                                       select new
                                       {
                                           g.Key,
                                           Status = g.Where(x => x.StatusID == 4).Select(x => x.StatusID),
                                           StdCode = g.Select(x => x.Tb_Student.StdCode).FirstOrDefault(),
                                           NameTH = g.Select(x => x.Tb_Student.NameTH).FirstOrDefault(),
                                           //CountDate = g.Where(x => x.StatusID != 7 && x.StatusID != 8 && x.StatusID != 9).Select(x => x.DateCheck).Distinct()
                                       }).OrderBy(r => r.StdCode).ToList();
                    var CountDate = db.Tb_StudentCheck.Where(r => r.StatusID != 7 && r.StatusID != 8 && r.StatusID != 9).Select(r => r.DateCheck).Distinct().Count();
                    int row = 10;  // รายละเอียดในตารางเริ่มบรรทัดที่ 10
                    int col = 1;
                    foreach (var item in dataStudent)
                    {
                        int n = CountDate; // n = วันที่หมดที่มีการเช็คชื่อ
                        int result = 100 * (n - item.Status.Count()) / n; // คำนวน % การเข้าเรียน = 100 * (วันทั้งหมดที่เช็คชื่อ(n) - จำนวนวันที่ขาดเรียน / วันทั้งหมดที่เช็คชื่อ(n))
                        if (result < 80) // คำนวน % การเข้าเรียน ต้องไม่น้อยกว่า 80% ถ้าน้อยกว่า = หมดสิทธิ์สอบ
                        {
                            ws.Cells[row, col].Value = item.StdCode;
                            ws.Cells[row, ++col].Value = item.NameTH;
                            ws.Cells[row, ++col].Value = item.Status.Count();
                            row++;
                            col -= 2; // col-=2;
                            endRow++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    for (row = 10; row <= 10 + endRow; row++)
                    {
                        for (col = 1; col <= 3; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    ws.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    ws.Cells[row, col].AutoFitColumns(16);
                                    break;
                                case 2:
                                    ws.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    ws.Cells[row, col].AutoFitColumns(28);
                                    break;
                                case 3:
                                    ws.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[row, col].AutoFitColumns(8);
                                    break;
                            }
                        }
                    }
                }

                // Border Table ใส่เส้นขอบช่องตาราง
                ws.SelectedRange[rowIndex, 1, rowIndex + endRow, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + endRow, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + endRow, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + endRow, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //var chart = ws.Drawings.AddChart("chart1", eChartType.AreaStacked);
                ////Set position and size
                //chart.SetPosition(0, 630);
                //chart.SetSize(800, 600);

                // Add the data series. 
                //var series = chart.Series.Add(ws.Cells["A2:A46"], ws.Cells["B2:B46"]);

                var memoryStream = package.GetAsByteArray();
                // mimetype from http://stackoverflow.com/questions/4212861/what-is-a-correct-mime-type-for-docx-pptx-etc
                return base.File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName); // save file .xlsx
            }
        } // ส่งออกรายงานสรุปรายชื่อนักศึกษาที่ไม่มีสิทธิ์สอบ ตามรายวิชา-กลุ่มเรียน ในรูปแบบไฟล์ .xlsx (Excel)

        public ActionResult ExportSummaryTask(StudyGroupModel model)
        {
            string[] strGroup = model.StudyGroupCode.Split('.');
            string fileName = "SummaryTask_" + model.SubjectCode + "-" + strGroup[0] + strGroup[1] + ".xlsx";
            var dataHeader = new object();
            int countSTask = 0;
            int countStudent = 0;
            int countGTask = 0;

            using (var db = new Student_AttendanceEntities())
            {
                dataHeader = (from t1 in db.Tb_SchoolYear
                              join t2 in db.Tb_StudyGroup on t1.SchYearID equals t2.SchYearID
                              join t3 in db.Tb_Subject on new { t2.SubjectCode, t2.Course } equals new { t3.SubjectCode, t3.Course }
                              join t4 in db.Tb_Student on t2.StudyGroupID equals t4.StudyGroupID
                              join t5 in db.Tb_Department on t2.DeptCode equals t5.DeptCode
                              where t1.SchYearID == model.SchYearID && t2.StudyGroupID == model.StudyGroupID && t3.SubjectCode == model.SubjectCode && t3.Course == model.Course && t2.UserID == UserLogon.UserID
                              orderby t4.StdCode
                              select new SummaryModel
                              {
                                  Term = t1.Term,
                                  Year = t1.Year,
                                  DeptName = t5.DeptName,
                                  SubjectCode = t3.SubjectCode,
                                  SubjectName = t3.SubjectName,
                                  Course = t3.Course,
                                  StudyGroupCode = t2.StudyGroupCode
                              }).ToList();
            }

            using (var package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("Sheet1");
                ExcelWorksheet ws = package.Workbook.Worksheets[1];
                // ws.Name = "Test"; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 14; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "TH SarabunPSK"; //Default Font name for whole sheet

                //ws.Cells[1, 1].Value = "Sample DataTable Export"; // Heading Name
                //ws.Cells[1, 1, 1, 10].Merge = true; //Merge columns start and end range
                //ws.Cells[1, 1, 1, 10].Style.Font.Bold = true; //Font should be bold
                //ws.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center
                var imgPath = Server.MapPath("~/icon/Logo_RMUTI_128X235.png");
                Bitmap image = new Bitmap(imgPath);
                ExcelPicture pic = ws.Drawings.AddPicture("Logo_RMUTI", image);
                pic.SetPosition(5, 5);
                pic.SetSize(48, (int)(235 * 0.347));


                //Header Page 
                ws.Cells["A1:I1"].Value = "                มหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A1:I1"].Merge = true;                                                 // Merge = ผสานเซลล์และจัดกึ่งกลาง
                ws.Cells["A1:I1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;    // จัดตำแหน่งอักษรแนวนอน 

                ws.Cells["A2:I2"].Value = "                ศูนย์กลางมหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A2:I2"].Merge = true;
                ws.Cells["A2:I2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                ws.Cells["A4:K4"].Value = "รายชื่อนักศึกษาตามรายวิชา - กลุ่มเรียน";
                ws.Cells["A4:K4"].Merge = true;
                ws.Cells["A4:K4"].Style.Font.Bold = true;
                ws.Cells["A4:K4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                foreach (var item in (List<SummaryModel>)dataHeader)
                {
                    ws.Cells["A6:B6"].Value = "ประจำภาคการศึกษา " + item.Term + "/" + item.Year;
                    ws.Cells["A6:B6"].Merge = true;
                    ws.Cells["A6:B6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["D6:L6"].Value = item.DeptName; // สาขาวิชา
                    ws.Cells["D6:L6"].Merge = true;
                    ws.Cells["D6:L6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["A7:E7"].Value = "รายวิชา " + item.SubjectCode + " | " + item.SubjectName;
                    ws.Cells["A7:E7"].Merge = true;
                    ws.Cells["A7:E7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["G7:I7"].Value = "กลุ่มเรียน " + item.StudyGroupCode;
                    ws.Cells["G7:I7"].Merge = true;
                    ws.Cells["G7:I7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    break;
                }
                //End Header Page 

                // Haeder Table

                int rowIndex = 9;  // หัวตาราง เริ่มบรรทัดที่ 9
                string[] scoreText = { "คะแนนรวม" };
                int j = 0;
                int k = 0;
                using (var db = new Student_AttendanceEntities())
                {
                    var dataSTask = (from r in db.Tb_Task
                                     where r.StudyGroupID == model.StudyGroupID && r.Type == 1
                                     orderby r.TaskID ascending
                                     select r.TaskName).ToArray();
                    var dataGTask = (from r in db.Tb_Task
                                     where r.StudyGroupID == model.StudyGroupID && r.Type == 2
                                     orderby r.TaskID ascending
                                     select r.TaskName).ToArray();
                    countSTask = dataSTask.Length;
                    countGTask = dataGTask.Length;

                    for (int col = 1; col <= 2 + countSTask + 2; col++)
                    {
                        ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[rowIndex, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //ws.Cells[rowIndex, col].Style.Font.Bold = false;
                        //ws.Cells[rowIndex, col].AutoFitColumns();
                        switch (col)
                        {
                            case 1:
                                ws.Cells[rowIndex, col].Value = "รหัสนักศึกษา";
                                break;
                            case 2:
                                ws.Cells[rowIndex, col].Value = "ชื่อ-สกุล";
                                break;
                            default:
                                {
                                    if (col > 2 && col <= countSTask + 2)  // แสดงวันที่มีการเช็คชื่อในแต่ละคอลัมน์ 
                                    {
                                        ws.Cells[rowIndex, col].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;    // จัดตำแหน่งอักษรแนวตั้ง
                                        ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[rowIndex, col].AutoFitColumns(4);                                          // จัดความกว้างคอลัมน์ parameter หน่วย = picxel
                                        ws.Cells[rowIndex, col].Style.TextRotation = 90;                                    // จัดทิศทางอักษร 90 องศา
                                        ws.Cells[rowIndex, col].Value = dataSTask[k] + " [S]";
                                        k++;
                                    }
                                    break;
                                }
                        }
                    }
                    k = 0;
                    rowIndex = 9;
                    for (int col = 1; col <= 2 + countSTask + countGTask + 1; col++)
                    {
                        ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[rowIndex, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //ws.Cells[rowIndex, col].Style.Font.Bold = false;
                        //ws.Cells[rowIndex, col].AutoFitColumns();
                        switch (col)
                        {
                            case 1:
                                ws.Cells[rowIndex, col].Value = "รหัสนักศึกษา";
                                break;
                            case 2:
                                ws.Cells[rowIndex, col].Value = "ชื่อ-สกุล";
                                break;
                            default:
                                {
                                    if (col > 2 + countSTask && col <= countSTask + countGTask + 2)  // แสดงวันที่มีการเช็คชื่อในแต่ละคอลัมน์ 
                                    {
                                        ws.Cells[rowIndex, col].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;    // จัดตำแหน่งอักษรแนวตั้ง
                                        ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[rowIndex, col].AutoFitColumns(4);                                          // จัดความกว้างคอลัมน์ parameter หน่วย = picxel
                                        ws.Cells[rowIndex, col].Style.TextRotation = 90;                                    // จัดทิศทางอักษร 90 องศา
                                        ws.Cells[rowIndex, col].Value = dataGTask[k] + " [G]";
                                        k++;
                                    }
                                    else if (col > countSTask + countGTask + 1)
                                    {
                                        ws.Cells[rowIndex, col].AutoFitColumns(9);
                                        ws.Cells[rowIndex, col].Value = scoreText[j];
                                        ws.Cells[rowIndex, col].Style.Font.Bold = true;
                                        j++;
                                    }
                                    break;
                                }
                        }
                    }
                }

                //// Detail Table
                using (var db = new Student_AttendanceEntities())
                {
                    var dataStudent = (from r in db.Tb_Student
                                       where r.StudyGroupID == model.StudyGroupID && r.StatusID == 5 // 5 = สถานะลงทะเบียนเรียน
                                       orderby r.StdCode
                                       select new
                                       {
                                           StdID = r.StdID,
                                           StdCode = r.StdCode,
                                           NameTH = r.NameTH
                                       }).ToList(); // ชุดแรก
                    countStudent = dataStudent.Count();

                    var dataTaskS = (from r in db.Tb_SingleTask
                                     join e in db.Tb_Task on r.TaskID equals e.TaskID
                                     join s in db.Tb_Student on r.StdID equals s.StdID
                                     where e.StudyGroupID == model.StudyGroupID && e.Type == 1 && s.StatusID == 5
                                     group e by new { e.TaskID } into g
                                     select new
                                     {
                                         g.Key,
                                     }).ToList();
                    var dataTaskG = (from c in db.Tb_Group
                                     join e in db.Tb_Task on c.TaskID equals e.TaskID
                                     where e.StudyGroupID == model.StudyGroupID && e.Type == 2
                                     group e by new { e.TaskID } into g
                                     select new
                                     {
                                         g.Key,
                                     }).ToList();
                    int rows = 10;
                    foreach (var x in dataStudent)
                    {
                        int cols = 3;
                        ws.Cells[rows, 1].Value = x.StdCode;
                        ws.Cells[rows, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[rows, 1].AutoFitColumns(16);
                        ws.Cells[rows, 2].Value = x.NameTH;
                        ws.Cells[rows, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[rows, 2].AutoFitColumns(28);
                        foreach (var r in dataTaskS)
                        {

                            var score = db.Tb_SingleTask.Where(e => e.TaskID == r.Key.TaskID && e.StdID == x.StdID).Select(e => e.Score).FirstOrDefault();
                            ws.Cells[rows, cols].Value = score;
                            ws.Cells[rows, cols].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            ws.Cells[rows, cols].AutoFitColumns(4);
                            cols += 1;
                        }
                        foreach (var r in dataTaskG)
                        {
                            var score = (from c in db.Tb_Group
                                         join g in db.Tb_GroupTask on c.GroupID equals g.GroupID
                                         join e in db.Tb_Task on c.TaskID equals e.TaskID
                                         where r.Key.TaskID == c.TaskID && x.StdID == g.StdID
                                         select g.Score).FirstOrDefault();
                            ws.Cells[rows, cols].Value = score;
                            ws.Cells[rows, cols].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            ws.Cells[rows, cols].AutoFitColumns(4);
                            cols += 1;
                        }
                        rows += 1;
                    }

                    rows = 10;
                    for (rows = 10; rows < countStudent + 10; rows++)
                    {
                        int col = dataTaskS.Count + dataTaskG.Count + 3;
                        string header = ws.Cells[rowIndex, col].Value.ToString();
                        switch (header)
                        {
                            case "คะแนนรวม":
                                ws.Cells[rows, col].Formula = "SUM(" + ws.Cells[rows, 3].Address + ":" + ws.Cells[rows, countSTask+countGTask + 2].Address + ")";  // สูตรคำนวณ SUM
                                break;
                        }
                    }
                }
                // Border Table ใส่เส้นขอบช่องตาราง
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, countGTask + countSTask + 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, countGTask + countSTask + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, countGTask + countSTask + 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[rowIndex, 1, rowIndex + countStudent, countGTask + countSTask + 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[rowIndex + countStudent + 2, 1].Value = "[S] คือ งานเดี่ยว";
                ws.Cells[rowIndex + countStudent + 3, 1].Value = "[G] คือ งานกลุ่ม";
                var memoryStream = package.GetAsByteArray();
                // mimetype from http://stackoverflow.com/questions/4212861/what-is-a-correct-mime-type-for-docx-pptx-etc
                return base.File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName); // save file .xlsx
            }
        } // ส่งออกรายงานสรุปคะแนนการมอบมายงาน ตามรายวิชา-กลุ่มเรียน ในรูปแบบไฟล์ .xlsx (Excel)

        public ActionResult HolidayCalendar()
        {
            return View();
        } // ปฏิทินวันหยุดประจำปี จาก Google API

        public ActionResult Exceptions(string message)
        {
            ViewBag.message = message;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // CRUD LINQ EXAMPLE
        public JsonResult RegisterSave(RegisterModel model) // บันทึกข้อมูลการสมัครใช้งานระบบ Register
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (var db = new Student_AttendanceEntities())
                {
                    db.Tb_User.Add(new Tb_User()
                    {
                        Username = model.Username,
                        Password = model.Password,
                        Name = model.Name,
                        NameEN = model.NameEN,
                        DeptCode = model.DeptCode,
                        Role = "user",
                    });
                    db.SaveChanges();
                    jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเเล้ว" };
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
    }
}