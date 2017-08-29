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
using OfficeOpenXml.Style;

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
                            where (string.IsNullOrEmpty(model.Query) || t.SubjectCode.Contains(model.Query) || t.SubjectName.Contains(model.Query) || t.SubjectNameEN.Contains(model.Query))
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
                            join s in db.Tb_Subject on c.SubjectCode equals s.SubjectCode
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
                                }
                                else
                                {
                                    var data = db.Tb_Student.Where(r => r.StdID == model.StdID).FirstOrDefault();
                                    if (data != null)
                                    {
                                        db.Tb_Student.Where(r => r.StdID == model.StdID).ForEach(r =>
                                        {
                                            r.StdCode = model.StdCode;
                                            r.NameEN = model.NameEN;
                                            r.NameTH = model.NameTH;
                                            r.StatusID = model.StatusID;
                                            r.StudyGroupID = model.StudyGroupID;
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
                                    //var notstd = new List<string>();
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
                                                //notstd.Add(id);
                                                continue;
                                            }
                                            else
                                            {
                                                db.Tb_Student.Add(new Tb_Student
                                                {
                                                    StdCode = id,
                                                    NameTH = nameTH,
                                                    NameEN = nameEN,
                                                    StatusID = 5, // 5 = สถานะลงทะเบียน
                                                    StudyGroupID = model.StudyGroupID
                                                });
                                                stdNew++;
                                                isFileCorrect = true;
                                            }
                                        }
                                    }
                                    var Scode = db.Tb_Student.Where(r => r.StudyGroupID == model.StudyGroupID).Select(r => r.StdCode).ToList();
                                    stdTotal = Scode.Count();
                                    //if (stdDuplicate < stdTotal)
                                    //{
                                    //    var stdcode = Scode.Where(r => !notstd.Contains(r)).ToList();
                                    //}
                                    if (isFileCorrect == true)
                                    {
                                        db.SaveChanges();
                                        tran.Commit();
                                        
                                        jsonReturn = new JsonResponse { status = true, message = "บันทึกข้อมูลเรียบร้อยเล้ว", data = new { stdDuplicate, stdNew, stdTotal } };
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
                                       join s1 in db.Tb_StudyGroup on s.SubjectCode equals s1.SubjectCode
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
                                        join s1 in db.Tb_StudyGroup on s.SubjectCode equals s1.SubjectCode
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
            var strTime = new string[] { "08:00", "09:00", "10.00", "11.00", "12.00", "13.00", "14.00", "15.00", "16.00", "17.00", "18.00", "19.00", "20.00", "21.00" };
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
                        for (int i = 1; i <= 17; i++)
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
                                   join s1 in db.Tb_StudyGroup on s.SubjectCode equals s1.SubjectCode
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

                ViewBag.Haveschedule = db.Tb_Schedule.Count();
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
                                     where r.StudyGroupID == id && (r.StatusID == null || r.StatusID == 9) && (r.Note == null || r.Note == "")
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
                DateTime dateCompensate = DateTime.ParseExact(dateCom, "dd/MM/yyyy", Shared.CultureInfoTh);
                DateTime dateOrigin = DateTime.ParseExact(dateorigin, "dd/MM/yyyy", Shared.CultureInfoTh);
                int groupID = int.Parse(str[0]);
                string today = DateTime.Now.ToString("MM/dd/yyyy", Shared.CultureInfoTh);
                DateTime Today = DateTime.Parse(today);
                using (var db = new Student_AttendanceEntities())
                {
                    var dataCompens = (from r in db.Tb_StudentCheck
                                       where r.StudyGroupID == groupID && r.DateCheck == dateCompensate && r.StatusID == 9
                                       select r).ToList();

                    var DateOriginal = (from r in db.Tb_StudentCheck
                                        where r.StudyGroupID == groupID && r.DateCheck == dateOrigin
                                        select r).ToList();
                    if (dataCompens.Count > 0 && dataCompens != null)
                    {
                        foreach (var r in dataCompens)
                        {
                            db.Tb_StudentCheck.Remove(r);
                        }

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
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
            }
            return Json(jsonReturn);
        } // delete วันสอนชดเชย ลบออกจากตารางการเช็คชื่อ ตามกลุ่มเรียนนั้น และให้วันที่เรียนปกติ มีสถานะ รอการเช็คชื่อ(9) , Note = ""

        public ActionResult StdCheck(FilterModel model, int id)
        {
            using (var db = new Student_AttendanceEntities())
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
                var today = DateTime.Now.ToString("MM/dd/yyyy");
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
                if (datastatus.Count > 0 && datastatus != null) // ถ้าวันนี้มีการเช็คชื่อแล้วไปแล้วอย่างน้อย 1 ครั้ง
                {
                    ViewBag.StdCheck = dataIscheck;             // ส่งข้อมูล Query dataIscheck ไปแก้ไขการเช็คชื่อ
                }
                else
                {
                    ViewBag.StdCheck = dataNoCheck;             // มิฉะนั้นแล้ว ส่งข้อมูล Query dataNoCheck ไปการเช็คชื่อครั้งใหม่
                }
                var dataHeader = (from sg in db.Tb_StudyGroup        // Query เอาข้อมูลกลุ่มเรียน ไปแสดง ใน Header
                                  join sj in db.Tb_Subject on sg.SubjectCode equals sj.SubjectCode
                                  join sy in db.Tb_SchoolYear on sg.SchYearID equals sy.SchYearID
                                  where sg.StudyGroupID == id
                                  select new StudyGroupModel
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
                ViewBag.StudyGroup = dataHeader;
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
                                x.StdCheckID = r.StdCheckID;
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

                    var today = DateTime.Now.ToString("MM/dd/yyyy");
                    DateTime Today = DateTime.Parse(today);
                    var dataholiday = db.Tb_Holiday.Where(r => r.HolidayDate == Today).Select(r => new { r.HolidayDate, r.HolidayName }).FirstOrDefault();

                    if (Today >= data.r.StartMidterm && Today <= data.r.EndMidterm)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "อยู่ในช่วง สอบกลางภาค" };
                    }
                    else if (Today >= data.r.StartFinal && Today <= data.r.EndFinal)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "อยู่ในช่วง สอบปลายภาค" };
                    }
                    else if (dataholiday != null /*|| dataholiday.HolidayDate == Today*/)
                    {
                        jsonReturn = new JsonResponse { status = false, message = "เป็นวัน " + dataholiday.HolidayName };
                    }
                    else
                    {
                        jsonReturn = new JsonResponse { status = true };
                    }
                }
            }
            catch (Exception ex)
            {
                jsonReturn = new JsonResponse { status = false, message = ex.Message };
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
                                             join s in db.Tb_Subject on r.SubjectCode equals s.SubjectCode
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
                                             join s in db.Tb_Subject on r.SubjectCode equals s.SubjectCode
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
                                            join s in db.Tb_Subject on r.SubjectCode equals s.SubjectCode
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
                                            join s in db.Tb_Subject on r.SubjectCode equals s.SubjectCode
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

        public ActionResult DetailSummaryListCheck(SummaryModel model)
        {
            using (var db = new Student_AttendanceEntities())
            {
                var dataHeader = (from t1 in db.Tb_SchoolYear
                                  join t2 in db.Tb_StudyGroup on t1.SchYearID equals t2.SchYearID
                                  join t3 in db.Tb_Subject on t2.SubjectCode equals t3.SubjectCode
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
                              join t3 in db.Tb_Subject on t2.SubjectCode equals t3.SubjectCode
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
                ws.Cells.Style.Font.Size = 16; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "TH SarabunPSK"; //Default Font name for whole sheet

                //ws.Cells[1, 1].Value = "Sample DataTable Export"; // Heading Name
                //ws.Cells[1, 1, 1, 10].Merge = true; //Merge columns start and end range
                //ws.Cells[1, 1, 1, 10].Style.Font.Bold = true; //Font should be bold
                //ws.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center

                //Header Page 
                ws.Cells["A1:I1"].Value = "มหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A1:I1"].Merge = true;                                                 // Merge = ผสานเซลล์และจัดกึ่งกลาง
                ws.Cells["A1:I1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;    // จัดตำแหน่งอักษรแนวนอน 

                ws.Cells["A2:I2"].Value = "ศูนย์กลางมหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
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
                        ws.Cells[rowIndex, col].Style.Font.Bold = true;
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
                        ws.Cells[rows, 1].AutoFitColumns(17);
                        ws.Cells[rows, 2].Value = x.NameTH;
                        ws.Cells[rows, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[rows, 2].AutoFitColumns(30);
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
                              join t3 in db.Tb_Subject on t2.SubjectCode equals t3.SubjectCode
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
                ws.Cells.Style.Font.Size = 16; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "TH SarabunPSK"; //Default Font name for whole sheet

                //ws.Cells[1, 1].Value = "Sample DataTable Export"; // Heading Name
                //ws.Cells[1, 1, 1, 10].Merge = true; //Merge columns start and end range
                //ws.Cells[1, 1, 1, 10].Style.Font.Bold = true; //Font should be bold
                //ws.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center

                //Header Page 
                ws.Cells["A1:E1"].Value = "มหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
                ws.Cells["A1:E1"].Merge = true;                                                 // Merge = ผสานเซลล์และจัดกึ่งกลาง
                ws.Cells["A1:E1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;    // จัดตำแหน่งอักษรแนวนอน 

                ws.Cells["A2:E2"].Value = "ศูนย์กลางมหาวิทยาลัยเทคโนโลยีราชมงคลอีสาน";
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
                        ws.Cells[rowIndex, col].Style.Font.Bold = true;
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
                                    ws.Cells[row, col].AutoFitColumns(17);
                                    break;
                                case 2:
                                    ws.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    ws.Cells[row, col].AutoFitColumns(30);
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

        public ActionResult HolidayCalendar()
        {
            return View();
        } // ปฏิทินวันหยุดประจำปี จาก Google API

        public ActionResult test()
        {
            // Add Student file 
            //DataSet ds = new DataSet();
            //var list = new List<StudentModel>();
            //bool isID = true;
            //if (Request.Files["file"].ContentLength > 0)
            //{
            //    string fileExtension = Path.GetExtension(Request.Files["file"].FileName);

            //    if (fileExtension == ".xls" || fileExtension == ".xlsx")
            //    {
            //        string fileLocation = Server.MapPath("~/FileUpload/") + Request.Files["file"].FileName;
            //        if (System.IO.File.Exists(fileLocation))
            //        {
            //            System.IO.File.Delete(fileLocation);
            //        }
            //        Request.Files["file"].SaveAs(fileLocation);
            //        string excelConnectionString = string.Empty;
            //        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
            //        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
            //        //connection String for xls file format.
            //        if (fileExtension == ".xls")
            //        {
            //            excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
            //            fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
            //        }
            //        //connection String for xlsx file format.
            //        else if (fileExtension == ".xlsx")
            //        {
            //            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
            //            fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
            //        }
            //        //Create Connection to Excel work book and add oledb namespace
            //        OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
            //        excelConnection.Open();
            //        DataTable dt = new DataTable();

            //        dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            //        if (dt == null)
            //        {
            //            return null;
            //        }

            //        String[] excelSheets = new String[dt.Rows.Count];
            //        int t = 0;
            //        //excel data saves in temp file here.
            //        foreach (DataRow row in dt.Rows)
            //        {
            //            excelSheets[t] = row["TABLE_NAME"].ToString();
            //            t++;
            //        }
            //        OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


            //        string query = string.Format("Select * from [{0}]", excelSheets[0]);
            //        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
            //        {
            //            dataAdapter.Fill(ds);
            //        }

            //        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            //        {
            //            string id = ds.Tables[0].Rows[i][0].ToString();
            //            string nameTH = ds.Tables[0].Rows[i][1].ToString();
            //            string nameEN = ds.Tables[0].Rows[i][2].ToString();
            //            if (id.Length < 13 || id.Length > 14) 
            //            {
            //                isID = false;
            //                break;
            //            }
            //            else
            //            {
            //                isID = true;
            //                db.Tb_Student.Add(new Tb_Student
            //                {
            //                    StdCode = id,
            //                    NameTH = nameTH,
            //                    NameEN = nameEN,
            //                    StudyGroupID = data.StudyGroupID
            //                });
            //            }
            //        }
            //        excelConnection.Close();
            //        if (isID == true)
            //        {
            //            //db.SaveChanges();
            //            tran.Commit();
            //            jsonReturn = new JsonResponse { status = true, messege = "บันทึกข้อมูลเรียบร้อยเล้ว" };
            //        }
            //        else
            //        {
            //            tran.Rollback();
            //            jsonReturn = new JsonResponse { status = false, messege = "fail", data = "File Incorrect" };
            //        }
            //    }
            //    else
            //    {
            //        jsonReturn = new JsonResponse { status = false, messege = "fail" };
            //    }
            //}
            //else
            //{
            //    jsonReturn = new JsonResponse { status = false, messege = "fail" };
            //}
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