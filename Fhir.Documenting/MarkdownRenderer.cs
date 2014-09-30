using MarkdownDeep;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class MarkdownRenderer : IRenderer    
    {
        private void Render(StreamReader reader, StreamWriter writer)
        {
            var mark = new Markdown();

            //set preferences of your markdown
            mark.SafeMode = true;
            mark.ExtraMode = true;

            string mdtext = reader.ReadToEnd();
            string htmlmd = mark.Transform(mdtext);

            writer.Write(htmlmd);            
        }

        public void Render(Stream input, Stream output)
        {
            var reader = new StreamReader(input);
            var writer = new StreamWriter(output);
            Render(reader, writer);
            writer.Flush();
        }
    }
}
