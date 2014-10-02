using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public abstract class TextRenderer : IRenderer
    {
        public abstract void Render(Source item, StreamReader reader, StreamWriter writer);

        public void Render(Source item, Stream input, Stream output)
        {
            var reader = new StreamReader(input);
            var writer = new StreamWriter(output);
            Render(item, reader, writer);
            writer.Flush();
        }
    }
}
