using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    
    public class Generator
    {
        private string sourcedir, targetdir;
        private MappingList mappings;
        private SourceProvider provider;

        public Generator(string sourcedir, string targetdir, MappingList mappings)
        {
            this.sourcedir = sourcedir;
            this.targetdir = targetdir;
            this.mappings = mappings;
            provider = createProvider(mappings);
        }

        private SourceProvider createProvider(MappingList mappings)
        {
            IEnumerable<string> extensions = mappings.GetSourceExtensions();
            return new SourceProvider(sourcedir, extensions);
        }

        public void EnsurePath(string path)
        {
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string TargetLocation(SourceFile item)
        {
            //string path = Path.GetDirectoryName(relativePath);
            string location = Path.Combine(this.targetdir, item.RelativePath);
            return location;
        }

        public string TargetFile(SourceFile item, RenderMapping mapping)
        {
            string location = TargetLocation(item);
            string corename = Path.GetFileNameWithoutExtension(item.FileName);
            string target = location + "\\" + corename + mapping.ToExtension;
            return target;
        }
        
        public void GenerateItem(SourceFile item)
        {
            RenderMapping mapping = mappings.GetMapping(item.Extension);
            if (mapping == null) throw new Exception("Mapping not found for " + item.FileName);

            string location = TargetLocation(item);
            EnsurePath(location);
            string outputfile = TargetFile(item, mapping);
            
            mapping.Render(item, outputfile);
        }

        public void Generate(IEnumerable<SourceFile> items)
        {
            foreach(SourceFile item in items)
            {
                GenerateItem(item);
            }
        }

        public void Generate()
        {
            List<SourceFile> items = provider.GetItems().ToList();
            Generate(items);
        }
    }

}
