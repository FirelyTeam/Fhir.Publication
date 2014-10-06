using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{ 
    public class Plan : IWork
    {
        public Context Context { get; set; }

        public List<IFilter> Filters = new List<IFilter>();

        public void Add(IFilter filter)
        {
            this.Filters.Add(filter);
        }

        public void ExecuteFilter(IFilter filter)
        {
            foreach (IWork work in filter.Items)
            {
                work.Execute();
            }
        }
        
        public void Execute()
        {
            foreach(IFilter filter in Filters)
            {
                ExecuteFilter(filter);
            }
        }
    }


    

}
