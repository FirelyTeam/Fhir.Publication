using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class TemplateRenderer : IRenderer
    {
        Document template;
        public TemplateRenderer(Document template)
        {
            this.template = template;
        }

        public void Render(Document input, Document output)
        {
            output.Text = template.Text.Replace("%body%", input.Text);
        }
    }
    
}
