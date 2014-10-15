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

    public class DictTableProcessor : IProcessor
    {
        public void Process(Document input, Stage output)
        {
            var generator = new DictHtmlGenerator(new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/"));
            var profile = (Profile)FhirParser.ParseResourceFromXml(input.Text);
            Document result = input.CloneMetadata();
            var xmldoc = generator.Generate(profile, "http://di.nl");
            result.Text = xmldoc.ToString(SaveOptions.DisableFormatting);
            output.Post(result);
        }
    }
}
