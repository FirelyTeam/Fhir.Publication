using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hl7.Fhir.Publication.Experimental
{

    public class ProfileProcessor : IProcessor
    {

        public void Process(Document input, Stage output)
        {
            var generator = new ProfileTableGenerator(input.Context.Target.Directory, input.Name, false, new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/"));
            var profile = (Profile)FhirParser.ParseResourceFromXml(input.Text);
            Document result = output.CreateDocumentBasedOn(input);
            var xmldoc = generator.generate(profile, extensionsOnly: false);
            result.Text = xmldoc.ToString(SaveOptions.DisableFormatting);
        }
    }
}
