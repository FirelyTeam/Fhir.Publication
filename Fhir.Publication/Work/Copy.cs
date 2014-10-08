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
        
        public void Execute()
        {
            Context.EnsureTargetDir();
            Log.Debug("Copy {0} to {1}.", Path.Combine(Context.RelativeDir, Context.FileName), Path.GetFileName(Context.TargetFullPath));
            File.Copy(Context.FullPath, Context.TargetFullPath, true);
        }
        
    }

}
