using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public interface IWork
    {
        void Execute();
    }

    public class Statement : IWork
    {
        public IFilter Filter { get; set; }
        public PipeLine PipeLine = new PipeLine();

        public void Add(IProcessor processor)
        {
            PipeLine.Add(processor);
        }

        public void Execute()
        {
            throw new NotImplementedException();
       //    PipeLine.Process(Filter);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Filter, PipeLine);
        }
    }

    public class Bulk : IWork
    {
        List<IWork> Worklist = new List<IWork>();

        public void Append(params IWork[] work)
        {
            Worklist.AddRange(work);
        }

        public void Append(IEnumerable<IWork> work)
        {
            Worklist.AddRange(work);
        }
        public void Execute()
        {
            foreach(IWork work in Worklist)
            {
                work.Execute();
            }
        }


    }

    public static class Work
    {
        public static void Append(this Bulk bulk, IFilter filter, PipeLine pipeline)
        {
            var statement = new Statement() { Filter = filter, PipeLine = pipeline };
            bulk.Append(statement);
        }
    }

   
}
