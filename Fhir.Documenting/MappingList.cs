using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
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

        /// <summary>
        /// Add a mapping for rendering one file type into to another 
        /// </summary>
        /// <param name="fromextension">Source file type extension. Must start with a dot. For example: .md or .txt</param>
        /// <param name="toExtension">Target file type extension. Must start with a dot. For example: .html or .xml</param>
        public void Map(string fromextension, string toExtension, IRenderer function)
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
