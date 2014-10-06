using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hl7.Fhir.Publication
{
    public class StructureWork : IWork
    {
        public Context Context { get; set; }

        public void Execute()
        {
            var pkp = new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/");
            var generator = new StructureGenerator(Context.TargetDir, false, pkp);
            string s = File.ReadAllText(Context.FullPath);
            var profile = (Profile)FhirParser.ParseResourceFromXml(s);

            foreach (var structure in profile.Structure)
            {
                var result = generator.generateStructureTable(structure, false, profile, "http://nu.nl/publisher.html", Context.Name)
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);

                File.WriteAllText(Context.TargetDir + "\\" + pkp.getLinkForStructure(Context.Name, structure.Name), result);
            }
        }
    }

}
