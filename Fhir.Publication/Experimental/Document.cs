using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public enum DocumentState { Closed, Open }

    public class Document
    {
        public Context Context { get; set; }
        public DocumentState State { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string GetSourceFileName()
        {
            string filename = Path.ChangeExtension(Name, Extension);
            filename = Path.Combine(Context.Source.Directory, filename);
            return filename;
        }

        public string GetTargetFileName()
        {
            string filename = Path.ChangeExtension(Name, Extension);
            filename = Path.Combine(Context.Target.Directory, filename);

            return filename;
        }

        private string _content;
        public void Load()
        {

            lock (this)
            {
                if (State == DocumentState.Closed)
                {
                    string filename = GetSourceFileName();
                    _content = File.ReadAllText(filename);
                    State = DocumentState.Open;
                }
            }
        }
        public void Save()
        {
            Load();
            string filename = GetTargetFileName();
            Context.EnsureTarget();
            File.WriteAllText(filename, _content);

        }
        public string Text
        {
            get
            {
                Load();
                return _content;
            }
            set
            {
                _content = value;
                State = DocumentState.Open;
            }
        }

        public Document Append(string value)
        {
            _content = _content + value;
            State = DocumentState.Open;
            return this;
        }

        /// <summary>
        /// Create a new Item, based on the current item, but with a new stream
        /// </summary>
        /// <returns></returns>
        public Document Duplicate()
        {
            Document doc = new Document();
            doc.Context = this.Context.Clone();
            doc.Name = this.Name;
            doc.Extension = this.Extension;
            doc.State = this.State;
            return doc;
        }
        public void SetFilename(string filename, string extension = null)
        {
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.Extension = (extension != null) ? extension : Path.GetExtension(filename);
        }
        public static Document CreateFromFullPath(Context context, string fullpath)
        {
            Document document = new Document();
            document.Context = Context.CreateFromSource(context.Root, fullpath);
            document.SetFilename(fullpath);
            return document;
        }
        public static Document CreateInContext(Context context, string filename)
        {
            Document document = new Document();
            document.Context = context.Clone();
            document.SetFilename(filename);
            return document;
        }
        private Document()
        {

        }

        public override string ToString()
        {
            string name = Path.ChangeExtension(Name, Extension);
            return string.Format("{0}\\{1}", Context.Source, name);
        }
    }
}
