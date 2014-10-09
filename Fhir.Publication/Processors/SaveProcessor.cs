using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class SaveProcessor : IProcessor
    {
        public string Extension { get; private set; }
        public SaveProcessor(string extension)
        {
            this.Extension = extension;
        }
        public void Process(Document input, Stage output)
        {
            if (this.Extension != null) input.Extension = this.Extension;
            input.Save();
        }
    }
}
