using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public enum LogLevel { Silent = 1, Error = 2, Info = 3, Debug = 4 }
    public static class Log
    {
        public static LogLevel Level { get; set; }

        static Log()
        {
            Level = LogLevel.Info;
        }

        public static string Capitalize(string value)
        {
            value = value.ToLower();
            value = char.ToUpper(value[0]) + value.Substring(1);
            return value;
        }

        public static LogLevel ParseLevel(string s, LogLevel defaultlevel)
        {
            if (s == null)
            {
                return defaultlevel;
            }

            s = Capitalize(s);
            try
            {
                return (LogLevel)Enum.Parse(typeof(LogLevel), s);
            }
            catch
            {
                throw new Exception("Unknown log level: " + s);
            }
            
        }

        public static void Write(LogLevel level, string message, params object[] args)
        {
            message = string.Format(message, args);
            if (level <= Level)
                Console.WriteLine(message);
        }

        public static void Info(string text, params object[] args)
        {
            Write(LogLevel.Info, text, args);
        }

        public static void Error(string text, params object[] args)
        {
            Write(LogLevel.Error, "ERROR: " + text, args);
        }
        
        public static void Debug(string text, params object[] args)
        {
            Write(LogLevel.Debug, "DEBUG: " + text, args);
        }
    }
}
