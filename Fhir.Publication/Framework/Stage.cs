using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class Stage 
    {
        private Queue<Document> queue = new Queue<Document>();
        
        public Stage(IEnumerable<Document> documents)
        {
            this.Post(documents);
        }

        public IEnumerable<Document> Documents
        {
            get
            {
                return queue;
            }
        }
        
        public void Post(Document item)
        {
            queue.Enqueue(item);
        }
        
        public void Post(IEnumerable<Document> documents)
        {
            foreach (var doc in documents)
            {
                queue.Enqueue(doc);
            }
        }

        public Document Take()
        {
            return queue.Dequeue();
        }

        public Document CloneAndPost(Document source)
        {
            Document item = source.CloneMetadata();
            Post(item);
            return item;
        }

        public Document Find(string name)
        {
            return queue.FirstOrDefault(d => d.Name == name);
        }

        public static Stage operator +(Stage stage, IEnumerable<Document> documents)
        {
            stage.Post(documents);
            return stage;
        }

        public static Stage New()
        {
            return new Stage(Enumerable.Empty<Document>());
        } 
    }
}
