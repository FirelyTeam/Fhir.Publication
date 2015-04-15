using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Model;
using Hl7.Fhir.Publication;
using System.IO;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Publication.Profile;
using Hl7.Fhir.Rest;

namespace Fhir.Profiling.Tests
{
    [TestClass]
    public class PublishProfile
    {
        [TestInitialize]
        public void Setup()
        {

        }
 
        [TestMethod]
        public void PublishLipidProfile()
        {
            //var profile = (Profile)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\lipid.profile.xml"));
            var source = ArtifactResolver.CreateCachedDefault();
            var pkp = new ProfileKnowledgeProvider("hvlabres", @"c:\temp\publisher\gen", "../gen", "../dist", true, source);

            var tableGenerator = new StructureTableGenerator(pkp);

       //     var lipid = source.ReadConformanceResource("http://hl7.org/fhir/StructureDefinition/lipid-report-lipidprofile") as StructureDefinition;
            var lipid = source.ReadConformanceResource("http://hl7.org/fhir/StructureDefinition/lipid-report-ldl-chol-calculated") as StructureDefinition;
            Assert.IsNotNull(lipid);
            var doc = tableGenerator.generateStructureTable(lipid, false, true);

            var filename = new ResourceIdentity(lipid.Url).Id;

            File.WriteAllText(@"c:\temp\publisher\" + filename + ".html", doc.ToString());

            //{
            //    var publisher = new ProfileTableGenerator(pkp);

            //    var result = File.ReadAllText(@"TestData\publish-header.cshtml");
            //    result += publisher.Generate(profile, false).ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            //    result += File.ReadAllText(@"TestData\publish-footer.cshtml");

            //    File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForProfileTable(profile), result);
            //}

            //{
            //    var publisher = new StructureTableGenerator(pkp);

            //    foreach (var structure in profile.Structure)
            //    {
            //        var result = File.ReadAllText(@"TestData\publish-header.cshtml");
            //        result += publisher.generateStructureTable(structure, false, profile)
            //                .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            //        result += File.ReadAllText(@"TestData\publish-footer.cshtml");

            //        File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForLocalStructure(profile, structure), result);
            //    }
            //}

            //{
            //    var dictgen = new DictHtmlGenerator(pkp);
            //    var result = File.ReadAllText(@"TestData\publish-header.cshtml");
            //    result += dictgen.Generate(profile)
            //                .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            //    result += File.ReadAllText(@"TestData\publish-footer.cshtml");

            //    File.WriteAllText(@"c:\temp\publisher\" + pkp.GetLinkForProfileDict(profile), result);
            //}

            //{
            //    var vs = (ValueSet)FhirParser.ParseResourceFromXml(File.ReadAllText(@"TestData\hv-laboratory-result-interpretation-v1.xml"));

            //    var vsGen = new ValueSetGenerator(pkp);

            //    var result = File.ReadAllText(@"TestData\publish-header.cshtml");
            //    result += vsGen.generate(vs)
            //                .ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            //    result += File.ReadAllText(@"TestData\publish-footer.cshtml");

            //    File.WriteAllText(@"c:\temp\publisher\" + "hv-laboratory-result-interpretation-v1.html", result);
            //}
        }
    }
}
