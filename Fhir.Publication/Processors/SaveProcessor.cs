using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class SaveProcessor : IProcessor
    {
        public ISelector Influx { get; set; }

        public string Mask { get; set; }

        public void Process(Document input, Stage output)
        {
            if (Mask != null)
            {
                if (Mask.StartsWith("."))
                {
                    input.Extension = Mask;
                }
                else 
                {
                    string s = Disk.ParseMask(input.Name, Mask);
                    input.SetFilename(s);
                }
            }

            input.Save();
        }
    }
}
