using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class CopyProcessor : IProcessor
    {

        public void Process(Document input, Stage output)
        {
            input.Context.EnsureTarget();
            Log.WriteLine("Copy {0} to {1}.",
                Path.Combine(input.Context.Source.Directory, input.Name),
                input.Context.Target.Directory);
            File.Copy(input.GetSourceFileName(), input.GetTargetFileName(), true);
        }
    }
}
