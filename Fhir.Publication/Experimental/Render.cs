using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class RenderProcessor : IProcessor
    {
        public IRenderer Renderer { get; set; }
        public RenderProcessor(IRenderer renderer)
        {
            this.Renderer = renderer;
        }

        public void Process(Document input, Stage stage)
        {
            Document output = stage.CreateFrom(input);
            Renderer.Render(input, output);
        }
    }

}
