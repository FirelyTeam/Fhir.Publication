using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public static class Make
    {
        public static IWork Interpret(Context context)
        {
            Bulk bulk = new Bulk();
            string[] lines = File.ReadAllLines(context.FullPath);
            foreach (string statement in lines)
            {
                IWork work = Make.Interpret(context, statement);
                bulk.Add(work);
            }
            return bulk;
        }
        
        public static IWork Interpret(Context context, string statement)
        {
            IWork work;

            string[] words = statement.Split(' ');
            string mask = words.Skip(1).First();
            string command = words.First();
            bool recurse = words.Contains("-recurse");

            if (command == "make")
            {
                string target = words.Skip(2).FirstOrDefault();
                work = new MakeFilter(context, mask, recurse);
            }
            else if (command == "copy")
            {
                string target = words.Skip(2).FirstOrDefault();
                work = new Copy(context, mask, recurse);
            }
            else if (command == "profiletable")
            {
                var file = context.Clone(mask, ".html");
                work =  new ProfileTableWork(file);
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

                work = new Filter(context, mask, target, recurse, pipeline);
            }

            return work;
        }
    }

}
