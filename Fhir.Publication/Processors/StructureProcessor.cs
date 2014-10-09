using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class StructureProcessor : IProcessor
    {
        public void Process(Document input, Stage output)
        {
            var pkp = new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/");
            var generator = new StructureGenerator(input.Context.Target.Directory, false, pkp);

            var profile = (Profile)FhirParser.ParseResourceFromXml(input.Text);

            foreach (var structure in profile.Structure)
            {
                var result = generator
                        .generateStructureTable(structure, false, profile, "http://nu.nl/publisher.html", input.Name)
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);

                Document document = output.CloneAndPost(input);
                document.Name = input.Name + "-" + structure.Name;
                document.Text = result;

            }
        }      
    }
}
