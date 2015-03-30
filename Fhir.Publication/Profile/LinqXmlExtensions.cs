using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using Hl7.Fhir.Support;

namespace Hl7.Fhir.Publication.Profile
{
    internal static class LinqXmlExtensions
    {
        public static XElement AddTag(this XElement elem, string name)
        {
            if (!(elem.NodeType == XmlNodeType.Element || elem.NodeType == XmlNodeType.Document))
                throw new ArgumentException("Wrong node type. is " + elem.NodeType.ToString());
            var node = new XElement(XmlNs.XHTMLNS+name);
            elem.Add(node);
            return node;
        }

        public static XText AddText(this XElement elem, string content)
        {
            if (!(elem.NodeType == XmlNodeType.Element || elem.NodeType == XmlNodeType.Document))
                throw new ArgumentException("Wrong node type. is " + elem.NodeType.ToString());

            if (content != null)
            {
                var node = new XText(content);
                elem.Add(node);
                return node;
            }
            else
                return null;
        }

        public static XElement SetAttribute(this XElement elem, XName name, object value)
        {
            elem.SetAttributeValue(name, value);
            return elem;
        }
    }
}
