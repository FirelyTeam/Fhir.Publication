using MarkdownDeep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class MarkdownRenderer : IRenderer    
    {
        public void Render(System.IO.StreamReader reader, System.IO.StreamWriter writer)
        {
            var mark = new Markdown();
            
            //set preferences of your markdown
            mark.SafeMode = true;
            mark.ExtraMode = true;

            string mdtext = reader.ReadToEnd();
            string htmlmd = mark.Transform(mdtext);

            writer.Write(htmlmd);            
        }
    }
}
