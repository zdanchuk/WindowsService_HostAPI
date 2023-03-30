//using HidSharp.Reports;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace WindowsService_HostAPI
{
        public static class LogWriter
    {
        public const string INFO = "info";
        public const string WARNING = "warn";
        public const string ERROR = "error";

        private static string m_exePath = string.Empty;

        public static void LogWrite(string logMessage)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); //"d:\\My Documents\\Visual Studio 2022\\repos\\WindowsService_HostAPI\\";//
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}: - {2}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString(), logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception)
            {
            }
        }


        public static void LogWrite(string className, string logMessage)
        {
            LogWrite(INFO, className, logMessage);
        }


        public static void LogWrite(string level, string className, string logMessage)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    Log(level, className, logMessage, w);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Log(string level, string className, string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.WriteLine("{0}: {1} - {2} {3}: - {4}", className, level.ToString(), DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString(), logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception)
            {
            }
        }
    }
}
