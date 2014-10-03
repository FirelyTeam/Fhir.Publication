using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public static class Publisher
    {
        public static void Generate(string sourcedir, string targetdir)
        {
            Context context = Context.Root(sourcedir, targetdir);
            Bulk generator = new Bulk();

            IEnumerable<IWork> work = Make.Filter(context, "make.mk");
            generator.Add(work);
            generator.Execute();
        }
    }
    
}
