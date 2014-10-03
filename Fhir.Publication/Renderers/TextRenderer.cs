using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public abstract class TextRenderer : IRenderer
    {
        public abstract void Render(Context context, StreamReader reader, StreamWriter writer);

        public void Render(Context context, Stream input, Stream output)
        {
            var reader = new StreamReader(input);
            var writer = new StreamWriter(output);
            Render(context, reader, writer);
            writer.Flush();
        }
    }
}
