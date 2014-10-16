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

        string stashkey, mask;

        private Document getTemplate(string inputname)
        {
            if (template != null)
            {
                return template;
            }
            else
            {
                if (mask != null)
                {
                    string name = Disk.ParseMask(inputname, mask);
                    return Stash.Get(stashkey, name);
                }
                else
                {
                    return Stash.Get(stashkey).Documents.First();
                }
            }
        }
        public TemplateRenderer(Document template)
        {
            this.template = template;
        }

        public TemplateRenderer(string stash, string mask = null)
        {
            this.stashkey = stash;
            this.mask = mask;
        }

        public void Render(Document input, Document output)
        {
            Document template = getTemplate(input.Name);
            output.Text = template.Text.Replace("%body%", input.Text);
            output.Extension = template.Extension;
        }
    }
    
}
