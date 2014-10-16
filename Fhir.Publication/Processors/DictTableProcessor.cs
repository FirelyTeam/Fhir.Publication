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
            var pkp = new ProfileKnowledgeProvider(input.Name);
            var generator = new DictHtmlGenerator(pkp);
            var profile = (Profile)FhirParser.ParseResourceFromXml(input.Text);
            Document result = input.CloneMetadata();
            var xmldoc = generator.Generate(profile);
            result.SetFilename(pkp.GetLinkForProfileDict(profile));
            result.Text = xmldoc.ToString(SaveOptions.DisableFormatting);
            output.Post(result);
        }
    }
}
