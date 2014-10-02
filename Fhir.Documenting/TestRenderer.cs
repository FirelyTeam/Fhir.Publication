using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class TestRenderer : TextRenderer
    {
        public override void Render(Source item, StreamReader reader, StreamWriter writer)
        {
            string content = reader.ReadToEnd();
            StringBuilder builder = new StringBuilder();
            builder.Append(item.FileName);
            builder.Append("==");
            builder.Append(content);

            writer.Write(content);
        }
    }
}
