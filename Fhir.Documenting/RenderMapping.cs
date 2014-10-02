using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{

    public class RenderFilter : IWorkFilter
    {
        string mask;
        Context context;
        string toExt;
        IRenderer renderer;

        public RenderFilter(Context context, string mask, string toExt, IRenderer renderer)
        {
            this.mask = mask;
            this.context = context;
            this.toExt = toExt;
            this.renderer = renderer;
        }

        public static string TargetFile(Source source, string toExtension)
        {
            string location = source.Context.TargetDir;
            string corename = Path.GetFileNameWithoutExtension(source.FileName);
            string target = location + "\\" + corename + toExtension;
            return target;
        }

        public IEnumerable<IWork> Select()
        {
            string[] files = Directory.GetFiles(context.CurrentDir, mask, SearchOption.AllDirectories);
            foreach(string file in files)
            {
                Source source = new Source(context, file);
                string targetfile = TargetFile(source, toExt);
                yield return new FileRendering(source, targetfile, renderer);
            }
        }

        public void Execute()
        {
            foreach(IWork work in Select())
            {
                work.Execute();
            }
        }
    }

        
}

