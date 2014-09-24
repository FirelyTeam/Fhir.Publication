using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Documenting
{
    public class SourceItem
    {
        public string FullPath { get; set; }
        public string Location { get; set; }
        public string RelativePath { get; set; }
        public string FileName { get; set; }
        public string Extension
        {
            get
            {
                return Path.GetExtension(FullPath);
            }
        }
        public string GetContent()
        {
            return File.ReadAllText(FullPath);
        }
        public override string ToString()
        {
            return string.Join("|", RelativePath, FileName);
        }
    }
}
