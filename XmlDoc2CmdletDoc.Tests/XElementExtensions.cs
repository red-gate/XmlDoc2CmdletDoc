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
            var newElement = new XElement(element.Name);
            foreach (var attribute in element.Attributes())
            {
                newElement.Add(attribute);
            }
            foreach (var node in element.Nodes().Where(x => x is XElement || x is XText))
            {
                if (node is XElement)
                {
                    newElement.Add(((XElement) node).Simplify());
                }
                else
                {
                    newElement.Add(((XText) node).Value.Trim());
                }
            }
            return newElement;
        }
    }
}