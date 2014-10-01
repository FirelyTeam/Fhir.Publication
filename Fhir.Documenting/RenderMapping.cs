using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{

    public class RenderMapping
    {
        public string FromExtension;
        public string ToExtension;
        public IStreamRenderer Renderer;

        public void Render(SourceFile item, string outputfile)
        {
            using (Stream input = File.OpenRead(item.FullPath))
            using (Stream output = File.OpenWrite(outputfile))
            {
                Renderer.Render(item, input, output);
            }
        }

        public RenderMapping(string fromExtension, IStreamRenderer renderer, string toExtension)
        {
            this.FromExtension = fromExtension;
            this.ToExtension = toExtension;
            this.Renderer = renderer;
        }
        public bool IsMappingFor(string extension)
        {
            return FromExtension == extension;
        }
    }
}
