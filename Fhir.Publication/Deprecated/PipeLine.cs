using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Deprecated
{
    public class Pipeline : IRenderer
    {
        List<IRenderer> Renderers = new List<IRenderer>();

        public Pipeline(params IRenderer[] renderers)
        {
            this.Renderers.AddRange(renderers);
        }

        public void Add(params IRenderer[] renderers)
        {
            this.Renderers.AddRange(renderers);
        }

        public void Render(Context context, Stream input, Stream output)
        {
            Stream source = input;
            Stream target = null;
            foreach (IRenderer renderer in Renderers)
            {
                source.Seek(0, SeekOrigin.Begin);
                target = new MemoryStream();
                renderer.Render(context, source, target);
                source = target;
            }
            target.Seek(0, SeekOrigin.Begin);
            target.CopyTo(output);
            output.Flush();
            output.Close();
        }


    }
}
