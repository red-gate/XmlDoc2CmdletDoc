using System.Linq;
using System.Xml.Linq;

namespace XmlDoc2CmdletDoc.Tests
{
    public static class XElementExtensions
    {
        public static string ToSimpleString(this XElement element)
        {
            element = element.Simplify();
            var container = new XElement("container",
                new XAttribute(XNamespace.Xmlns + "maml", "http://schemas.microsoft.com/maml/2004/10"),
                new XAttribute(XNamespace.Xmlns + "command", "http://schemas.microsoft.com/maml/dev/command/2004/10"),
                new XAttribute(XNamespace.Xmlns + "dev", "http://schemas.microsoft.com/maml/dev/2004/10"),
                element);
            element = container.Elements().First();
            return element.ToString(SaveOptions.OmitDuplicateNamespaces);
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