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
            bool ok; ;
            do
            {
                string statement = reader.ReadLine();
                bool skip = (string.IsNullOrWhiteSpace(statement)) || (statement.StartsWith("//"));
                ok = (statement != null);
                if (skip) continue;
                
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
            statement.Selector = InterpretSelector(context, sentences.First());
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
                var parameters = words.Skip(1);

                switch (command)
                {
                    case "markdown":
                        return new RenderProcessor(new MarkDownRenderer());

                    case "template":
                    {
                        var processor = new TemplateProcessor();
                        //pr.Template = influx.Documents.First(); //Document document = Document.CreateInContext(context, template);
                        processor.Influx = Selector.Create(context, parameters);
                        return processor;
                    }

                    case "razor":
                        return new RenderProcessor(new RazorRenderer());

                    case "copy":
                        return new CopyProcessor();

                    case "save":
                    {
                        string mask = parameters.FirstOrDefault();
                        var processor = new SaveProcessor();
                        processor.Mask = mask;
                        return processor;
                    }

                    case "stash":
                    {
                        string key = words.Skip(1).First();
                        if (!key.StartsWith(Selector.STASHPREFIX)) throw new Exception("Stash name should always begin with " + Selector.STASHPREFIX);
                        return new StashProcessor(key);
                    }

                    case "attach":
                    {
                        IProcessor p = new AttachProcessor();
                        p.Influx = Selector.Create(context, parameters);
                        return p;
                    }

                    case "concatenate":
                        return new ConcatenateProcessor();

                    case "make":
                        return new MakeProcessor();

                    case "profiletable":
                        return new ProfileProcessor();

                    case "structure":
                        return new StructureProcessor();

                    case "dict":
                        return new DictTableProcessor();

                    case "valueset":
                        return new ValueSetProcessor();

                    default:
                        throw new Exception("Unknown processing command: " + command);
                }
            }
            catch
            {
                throw new Exception("Invalid processor statement: " + text);
            }
        }

        public static ISelector InterpretSelector(Context context, string text)
        {
            try
            {
                var words = text.Split(' ');
                string command = words.First();
                var parameters = words.Skip(1).ToArray();
                return Selector.Create(context, parameters);
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
