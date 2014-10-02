using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class RazorRenderer : IRenderer
    {
        public void Render(Source source, Stream input, Stream output)
        {
            Razor.Render(source, input, output);
        }
    }
}
