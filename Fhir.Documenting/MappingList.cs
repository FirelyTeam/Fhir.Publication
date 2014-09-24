using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Documenting
{
    public class MappingList
    {

        public List<RenderMapping> Mappings = new List<RenderMapping>();

        public RenderMapping GetMapping(string extension)
        {
            foreach (RenderMapping mapping in Mappings)
            {
                if (mapping.IsMappingFor(extension)) return mapping;
            }
            return null;
        }

        public void Map(string fromextension, string toExtension, RenderFunction function)
        {
            RenderMapping mapping = new RenderMapping(fromextension, function, toExtension);
            this.Mappings.Add(mapping);
        }

        public IEnumerable<string> GetSourceExtensions()
        {
            return Mappings.Select(m => m.FromExtension);
        }

    }

}
