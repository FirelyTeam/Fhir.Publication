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
        public Context context;
        public string FileName;

        public Source(Context context, string filename)
        {
            this.context = context;
            this.FileName = filename;
        }

        public string FullPath { 
            get 
            {
                return Path.Combine(context.CurrentDir, FileName);
            } 
        }

        public string Location 
        { 
            get 
            {
                return context.CurrentDir;
            }
        }

        public string RelativePath 
        { 
            get 
            {
                return context.RelativeDir;
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
            return string.Join("|", context.RelativeDir, FileName);
        }
    }
    
        
}
