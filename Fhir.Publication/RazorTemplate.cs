using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public abstract class RazorTemplate<T>
    {
        public StringBuilder Buffer { get; set; }
        public StringWriter Writer { get; set; }

        public RazorTemplate()
        {
            Buffer = new StringBuilder();
            Writer = new StringWriter(Buffer);
        }

        public T Model { get; set; }

        public abstract void Execute();

        public virtual void Write(object value)
        {
            WriteLiteral(value);
        }

        public void Include(string filename)
        {
            WriteLiteral(File.ReadAllText(filename));
        }

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
