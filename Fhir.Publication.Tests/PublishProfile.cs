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
            var pkp = new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/");
            var profile =  (Profile)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\lipid.profile.xml"));

            var pagename = profile.Name;

            var publisher = new ProfileTableGenerator(@"c:\temp\publisher", false, pkp);

            var result = File.ReadAllText(@"TestData\publish-header.cshtml");
            result += publisher.Generate(profile, false).ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            result += File.ReadAllText(@"TestData\publish-footer.cshtml");

            File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForProfileTable(profile),result);
        }

        [TestMethod]
        public void PublishLipidProfileStructures()
        {
            var source = new FileArtifactSource(true);
            var profile = (Profile)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\lipid.profile.xml"));

            var pkp = new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/");
            var publisher = new StructureGenerator(@"c:\temp\publisher\",false,pkp);

            foreach (var structure in profile.Structure)
            {
                var result = File.ReadAllText(@"TestData\publish-header.cshtml");
                result += publisher.generateStructureTable(structure, false, profile, "http://nu.nl/publisher.html", "lipid")
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                result += File.ReadAllText(@"TestData\publish-footer.cshtml");

                File.WriteAllText(@"c:\temp\publisher\" + pkp.getLinkForStructure(profile,structure), result);
            }
        }


        [TestMethod]
        public void PublishLipidDictHtml()
        {
            var source = new FileArtifactSource(true);
            var profile = (Profile)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\lipid.profile.xml"));

            var pkp = new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/");
            var publisher = new DictHtmlGenerator(pkp);

            var result = File.ReadAllText(@"TestData\publish-header.cshtml");
            result += publisher.Generate(profile)
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            result += File.ReadAllText(@"TestData\publish-footer.cshtml");

            File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForProfileDict(profile), result);
        }

        [TestMethod]
        public void PublishValueSet()
        {
            var source = new FileArtifactSource(true);
            var vs = (ValueSet)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\hv-laboratory-result-interpretation-v1.xml"));

            var pkp = new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/");
            var publisher = new ValueSetGenerator(@"c:\temp\publisher\", pkp);

            var result = File.ReadAllText(@"TestData\publish-header.cshtml");
            result += publisher.generate(vs)
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            result += File.ReadAllText(@"TestData\publish-footer.cshtml");

            File.WriteAllText(@"c:\temp\publisher\" + "hv-laboratory-result-interpretation-v1.html", result);
        }
    }
}
