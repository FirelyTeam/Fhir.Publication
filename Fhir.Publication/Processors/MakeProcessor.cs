using System;
using System.Collections.Generic;
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
            IWork work = Make.InterpretDocument(input);
            work.Execute();
        }
    }
}
