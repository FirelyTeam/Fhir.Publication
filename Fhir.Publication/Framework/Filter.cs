using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class Filter : IFilter 
    {
        public Context Context { get; set; }
        public string Mask 
        {
            get
            {
                return _mask;
            }
            set
            {
                _mask = value;
                patterns = parseMask(value);
            }
        }
        public bool Recursive { get; set; }
        public bool FromOutput { get; set; }
        public string Directory
        {
            get
            {
                string dir = FromOutput ? Context.Target.Directory : Context.Source.Directory;
                return dir;
            }
        }
        
        private string[] patterns { get; set; }
        private string _mask;

        private string FileFullPathToRelativePath(string value)
        {
            string filename = Path.GetFileName(value);
            string dir = Path.GetDirectoryName(value);
            string location = Disk.RelativePath(Directory, dir);
            string s = Path.Combine(location, filename).ToLower();
            return s;
        }

        public bool IsMatch(string value)
        {
            string filepath = FileFullPathToRelativePath(value);

            foreach(string pattern in this.patterns)
            {
                bool match = Regex.IsMatch(filepath, pattern);
                if (match) return true;
            }
            return false;
        }

        private string[] parseMask(string mask)
        {
            return mask.Split(',').Select(m => Disk.FileMaskToRegExPattern(m)).ToArray();
        }

        private IEnumerable<string> FileNames()
        {
            bool recursive = Recursive | maskIsRecursive(this.Mask);
            return Disk.FilterFiles(Directory, recursive, IsMatch);
        }

        public IEnumerable<Document> GetItems()
        {
            foreach (string filename in FileNames())
            {
                var document = Document.CreateFromFullPath(Context, filename);
                yield return document;

            }
        }

        public static IFilter Create(Context context, string mask, bool recursive = false, bool fromoutput = false)
        {
            Filter filter = new Filter();
            filter.Context = context;
            filter.Mask = mask;
            filter.Recursive = recursive;
            filter.FromOutput = fromoutput;

            return filter;
        }

        private static bool maskIsRecursive(string mask)
        {
            return mask.Contains('\\');
        }

        public static Document GetDocument(Context context, string mask)
        {
            IFilter filter = Filter.Create(context, mask, false);
            return filter.GetItems().First();
        }

        public override string ToString()
        {

            return string.Format("{0}\\{1} {2}", Context, Mask, Recursive ? "-recursive" : "");
        }

        
    }
}
