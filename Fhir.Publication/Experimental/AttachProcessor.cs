using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class AttachProcessor : IProcessor
    {
        string key, mask;

        public AttachProcessor(string key, string mask)
        {
            this.key = key;
            this.mask = mask;
        }

        public static string ParseMask(string name, string mask)
        {
            string[] parts = name.Split(new char[] { '-', '.' });
            string result = mask;
            for (int i = 0; i <= parts.Count()-1; i++)
            {
                string alias = string.Format("${0}", i+1);
                result = result.Replace(alias, parts[i]);
            }
            return result;
        }

        public void Process(Document input, Stage output)
        {
            string name = ParseMask(input.Name, mask);
            Document attachment = Stash.Get(key, name);
            if (attachment != null) input.Attach(attachment);
            output.Add(input);
        }

    }
}
