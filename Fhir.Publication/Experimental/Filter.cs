using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class Filter : IFilter
    {
        public Context Context { get; set; }
        public string Mask { get; set; }
        public bool Recurse { get; set; }
        private IEnumerable<string> FileNames()
        {
            SearchOption option = Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] filenames = Directory.GetFiles(Context.Source.Directory, Mask, option);
            return filenames;
        }
        public IEnumerable<Document> GetItems()
        {
            foreach (string filename in FileNames())
            {
                var document = Document.CreateFromFullPath(Context, filename);
                yield return document;

            }
        }

        public static IFilter Create(Context context, string mask, bool recurse = false)
        {
            Filter filter = new Filter();
            filter.Context = context;
            filter.Mask = mask;
            filter.Recurse = recurse;

            return filter;
        }

        public static Document GetDocument(Context context, string mask)
        {
            IFilter filter = Filter.Create(context, mask, false);
            return filter.GetItems().First();
        }

        public override string ToString()
        {
            return string.Format("{0}\\{1} {2}", Context, Mask, Recurse ? "-recurse" : "");
        }
    }
}
