using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class RazorRenderer : IRenderer
    {
        public void Render(Document input, Document output)
        {
            output.Extension = ".html";
            output.Text = Razor.Render(input.Context, input.Text);
        }
    }
}
