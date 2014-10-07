using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public interface IProcessor
    {
        void Process(Document input, Stage output);
    }

    public static class Processor
    {
        
        public static Stage Process(this IProcessor processor, Stage stage)
        {
            Stage output = Stage.Empty();

            foreach (Document document in stage.Queue)
            {
                processor.Process(document, output);
            }
            return output;
        }

        public static Stage Process(this PipeLine pipeline, Stage source)
        {
            Stage stage = source;
            foreach (IProcessor p in pipeline.Processors)
            {
                stage = p.Process(stage);
            }
            return stage;
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


