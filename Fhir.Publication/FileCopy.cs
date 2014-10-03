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
        Context context;
        string mask;
        bool recurse;

        public Copy(Context context, string mask, bool recurse = false)
        {
            this.context = context;
            this.mask = mask;
            this.recurse = recurse;
        }

        private void CopyFile(Context context)
        {
            context.EnsureTargetDir();
            Log.WriteLine("Copy {0} to {1}.", Path.Combine(context.RelativeDir, context.FileName), Path.GetFileName(context.TargetFullPath));
            File.Copy(context.FullPath, context.TargetFullPath, true);
        }

        public void Execute()
        {
            SearchOption option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(context.CurrentDir, mask, option);
            foreach (string filename in files)
            {
                Context file = this.context.Clone(filename);
                CopyFile(file);
            }
            
        }
    }
}
