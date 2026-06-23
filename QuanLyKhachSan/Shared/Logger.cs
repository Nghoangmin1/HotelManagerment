using System;
using System.IO;

namespace HotelManagement.Shared
{
    public class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hotel_management.log");
        private static readonly object LockObj = new object();

        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            string fullMessage = message;
            if (ex != null)
            {
                fullMessage += $" | Exception: {ex.Message} | StackTrace: {ex.StackTrace}";
            }
            WriteLog("ERROR", fullMessage);
        }

        private static void WriteLog(string level, string message)
        {
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            
            // Write to Console
            Console.WriteLine(formattedMessage);

            // Write to File (Thread-safe)
            lock (LockObj)
            {
                try
                {
                    File.AppendAllText(LogFilePath, formattedMessage + Environment.NewLine);
                }
                catch
                {
                    // Fail silently to avoid interrupting the application
                }
            }
        }
    }
}
