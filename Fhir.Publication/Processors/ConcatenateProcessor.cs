using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class ConcatenateProcessor : IProcessor
    {

        public void Process(Document input, Stage output)
        {
            StringBuilder builder = new StringBuilder(input.Text);
            foreach(Document doc in input.Attachments)
            {
                builder.Append(doc.Text);
            }
            Document result = output.CloneAndPost(input);
            result.Text = builder.ToString();
        }
    }
    
}

