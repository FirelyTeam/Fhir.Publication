using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    
    public class Bulk : IWork
    {
        public List<IWork> Worklist = new List<IWork>();

        public void Add(IEnumerable<IWork> worklist)
        {
            this.Worklist.AddRange(worklist);
        }

        public void Add(IWork work)
        {
            this.Worklist.Add(work);
        }

        public void Execute()
        {
            foreach(IWork work in Worklist)
            {
                work.Execute();
            }
        }
    }

}
