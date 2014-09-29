using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Documenting;

namespace Hl7.Fhir.DocumenterTool
{
    public static class FhirDocumentation
    {
        public static void Generate(string sourcedir, string targetdir)
        {
            MappingList mappings = new MappingList();
            mappings.Map(".md", ".html", new TestRenderer());

            Generator generator = new Generator(sourcedir, targetdir, mappings);
            generator.Generate();
        }
    }
}
