using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public interface ISelector
    {
        string Mask { get; set; }
        IEnumerable<Document> Documents { get; }
    }

    
}
