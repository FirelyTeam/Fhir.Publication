using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Documenting
{
    public delegate string RenderFunction(string input);

    public class RenderMapping
    {
        public string FromExtension;
        public string ToExtension;
        public RenderFunction Render { get; set; }
        public RenderMapping(string fromExtension, RenderFunction function, string toExtension)
        {
            this.FromExtension = fromExtension;
            this.ToExtension = toExtension;
            this.Render = function;
        }
        public bool IsMappingFor(string extension)
        {
            return FromExtension == extension;
        }
    }
}
