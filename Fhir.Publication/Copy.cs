using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class Copy : IWork
    {
        public Context Context { get; set; }

        public Copy(Context context)
        {
            this.Context = context;
        }

        public void Execute()
        {
            Context.EnsureTargetDir();
            Log.WriteLine("Copy {0} to {1}.", Path.Combine(Context.RelativeDir, Context.FileName), Path.GetFileName(Context.TargetFullPath));
            File.Copy(Context.FullPath, Context.TargetFullPath, true);
        }

        public static IEnumerable<IWork> Filter(Context context, string mask, bool recurse)
        {
            return Work.Filter(context, mask, null, recurse, c => new Copy(c));
        }
    }

}
