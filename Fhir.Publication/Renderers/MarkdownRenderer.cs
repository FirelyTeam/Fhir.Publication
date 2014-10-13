using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownDeep;

namespace Hl7.Fhir.Publication
{
    public class MarkDownRenderer : IRenderer
    {
        public void Render(Document input, Document output)
        {
            output.Extension = ".html";
            var mark = new Markdown();

            //set preferences of your markdown
            mark.SafeMode = true;
            mark.ExtraMode = true;

            string mdtext = input.Text;
            output.Text = mark.Transform(mdtext);
        }
    }


}
