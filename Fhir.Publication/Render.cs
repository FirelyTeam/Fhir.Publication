using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class Render : IWork
    {
        public Context Context { get; set; }
        IRenderer renderer;

        public Render(Context context, IRenderer renderer)
        {
            this.Context = context;
            this.renderer = renderer;
        }

        public void PrepareDir()
        {
            Context.EnsureTargetDir();
            Directory.SetCurrentDirectory(Context.CurrentDir);
        }

        public void Execute()
        {
            PrepareDir();
            Log.WriteLine("Rendering {0} to {1}.", Path.Combine(Context.RelativeDir, Context.FileName), Path.GetFileName(Context.TargetFullPath));
            using (Stream input = File.OpenRead(Context.FullPath))
            using (Stream output = File.OpenWrite(Context.TargetFullPath))
            {
                renderer.Render(Context, input, output);
            }
        }
    }
}
