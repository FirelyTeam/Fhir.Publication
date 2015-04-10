using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class MakeProcessor : IProcessor
    {
        public ISelector Influx { get; set; }

        public void Process(Document input, Stage output)
        {
            IWork work = Make.InterpretDocument(input.Text, input.Context);
            work.Execute();
        }

    }

    public class MakeForAllProcessor : IProcessor
    {
        private string pattern;
        public ISelector Influx { get; set; }

        public MakeForAllProcessor(string pattern)
        {
            this.pattern = pattern;
        }

        public void Process(Document input, Stage output)
        {

            IEnumerable<string> folders = Disk.Directories(input.Context.Source.Directory, pattern);
            foreach(string folder in folders)
            {
                Context context = Context.CreateFromSource(input.Context.Root, folder);

                IWork work = Make.InterpretDocument(input.Text, context);
                work.Execute();
            }
        }
    }

    
    
}
