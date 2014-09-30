using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class TestRenderer : IRenderer
    {

        public void Render(Stream input, Stream output)
        {
            var reader = new StreamReader(input);
            var writer = new StreamWriter(output);
            
            string content = reader.ReadToEnd();
            content = "TestRendering - Hello World!!!\n" + content;
            writer.Write(content);
            writer.Flush();
        }
    }
}
