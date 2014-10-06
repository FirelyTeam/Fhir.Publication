using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public delegate IWork WorkAction(Context context);

    public class Worklist : IFilter
    {
        public WorkAction Action { get; set; }
        public Context context { get; set; }
        public string Mask { get; set; }
        public bool Recurse { get; set; }
        public string ToExt { get; set; }
        private IEnumerable<string> FileNames()
        {
            SearchOption option = Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] filenames = Directory.GetFiles(context.CurrentDir, Mask, option);
            return filenames;
        }
        public IEnumerable<IWork> Items 
        { 
            get
            {
                foreach (string filename in FileNames())
                {
                    Context c = context.Clone(filename, ToExt);
                    yield return Action(c);
                }
            }
        }
    }

    public class Work 
    {

        public static IFilter Filter(Context context, string mask, string toExt, bool recurse, WorkAction action)
        {
            Worklist work = new Worklist();
            
            work.Mask = mask;
            work.context = context;
            work.ToExt = toExt;
            work.Action = action;
            work.Recurse = recurse;

            return work;
        }

        public static IFilter Filter(Context context, string mask, bool recurse, WorkAction action)
        {
            return Work.Filter(context, mask, null, recurse, action);
        }

        public static IFilter Filter(Context context, string mask, WorkAction action)
        {
            return Work.Filter(context, mask, null, false, action);
        }

        public static IFilter Filter<T>(Context context, string mask, string toExt, bool recurse = false) where T: IWork, new()
        {
            return Filter(context, mask, toExt, recurse, 
                delegate(Context c) 
                { 
                    T work = new T(); 
                    work.Context = c;
                    return work;
                }
            );
        }

        public static IFilter Filter<T>(Context context, string mask) where T : IWork, new()
        {
            return Filter<T>(context, mask, null, false);
        }

        /*
        public static IFilter Filter(IEnumerable<Context> contexts, WorkAction action)
        {
            foreach(Context c in contexts)
            {
                yield return action(c);
            }
        }
        */
    }
        
}

