using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class MakeProcessor : IProcessor
    {

        public void Process(Document input, Stage output)
        {
            IWork work = MakeFile.InterpretDocument(input);
            work.Execute();
        }
    }
}
