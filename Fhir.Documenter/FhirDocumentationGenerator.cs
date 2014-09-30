using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Documenting;

namespace Hl7.Fhir.DocumenterTool
{
    public static class FhirDocumentation
    {
        public static void Generate(string sourcedir, string targetdir)
        {
            MappingList mappings = new MappingList();
            //mappings.Map(".md", ".html", new TestRenderer());
            IRenderer test = new TestRenderer();
            IRenderer md = new MarkdownRenderer();

            IRenderer pipe = new PipeLine(test, md);

            mappings.Map(".md", ".html", pipe);

            Generator generator = new Generator(sourcedir, targetdir, mappings);
            generator.Generate();
        }
    }
}
