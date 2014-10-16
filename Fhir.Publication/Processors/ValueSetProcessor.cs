using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hl7.Fhir.Publication
{

    public class ValueSetProcessor : IProcessor
    {
        public ISelector Influx { get; set; }
        
        public void Process(Document input, Stage output)
        {
            var generator = new ValueSetGenerator(new ProfileKnowledgeProvider(input.Name, input.Context.Target.Directory));
            var valueset = (ValueSet)FhirParser.ParseResourceFromXml(input.Text);
            Document result = input.CloneMetadata();
            var xmldoc = generator.generate(valueset);
            result.Text = xmldoc.ToString(SaveOptions.DisableFormatting);
            output.Post(result);
        }
    }
}
