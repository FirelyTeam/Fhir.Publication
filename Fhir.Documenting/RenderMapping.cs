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
        public IRenderer Renderer;

        public void Render(Stream input, Stream output)
        {
            using(var reader = new StreamReader(input))
            using (var writer = new StreamWriter(output))
            {
                Renderer.Render(reader, writer);
            }
        }

        public void Render(string inputfile, string outputfile)
        {
            using (Stream input = File.OpenRead(inputfile))
            using (Stream output = File.OpenWrite(outputfile))
            {
                Render(input, output);
            }
        }

        public RenderMapping(string fromExtension, IRenderer renderer, string toExtension)
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
