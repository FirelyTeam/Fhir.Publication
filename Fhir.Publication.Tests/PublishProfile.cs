using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Hl7.Fhir.Profiling;
using Hl7.Fhir.Specification.Model;
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

            var pagename = "lipid";

            var publisher = new ProfileTableGenerator(@"c:\temp\publisher", "lipid", false, pkp);

            var result = File.ReadAllText(@"TestData\publish-header.xml");
            result += publisher.generate(profile, false).ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            result += File.ReadAllText(@"TestData\publish-footer.xml");

            File.WriteAllText(@"c:\temp\publisher\" + pagename + ".html",result);
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
                var result = File.ReadAllText(@"TestData\publish-header.xml");
                result += publisher.generateStructureTable(structure, false, profile, "http://nu.nl/publisher.html", "lipid")
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                result += File.ReadAllText(@"TestData\publish-footer.xml");

                File.WriteAllText(@"c:\temp\publisher\" + pkp.getLinkForStructure("lipid",structure.Name), result);
            }
        }


        [TestMethod]
        public void PublishLipidDicHtml()
        {
            var source = new FileArtifactSource(true);
            var profile = (Profile)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\lipid.profile.xml"));

            var pkp = new ProfileKnowledgeProvider("http://www.hl7.org/implement/standards/fhir/");
            var publisher = new DictHtmlGenerator(@"c:\temp\publisher\", pkp);

            var result = File.ReadAllText(@"TestData\publish-header.xml");
            result += publisher.generate(profile, "http://nu.nl/publisher.html")
                        .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            result += File.ReadAllText(@"TestData\publish-footer.xml");

            File.WriteAllText(@"c:\temp\publisher\" + "dict.html", result);
        }
    }
}
