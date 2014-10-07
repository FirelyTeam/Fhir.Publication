using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class RazorProcessor : IProcessor
    {

        public void Process(Document input, Stage output)
        {
            Document target = output.CreateFrom(input);

            target.Text = Razor.Render(input.Context, input.Text);
        }
    }
}
