using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hl7.Fhir.Publication.Renderers;

namespace Hl7.Fhir.Publication.Experimental
{
    public static class Publisher
    {

        public static void Generate(string sourcedir, string targetdir, string mask)
        {
            Root root = new Root(sourcedir, targetdir);
            Context context = root.Context();

            Document document = Filter.GetDocument(context, mask);
            IWork work = MakeFile.InterpretDocument(document);
            
            work.Execute();
            
            Log.Info("Rendering complete. Output to directory {0}", targetdir);
            
        }
    }
    
}
