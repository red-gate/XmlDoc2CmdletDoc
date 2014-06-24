using System.Linq;
using System.Xml.Linq;

namespace XmlDoc2CmdletDoc.Tests
{
    public static class XElementExtensions
    {
        public static string ToSimpleString(this XElement element)
        {
            return element.Simplify().ToString(SaveOptions.OmitDuplicateNamespaces);
        }

        private static XElement Simplify(this XElement element)
        {
            foreach (var node in element.Nodes().ToList())
            {
                if (node is XElement)
                {
                    var xElement = (XElement) node;
                    Simplify(xElement);
                }
                else if (node is XText)
                {
                    var text = (XText) node;
                    text.Value = text.Value.Trim();
                }
                else
                {
                    node.Remove();
                }
            }
            return element;
        }
    }
}