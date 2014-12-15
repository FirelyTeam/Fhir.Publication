using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class FileFilter : ISelector 
    {
        public Context Context { get; set; }
        public string Filter 
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                patterns = parseFilter(value);
            }
        }

        public string Mask { get; set; }

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
        private string _filter;

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
            string filepath = FileFullPathToRelativePath(value).ToLower();

            foreach(string pattern in this.patterns)
            {
                bool match = Regex.IsMatch(filepath, pattern);
                if (match) return true;
            }
            return false;
        }

        private string[] parseFilter(string mask)
        {
            return mask.Split(',').Select(m => Disk.FileMaskToRegExPattern(m)).ToArray();
        }

        private IEnumerable<string> FileNames()
        {
            bool recursive = Recursive | maskIsRecursive(this.Filter);
            return Disk.FilterFiles(Directory, recursive, IsMatch);
        }

        public IEnumerable<Document> Documents
        {
            get
            {
                foreach (string filename in FileNames())
                {
                    var document = Document.CreateFromFullPath(Context, filename);
                    yield return document;

                }
            }
        }

        public static ISelector Create(Context context, string mask, bool recursive = false, bool fromoutput = false)
        {
            FileFilter filter = new FileFilter();
            filter.Context = context;
            filter.Filter = mask;
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
            ISelector filter = FileFilter.Create(context, mask, false);
            Document doc = filter.Documents.FirstOrDefault();
            if (doc == null)
            {
                throw new Exception(string.Format("Mask {0} yielded no results", mask));
            }
            else return doc;
        }

        public override string ToString()
        {

            return string.Format("{0}\\{1} {2}", Context, Filter, Recursive ? "-recursive" : "");
        }

    }

    public class StashFilter : ISelector
    {
        public string Key;
        public string Mask { get; set; }

        public StashFilter(string key, string mask)
        {
            this.Key = key;
            this.Mask = mask;
        }

        
        public IEnumerable<Document> Documents
        {
            get
            {
                return Stash.Get(Key).Documents;
            }
        }

        public override string ToString()
        {
            return string.Format("Stash {0} ({1})", Key, Mask);
        }

    }

    public static class Selector
    {        
        public const string STASHPREFIX = "$";

        public static ISelector Create(Context context, params string[] args)
        {
            string p1 = args.FirstOrDefault();
            string mask = args.Skip(1).Where(a => !a.StartsWith("-")).FirstOrDefault(); // first non option parameter 

            if (p1 == null)
                throw new Exception("No parameters for filter");

            if (p1.StartsWith(STASHPREFIX))
            {
                return new StashFilter(p1, mask);
            }
            else
            {
                FileFilter filter = new FileFilter();
                filter.Filter = p1;
                filter.Recursive = args.Contains("-recursive");
                filter.FromOutput = args.Contains("-output");
                filter.Context = context.Clone();
                filter.Mask = mask;
                return filter;
            }
        }

        public static ISelector Create(Context context, IEnumerable<string> args)
        {
            return Selector.Create(context, args.ToArray());
        }

        public static IEnumerable<Document> Match(this ISelector selector, Document document)
        {
            if (selector.Mask != null)
            {
                string name = Disk.ParseMask(document.Name, selector.Mask).ToLower();
                return selector.Documents.Where(d => d.Name.ToLower() == name);
            }
            else
            {
                return selector.Documents;
            }
            
        }

        public static Document Single(this ISelector selector, Document document)
        {
            
            IEnumerable<Document> documents = Selector.Match(selector, document);
            Document output = documents.FirstOrDefault();
            if (output == null)
            {
                throw new Exception(string.Format("Selection {0} is empty for {1}.", selector, document));
            }
            else return output;
        }
        

    }
}
