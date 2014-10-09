using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Deprecated
{
    public class Templater : IWork
    {
        public Context Context {get; set;}

        public void Execute()
        {
            string s = File.ReadAllText(Context.FullPath);
            string template = File.ReadAllText(Context.CurrentDir + "\\template.html");
            string output = template.Replace("%body%", s);
            File.WriteAllText(Context.TargetFullPath, output);
            
        }
    }
}
