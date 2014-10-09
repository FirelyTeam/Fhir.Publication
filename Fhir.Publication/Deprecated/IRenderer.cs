using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Deprecated
{

    public interface IRenderer
    {
        void Render(Context context, Stream input, Stream output);
    }


}
