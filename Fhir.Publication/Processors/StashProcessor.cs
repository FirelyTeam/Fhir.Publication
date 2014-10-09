using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class StashProcessor : IProcessor
    {
        string key;

        public StashProcessor(string key)
        {
            this.key = key;
        }

        public void Process(Document input, Stage output)
        {
            Stash.Push(key, input.Clone());
        }
    }

}
