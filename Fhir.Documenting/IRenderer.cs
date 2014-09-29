using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Documenting
{

    public interface IRenderer
    {
        void Render(StreamReader reader, StreamWriter writer);
    }

}
