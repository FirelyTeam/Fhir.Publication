using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{

    public interface IStreamRenderer
    {
        void Render(SourceFile item, Stream input, Stream output);
    }

    public interface IBlock<INPUT, OUTPUT>
    {
        OUTPUT Process(INPUT input);
    }
    

}
