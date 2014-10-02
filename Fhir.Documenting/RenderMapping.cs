using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{

    public class RenderMapping : IWorkFilter
    {
        string mask;
        Context context;
        string toExt;
        IRenderer renderer;

        public RenderMapping(Context context, string fromExt, string toExt, IRenderer renderer)
        {
            this.mask = "*" + fromExt;
            this.context = context;
            this.toExt = toExt;
            this.renderer = renderer;
        }

        public static string TargetFile(Source item, string toExtension)
        {
            string location = item.context.TargetDir;
            string corename = Path.GetFileNameWithoutExtension(item.FileName);
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
                yield return new FileRenderWork(source, targetfile, renderer);
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

