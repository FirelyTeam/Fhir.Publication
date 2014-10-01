using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading.Tasks.Dataflow;

namespace Hl7.Fhir.Documenting
{
    public class XmlLoader : IBlock<SourceFile, XmlDocument>
    {
        public XmlDocument Process(SourceFile input)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(input.FullPath);
            return doc;
        }
    }

    public class XmlTransformer : IBlock<XmlDocument, XmlDocument>
    {

        public XmlDocument Process(XmlDocument input)
        {
            XmlDocument doc = (XmlDocument)input.Clone();
            return doc;
        }
    }

    public class XmlToString : IBlock<XmlDocument, string>
    {

        public string Process(XmlDocument input)
        {
            return input.ToString();
        }
    }

    



}
