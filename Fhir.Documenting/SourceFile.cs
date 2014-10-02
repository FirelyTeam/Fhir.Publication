using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Documenting
{
    public class Source
    {
        public Context Context;
        public string FileName;

        public Source(Context context, string filename)
        {
            this.Context = context;
            this.FileName = filename;
        }

        public string FullPath { 
            get 
            {
                return Path.Combine(Context.CurrentDir, FileName);
            } 
        }

        public string Location 
        { 
            get 
            {
                return Context.CurrentDir;
            }
        }

        public string RelativePath 
        { 
            get 
            {
                return Context.RelativeDir;
            }
        }

        public string Extension
        {
            get
            {
                return Path.GetExtension(FullPath);
            }
        }

        public override string ToString()
        {
            return string.Join("|", Context.RelativeDir, FileName);
        }
    }
    
        
}
