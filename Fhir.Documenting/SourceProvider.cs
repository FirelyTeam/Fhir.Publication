using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Documenting
{

    class SourceProvider
    {
        private string directory;
        private string[] extensions;

        public SourceProvider(string directory, IEnumerable<string> extensions)
        {
            this.directory = directory;
            this.extensions = extensions.ToArray();
        }

        private IEnumerable<string> FileNamesWithExtension(string extension)
        {
            string mask = "*" + extension;
            return Directory.GetFiles(directory, mask, SearchOption.AllDirectories);
        }

        public IEnumerable<string> FileNames()
        {
            return extensions.SelectMany(m => FileNamesWithExtension(m));
        }

        public SourceItem CreateItem(string filename)
        {
            SourceItem item = new SourceItem();
            item.FullPath = filename;
            item.Location = Path.GetDirectoryName(filename);
            item.RelativePath = FileUtils.RelativePath(directory, item.Location);
            item.FileName = Path.GetFileName(filename);
            return item;
        }

        public IEnumerable<SourceItem> GetItems()
        {
            foreach(string filename in FileNames())
            {
                yield return CreateItem(filename);
            }
        }
    }

    
}

