using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Publication;


namespace Hl7.Fhir.DocumenterTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = (args.Count() == 1) ? args[0] : Directory.GetCurrentDirectory();

            string sourcedir = dir + "\\Source";
            string targetdir = dir + "\\Generated";
            Console.WriteLine("Fhir publisher tool 0.9 BETA");
            try
            {
                Publisher.Generate(sourcedir, targetdir);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0}:\n{1}", e.GetType(), e.Message);
            }
        }
    }
}
