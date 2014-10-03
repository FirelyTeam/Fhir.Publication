﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{

    public class Work 
    {
        public delegate IWork WorkAction(Context context);

        public static IEnumerable<IWork> Filter(Context context, string mask, string toExt, bool recurse, WorkAction action)
        {
            SearchOption option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] filenames = Directory.GetFiles(context.CurrentDir, mask, option);
            foreach (string filename in filenames)
            {
                Context c = context.Clone(filename, toExt);
                yield return action(c);
            }
        }

        public static IEnumerable<IWork> Filter<T>(Context context, string mask, string toExt, bool recurse) where T: IWork, new()
        {
            return Filter(context, mask, toExt, recurse, 
                delegate(Context c) 
                { 
                    T work = new T(); 
                    work.Context = c;
                    return (IWork) work;
                }
            );

        }

        public static IEnumerable<IWork> Filter(IEnumerable<Context> contexts, WorkAction action)
        {
            foreach(Context c in contexts)
            {
                yield return action(c);
            }
        }

    }
        
}

