using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class AttachProcessor : IProcessor
    {
        string key, mask;

        public AttachProcessor(string key, string mask)
        {
            this.key = key;
            this.mask = mask;
        }

        

        public void Process(Document input, Stage output)
        {
            string name = Disk.ParseMask(input.Name, mask);
            Document attachment = Stash.Get(key, name);
            if (attachment != null) input.Attach(attachment);
            output.Add(input);
        }

    }
}
