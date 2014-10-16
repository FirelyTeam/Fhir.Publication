using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public interface IProcessor
    {
        ISelector Influx { get; set; }
        void Process(Document input, Stage output);
    }

    
}


