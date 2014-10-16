using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class CopyProcessor : IProcessor
    {
        public ISelector Influx { get; set; }

        public void Process(Document input, Stage output)
        {
            input.Context.EnsureTarget();
            Log.Debug("Copy \n from: {0} \n ..to: {1}.", 
                Path.Combine(input.Context.Source.Directory, input.Name),
                input.Context.Target.Directory);

            File.Copy(input.SourceFullPath, input.TargetFullPath, true);
        }
    }
}
