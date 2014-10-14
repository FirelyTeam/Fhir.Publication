using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class TemplateRenderer : IRenderer
    {
        Document template;

        string stashkey, name;

        private Document getTemplate()
        {
            if (template != null)
            {
                return template;
            }
            else
            {
                return Stash.Get(stashkey, name);
            }
        }
        public TemplateRenderer(Document template)
        {
            this.template = template;
        }

        public TemplateRenderer(string stash, string name)
        {
            this.stashkey = stash;
            this.name = name;
        }

        public void Render(Document input, Document output)
        {
            Document template = getTemplate();
            output.Text = template.Text.Replace("%body%", input.Text);
        }
    }
    
}
