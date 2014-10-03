using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{

    public class Filter : IWork
    {
        string mask;
        Context context;
        string toExt;
        bool recurse;
        IRenderer renderer;

        public Filter(Context context, string mask, string toExt, bool recurse, IRenderer renderer)
        {
            this.mask = mask;
            this.context = context;
            this.toExt = toExt;
            this.recurse = recurse;
            this.renderer = renderer;
        }

        public IEnumerable<IWork> Select()
        {
            SearchOption option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] filenames = Directory.GetFiles(context.CurrentDir, mask, option);
            foreach(string filename in filenames)
            {
                Context file = context.Clone(filename, toExt);
                yield return new FileRendering(file, renderer);
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

