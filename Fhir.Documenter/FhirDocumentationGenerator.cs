using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Documenting;

namespace Hl7.Fhir.DocumenterTool
{
    public static class Generator
    {
        

        public static void Generate(string sourcedir, string targetdir)
        {
            Context context = Context.Root(sourcedir, targetdir);
            Bulk generator = new Bulk();

            //RenderMapping markdown = new RenderMapping(context, ".md", ".html", new MarkdownRenderer());
            //RenderFilter razor = new RenderFilter(context, "*.cshtml", ".html", new RazorRenderer());
            //generator.Add(markdown);

            IWork work = new MakeFilter(context, "make.gen");
           
            generator.Add(work);

            generator.Execute();

        }
        
    }

    
}
