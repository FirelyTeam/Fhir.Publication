using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public static class Log
    {
        public static void Info(string text, params object[] args)
        {
            Console.WriteLine("INFO: " + text, args);
        }

        public static void Error(string text, params object[] args)
        {
            Console.WriteLine("ERROR: " + text, args);
        }
        
        public static void Debug(string text, params object[] args)
        {
            Console.WriteLine("DEBUG: " + text, args);
        }
    }
}
