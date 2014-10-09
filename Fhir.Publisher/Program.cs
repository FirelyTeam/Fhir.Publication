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
            Console.WriteLine("Fhir publisher tool 0.9.1 BETA");

            string dir = null;
            string sourcedir = null;

            if (args.Count() != 1) 
            {
                Console.WriteLine("You must provide a make file.");
                return;
            }
            
            string input = args[0];
            string filter = Path.GetFileName(input);
            sourcedir = Path.GetDirectoryName(input);
            if (!Path.IsPathRooted(sourcedir))
            {
                sourcedir = Directory.GetCurrentDirectory();
            }
            
            dir = Directory.GetParent(sourcedir).FullName;
            
            string targetdir = dir + "\\Generated";

            
            try
            {
                Publisher.Generate(sourcedir, targetdir, filter);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0}:\n{1}", e.GetType(), e.Message);
            }
        }
    }
}
