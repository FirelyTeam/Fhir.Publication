using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Hl7.Fhir.Profiling;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Model;
using Hl7.Fhir.Publication;
using System.IO;
using Hl7.Fhir.Serialization;

namespace Fhir.Profiling.Tests
{
    [TestClass]
    public class PublishProfile
    {     
        [TestMethod]
        public void PublishLipidProfile()
        {
            var profile = (Profile)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\lipid.profile.xml"));
            var pkp = new ProfileKnowledgeProvider("lipid");

            {
                var publisher = new ProfileTableGenerator(@"c:\temp\dist\images", false, pkp);

                var result = File.ReadAllText(@"TestData\publish-header.cshtml");
                result += publisher.Generate(profile, false).ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                result += File.ReadAllText(@"TestData\publish-footer.cshtml");

                File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForProfileTable(profile), result);
            }

            {
                var publisher = new StructureGenerator(@"c:\temp\dist\images", false, pkp);

                foreach (var structure in profile.Structure)
                {
                    var result = File.ReadAllText(@"TestData\publish-header.cshtml");
                    result += publisher.generateStructureTable(structure, false, profile)
                            .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                    result += File.ReadAllText(@"TestData\publish-footer.cshtml");

                    File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForLocalStructure(profile, structure), result);
                }
            }

            {
                var dictgen = new DictHtmlGenerator(pkp);
                var result = File.ReadAllText(@"TestData\publish-header.cshtml");
                result += dictgen.Generate(profile)
                            .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                result += File.ReadAllText(@"TestData\publish-footer.cshtml");

                File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForProfileDict(profile), result);
            }

            {
                var vs = (ValueSet)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\hv-laboratory-result-interpretation-v1.xml"));

                var vsGen = new ValueSetGenerator(@"c:\temp\publisher\", pkp);

                var result = File.ReadAllText(@"TestData\publish-header.cshtml");
                result += vsGen.generate(vs)
                            .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                result += File.ReadAllText(@"TestData\publish-footer.cshtml");

                File.WriteAllText(@"c:\temp\publisher\" + "hv-laboratory-result-interpretation-v1.html", result);
            }
        }
    }
}
