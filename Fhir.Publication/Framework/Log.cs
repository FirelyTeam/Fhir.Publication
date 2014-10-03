using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public static class Log
    {
        public static void WriteLine(string text, params object[] args)
        {
            Console.WriteLine(text, args);
        }
    }
}
