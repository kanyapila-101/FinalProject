using System;
using System.IO;
using System.Threading;

public class Log
{
    public static readonly object lockerError = new object();
    public static readonly object lockerInfo = new object();

    private static void Init()
    {
        string logPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }
    }
    private static void Init(string folderName)
    {
        string logPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\" + folderName + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }
    }

    public static void Warning(string message)
    {
        new Thread(() => WriteWarning(message)).Start();
    }

    public static void Error(string message)
    {
        new Thread(() => WriteError(message)).Start();
    }

    public static void Info(string message)
    {
        new Thread(() => WriteInfo(message)).Start();
    }

    public static void More(string message, string fileName)
    {
        new Thread(() => WriteMore(message, fileName)).Start();
    }

    public static void More(string message, string folderName, string fileName)
    {
        new Thread(() => WriteMore(message, folderName, fileName)).Start();
    }

    private static void WriteWarning(string message)
    {
        try
        {
            lock (lockerError)
            {
                Init();
                string fileName = string.Empty;
                fileName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\Logs\\" +
                            DateTime.Now.ToString("yyyy-MM-dd") + "\\" + $"Warning-{DateTime.Now:yyyyMMdd}" + ".txt";
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    sw.Write("{0:dd/MM/yyyy-HH:mm:ss} | ", DateTime.Now);
                    sw.WriteLine(message);
                    sw.Close();
                    sw.Dispose();
                }
            }
        }
        catch
        {

        }
    }

    private static void WriteError(string message)
    {
        try
        {
            lock (lockerError)
            {
                Init();
                string fileName = string.Empty;
                fileName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\Logs\\" +
                           DateTime.Now.ToString("yyyy-MM-dd") + "\\" + $"Error-{DateTime.Now:yyyyMMdd}" + ".txt";
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    sw.Write("{0:dd/MM/yyyy-HH:mm:ss} | ", DateTime.Now);
                    sw.WriteLine(message);
                    sw.Close();
                    sw.Dispose();
                }

                //try
                //{
                //    string email = System.Configuration.ConfigurationManager.AppSettings["ErrorReportEmail"];
                //    if (!string.IsNullOrEmpty(email))
                //    {
                //        using (var db = new DataEntities())
                //        {
                //            var conf = db.SettingEmail.FirstOrDefault();
                //            if (conf != null)
                //            {
                //                conf.EmailTo = email;
                //                Shared.SendEmail(conf, "เกิดข้อผิดพลาด","["+ DateTime.Now.ToString("dd/MM/yyyy HH:mm") +"] " + message);
                //            }
                //        }
                //    }
                //}catch{}
            }
        }
        catch
        {

        }
    }

    private static void WriteInfo(string message)
    {
        try
        {
            lock (lockerInfo)
            {
                Init();
                string fileName = string.Empty;
                fileName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\" + $"Info-{DateTime.Now:yyyyMMdd}" + ".txt";
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    sw.Write("{0:dd/MM/yyyy-HH:mm:ss} | ", DateTime.Now);
                    sw.WriteLine(message);
                    sw.Close();
                    sw.Dispose();
                }
            }
        }
        catch
        {

        }
    }

    private static void WriteMore(string message, string fileName)
    {
        try
        {
            lock (lockerInfo)
            {
                Init();
                fileName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\" + fileName + ".txt";
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    sw.Write("{0:dd/MM/yyyy-HH:mm:ss} | ", DateTime.Now);
                    sw.WriteLine(message);
                    sw.Close();
                    sw.Dispose();
                }
            }
        }
        catch
        {

        }
    }

    private static void WriteMore(string message, string folderName, string fileName)
    {
        try
        {
            lock (lockerInfo)
            {
                Init(folderName);
                fileName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\" + folderName + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\" + fileName + ".txt";
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    sw.Write("{0:dd/MM/yyyy-HH:mm:ss} | ", DateTime.Now);
                    sw.WriteLine(message);
                    sw.Close();
                    sw.Dispose();
                }
            }
        }
        catch
        {

        }
    }
}