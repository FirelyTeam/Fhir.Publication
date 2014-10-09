using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public static class Stash
    {
        static Dictionary<string, Stage> stages = new Dictionary<string, Stage>();
        
        public static Stage New(string key)
        {
            Stage stage = Stage.New();
            stages.Add(key, stage);
            return stage;
        }

        public static Stage Assert(string key)
        {
            if (stages.ContainsKey(key))
            {
                return stages[key];
            }
            else 
            {
                return New(key);
            }
        }

        public static void Push(string key, Document document)
        {
            Stage stage = Assert(key);
            stage.Post(document);
        }

        public static Document Get(string key, string name)
        {
            Stage stage = stages[key];
            if (stage == null) throw new ArgumentException(string.Format("Stash {1} does not exist", key));
            return stage.Find(name);
        }
    }
    
}
