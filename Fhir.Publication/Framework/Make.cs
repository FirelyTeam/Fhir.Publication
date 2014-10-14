using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class Make
    {
        
        public static IWork InterpretDocument(Document document)
        {
            Bulk bulk = new Bulk();
            StringReader reader = new StringReader(document.Text);
            bool ok;
            do
            {
                string statement = reader.ReadLine();
                ok = !string.IsNullOrWhiteSpace(statement);
                if (ok)
                {
                    IWork work = InterpretStatement(document.Context, statement);
                    bulk.Append(work);
                }
            }
            while (ok);
            return bulk;
        }

        public static Statement InterpretStatement(Context context, string text)
        {
            // example statement:
            // select *.md -recursive >>  markdown >> template template.html >> save .html

            Statement statement = new Statement();

            string[] sentences = text.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries);
            statement.Filter = InterpretFilter(context, sentences.First());
            foreach (string s in sentences.Skip(1))
            {
                IProcessor processor = InterpretProcessor(context, s);
                if (processor != null)
                {
                    statement.PipeLine.Add(processor);
                }
                
            }
            return statement;
        }

        public static IProcessor InterpretProcessor(Context context, string text)
        {
            try
            {
                var words = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Count() == 0) 
                    return null;
                
                string command = words.First().ToLower();
                switch (command)
                {
                    case "markdown":
                        return new RenderProcessor(new MarkDownRenderer());

                    case "template":
                    {
                        string s = words.Skip(1).First();
                        TemplateRenderer renderer;
                        if (s.StartsWith("$"))
                        {
                            string key = s;
                            string name = words.Skip(2).First();
                            renderer = new TemplateRenderer(key, name);
                        }
                        else
                        {
                            string template = s;
                            Document document = Document.CreateInContext(context, template);
                            renderer = new TemplateRenderer(document);
                        }
                        
                        return new RenderProcessor(renderer);
                    }

                    case "razor":
                        return new RenderProcessor(new RazorRenderer());

                    case "copy":
                        return new CopyProcessor();

                    case "save":
                    {
                        string extension = words.Skip(1).FirstOrDefault();
                        return new SaveProcessor(extension);
                    }

                    case "stash":
                    {
                        string key = words.Skip(1).First();
                        if (!key.StartsWith("$")) throw new Exception("Stash name should always begin with $.");
                        return new StashProcessor(key);
                    }

                    case "attach":
                    {
                        string key = words.Skip(1).First();
                        string mask = words.Skip(2).First();
                        return new AttachProcessor(key, mask);
                    }

                    case "concatenate":
                        return new ConcatenateProcessor();

                    case "make":
                        return new MakeProcessor();

                    case "profiletable":
                        return new ProfileProcessor();

                    case "structure":
                        return new StructureProcessor();

                    default:
                        return null;
                }
            }
            catch
            {
                throw new Exception("Invalid processor statement: " + text);
            }
        }

        public static IFilter InterpretFilter(Context context, string text)
        {
            try
            {
                var words = text.Split(' ');
                Filter filter = new Filter();
                filter.Mask = words.Skip(1).First();
                filter.Recursive = words.Contains("-recursive");
                filter.FromOutput = words.Contains("-output");
                filter.Context = context.Clone();
                return filter;
            }
            catch
            {
                throw new Exception("Invalid select statement: " + text);
            }

        }

        /*public static IFilter Filter(Context context, string mask, bool recurse = false)
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
            bool recurse = words.Contains("-recurse");


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
        */
        
    }

}
