using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class Stage 
    {
        public Queue<Document> Queue = new Queue<Document>();

        public static Stage Empty()
        {
            return new Stage(Enumerable.Empty<Document>());
        } 
        
        public Stage(IEnumerable<Document> documents)
        {
            this.Add(documents);
        }
        public void Add(Document item)
        {
            Queue.Enqueue(item);
        }
        
        public void Add(IEnumerable<Document> documents)
        {
            foreach (var doc in documents)
            {
                Queue.Enqueue(doc);
            }
        }

        public Document Take()
        {
            return Queue.Dequeue();
        }
        public Document CreateFrom(Document source)
        {
            Document item = source.Duplicate();
            Add(item);
            return item;
        }
        public static Stage operator +(Stage stage, IEnumerable<Document> documents)
        {
            stage.Add(documents);
            return stage;
        }
    }
}
