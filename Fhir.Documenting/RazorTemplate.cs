using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public abstract class RazorTemplate
    {
        public StringBuilder Buffer { get; set; }
        public StringWriter Writer { get; set; }

        public RazorTemplate()
        {
            Buffer = new StringBuilder();
            Writer = new StringWriter(Buffer);
        }

        public abstract void Execute();

        // Writes the results of expressions like: "@foo.Bar"
        public virtual void Write(object value)
        {
            // Don't need to do anything special
            // Razor for ASP.Net does HTML encoding here.
            WriteLiteral(value);
        }

        // Writes literals like markup: "<p>Foo</p>"
        public virtual void WriteLiteral(object value)
        {
            Buffer.Append(value);
        }

        public string Render()
        {
            Execute();
            string s = Buffer.ToString();
            Buffer.Clear();
            return s;
        }
    }
}
