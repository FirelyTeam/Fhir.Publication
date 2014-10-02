using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public static class Make
    {
        public static IWork Interpret(Source source)
        {
            Bulk bulk = new Bulk();
            string[] lines = File.ReadAllLines(source.FullPath);
            foreach (string statement in lines)
            {
                IWork work = Make.Interpret(source, statement);
                bulk.Add(work);
            }
            return bulk;
        }

        public static IWork Interpret(Source source, string statement)
        {
            IWork work;

            string[] words = statement.Split(' ');
            string mask = words.Skip(1).First();
            string command = words.First();


            if (command == "make")
            {
                string target = words.Skip(2).FirstOrDefault();
                bool recurse = (target == "recurse");
                work = new MakeFilter(source.Context, mask, recurse);
            }
            else
            {
                Pipeline pipeline = new Pipeline();
                foreach (string c in command.Split('+'))
                {
                    if (c == "razor") pipeline.Add(new RazorRenderer());
                    if (c == "markdown") pipeline.Add(new MarkdownRenderer());
                }

                string target = words.Skip(2).First();

                work = new RenderFilter(source.Context, mask, target, pipeline);
            }

            return work;
        }
    }

}
