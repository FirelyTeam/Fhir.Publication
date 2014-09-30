using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{

    public interface IRenderer
    {
        void Render(Stream input, Stream output);
    }

}
