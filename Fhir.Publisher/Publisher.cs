using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public static class Publisher
    {

        public static void Generate(string sourcedir, string targetdir, string mask, string loglevel)
        {
            Log.Level = Log.ParseLevel(loglevel, LogLevel.Info);
            
            Log.Info("Fhir publisher tool 0.9.3 BETA");

            Root root = new Root(sourcedir, targetdir);
            Context context = root.Context();

            Document document = Filter.GetDocument(context, mask);
            IWork work = Make.InterpretDocument(document);
            
            work.Execute();
            
            Log.Info("Rendering complete. Output to directory {0}", targetdir);
            
        }
    }
    
}
