using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public interface IProcessor
    {
        void Process(Document input, Stage output);
    }

    public static class Processor
    {
        
        public static Stage Process(this IProcessor processor, Stage input)
        {
            Stage output = Stage.Empty();

            foreach (Document document in input.Queue)
            {
                processor.Process(document, output);
            }
            return output;
        }

        public static Stage Process(this PipeLine pipeline, Stage input)
        {
            Stage output = input;
            foreach (IProcessor p in pipeline.Processors)
            {
                output = p.Process(output);
            }
            return output;
        }

        public static Stage ToStage(this IFilter filter)
        {
            Stage stage = new Stage(filter.GetItems());
            return stage;
        }

        public static Stage Process(this PipeLine pipeline, IFilter filter)
        {
            Stage stage = filter.ToStage();
            return Processor.Process(pipeline, stage);
        }
    }
}


