using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public interface IWork
    {
        void Execute();
    }

    public interface IWorkFilter : IWork
    {
        IEnumerable<IWork> Select();
    }

}
