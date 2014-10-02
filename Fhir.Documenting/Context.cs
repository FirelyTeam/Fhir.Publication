using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class Context
    {
        public string RootDir;
        public string TargetRootDir;
        public string CurrentDir;

        public string RelativeDir
        {
            get
            {
                return FileUtils.RelativePath(RootDir, CurrentDir);
            }
        }

        public string TargetDir
        {
            get
            {
                return Path.Combine(TargetRootDir, RelativeDir);
            }
        }

        public void EnsureTarget()
        {
            FileUtils.EnsurePath(TargetDir);
        }

        public static Context Root(string source, string target)
        {
            Context context = new Context();
            context.RootDir = source;
            context.TargetRootDir = target;
            context.CurrentDir = source;
            return context;

        }

        public Context Clone()
        {
            Context context = new Context();
            context.RootDir = this.RootDir;
            context.TargetRootDir = this.TargetRootDir;
            context.CurrentDir = this.CurrentDir;
            return context;
        }

        public Context Clone(string directory)
        {
            Context context = this.Clone();
            context.CurrentDir = directory;
            return context;
        }
    }
}
