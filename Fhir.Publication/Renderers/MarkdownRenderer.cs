using MarkdownDeep;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{

    public class MarkdownRenderer : TextRenderer    
    {
        public override void Render(Context context, StreamReader reader, StreamWriter writer)
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
