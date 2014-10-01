using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public abstract class TextRenderer : IStreamRenderer
    {
        public abstract void Render(SourceFile item, StreamReader reader, StreamWriter writer);

        public void Render(SourceFile item, Stream input, Stream output)
        {
            var reader = new StreamReader(input);
            var writer = new StreamWriter(output);
            Render(item, reader, writer);
            writer.Flush();
        }
    }
}
