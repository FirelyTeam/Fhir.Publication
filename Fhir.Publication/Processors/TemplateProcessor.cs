using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class TemplateProcessor : IProcessor
    {
        public ISelector Influx { get; set; }

        public void Process(Document input, Stage output)
        {
            Document template = Influx.Single(input);
            Document result = input.CloneMetadata();
            
            result.Text = template.Text.Replace("%body%", input.Text);
            result.Extension = template.Extension;

            output.Post(result);
           
        }
    }
    
}
