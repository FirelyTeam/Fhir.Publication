using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public enum DocumentState { Closed, Open }

    public class Document
    {
        public Context Context { get; set; }
        public DocumentState State { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }

        public List<Document> Attachments = new List<Document>();

        public void Attach(params Document[] attachments)
        {
            Attachments.AddRange(attachments);
        }

        public void Attach(IEnumerable<Document> attachments)
        {
            Attachments.AddRange(attachments);
        }
        private string _content;

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

        public string SourceFullPath
        {
            get 
            {
                //string filename = Path.ChangeExtension(Name, Extension);
                string filename = Path.Combine(Context.Source.Directory, FileName);
                return filename;
            }
        }

        public string TargetFullPath
        {
            get 
            {
                string filename = Path.Combine(Context.Target.Directory, FileName);
                return filename;
            }

        }

        
        public void Load()
        {

            lock (this)
            {
                if (State == DocumentState.Closed)
                {
                    _content = File.ReadAllText(SourceFullPath);
                    State = DocumentState.Open;
                }
            }
        }

        public void Save()
        {
            Load();

            Context.EnsureTarget();
            File.WriteAllText(TargetFullPath, _content);

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
        public Document CloneMetadata()
        {
            Document doc = new Document();
            doc.Context = this.Context.Clone();
            doc.Name = this.Name;
            doc.Extension = this.Extension;
            doc.State = this.State;
            return doc;
        }

        public Document Clone()
        {
            Document clone = CloneMetadata();
            clone.Text = this.Text;
            return clone;
        }

        public string FileName
        {
            get
            {
                string extension = Extension;
                if (!extension.StartsWith(".")) extension = "." + extension ;
                return this.Name + extension;
            }
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
            string dir = Path.GetDirectoryName(filename);
            document.Context.MoveTo(dir);
            document.SetFilename(filename);
            return document;
        }

        private Document()
        {

        }

        public override string ToString()
        {
            return string.Format("{0}\\{1}", Context.Source, FileName);
        }
    }
}
