using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{

    public class MakeFilter : IWorkFilter
    {
        string mask;
        Context context;
        bool recurse;
        
        public MakeFilter(Context context, string mask, bool recurse = false)
        {
            this.mask = mask;
            this.context = context;
            this.recurse = recurse;
        }

        public IEnumerable<IWork> Select()
        {
            SearchOption option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(context.CurrentDir, mask, option);
            foreach (string file in files)
            {
                Source source = new Source(context, file);
                yield return Make.Interpret(source);
            }
        }

        public void Execute()
        {
            foreach (IWork work in Select())
            {
                work.Execute();
            }
        }
        
    }
}
