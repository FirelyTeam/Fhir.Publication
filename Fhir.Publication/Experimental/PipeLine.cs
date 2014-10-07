using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class PipeLine 
    {
        public List<IProcessor> Processors = new List<IProcessor>();
        public PipeLine Add(IProcessor processor)
        {
            this.Processors.Add(processor);
            return this;
        }

        public override string ToString()
        {
            return string.Join(" >> ", Processors.Select(p => p.GetType().Name));
        }
    }
}
