using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public static class Publisher
    {
        public static void Generate(string sourcedir, string targetdir, string mask)
        {
            Context context = Context.Root(sourcedir, targetdir);
            Plan generator = new Plan();

            IFilter filter = Make.Filter(context, mask);
            generator.Add(filter);
            generator.Execute();
        }
    }
    
}
