using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Publication;

namespace Hl7.Fhir.Publication.Deprecated
{
    public static class Make
    {
        public static IFilter Filter(Context context, string mask, bool recurse = false)
        {
            return Work.Filter(context, mask, null, recurse, c => Make.Interpret(c));
        }

        public static IWork Interpret(Context context)
        {
            Plan bulk = new Plan();
            string[] lines = File.ReadAllLines(context.FullPath);
            foreach (string statement in lines)
            {
                var filter = Make.Interpret(context, statement);
                
                if (filter != null) bulk.Add(filter);
            }
            return bulk;
        }
        
        public static IFilter Interpret(Context context, string statement)
        {
            if (string.IsNullOrWhiteSpace(statement)) return null;

            string[] words = statement.Split(' ');
            string mask = words.Skip(1).First();
            string command = words.First();
            bool recurse = words.Contains("-recursive");
            

            if (words.Contains("-fromoutput"))
            {
                string target = context.TargetDir;
                context = context.Clone();
                context.FullPath = target;
            }
            if (command == "make")
            {
                string target = words.Skip(2).FirstOrDefault();
                return Make.Filter(context, mask, recurse);
            }
            else if (command == "copy")
            {
                string target = words.Skip(2).FirstOrDefault();
                return Work.Filter<Copy>(context, mask, null, recurse);
            }
            
            else if (command == "structure")
            {
                return Work.Filter<StructureWork>(context, mask, null, recurse);
            }
            else if (command == "template")
            {
                return Work.Filter<Templater>(context, mask, null, recurse);
            }
            else
            {
                Pipeline pipeline = new Pipeline();
                foreach (string c in command.Split('+'))
                {
                    if (c == "razor") pipeline.Add(new RazorRenderer());
                    if (c == "markdown") pipeline.Add(new MarkdownRenderer());
                    if (c == "profiletable") pipeline.Add(new ProfileTableRenderer());
                    //if (c == "structure") pipeline.Add(new StructureRenderer());


                }

                string target = words.Skip(2).First();
                
                return Work.Filter(context, mask, target, recurse, c => new Render(c, pipeline));
            }
           
        }
    }

}
