using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class FileRendering : IWork
    {
        Context context;
        IRenderer renderer;

        public FileRendering(Context context, IRenderer renderer)
        {
            this.context = context;
            this.renderer = renderer;
        }

        public void PrepareDir()
        {
            context.EnsureTargetDir();
            Directory.SetCurrentDirectory(context.CurrentDir);
        }

        public void Execute()
        {
            PrepareDir();
            Log.WriteLine("Rendering {0} to {1}.", Path.Combine(context.RelativeDir, context.FileName), Path.GetFileName(context.TargetFullPath));
            using (Stream input = File.OpenRead(context.FullPath))
            using (Stream output = File.OpenWrite(context.TargetFullPath))
            {
                renderer.Render(context, input, output);
            }
        }
    }
}
