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
        public ISelector Influx { get; set; }

        public void Process(Document input, Stage output)
        {
            var pkp = new ProfileKnowledgeProvider(input.Name, input.Context.Target.Directory);
            var generator = new StructureGenerator(pkp);

            var profile = (Profile)FhirParser.ParseResourceFromXml(input.Text);

            foreach (var structure in profile.Structure)
            {
                var result = generator
                        .generateStructureTable(structure, false, profile)
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);

                Document document = output.CloneAndPost(input);
                document.SetFilename(pkp.GetLinkForLocalStructure(profile, structure));
                document.Text = result;
            }
        }      
    }
}
