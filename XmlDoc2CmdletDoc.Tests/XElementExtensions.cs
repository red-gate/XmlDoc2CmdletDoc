using System.Linq;
using System.Xml.Linq;

namespace XmlDoc2CmdletDoc.Tests
{
    public static class XElementExtensions
    {
        /// <summary>
        /// Returns a simple, clean string representation of the 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string ToSimpleString(this XElement element)
        {
            // Strip out superfluous text and comments from the element.
            element = element.Simplify();

            // Add the element to a container element with the common namespace prefixes defined,
            // so that it adops those prefixes.
            var container = new XElement("container",
                new XAttribute(XNamespace.Xmlns + "maml", "http://schemas.microsoft.com/maml/2004/10"),
                new XAttribute(XNamespace.Xmlns + "command", "http://schemas.microsoft.com/maml/dev/command/2004/10"),
                new XAttribute(XNamespace.Xmlns + "dev", "http://schemas.microsoft.com/maml/dev/2004/10"),
                element);
            element = container.Elements().First();

            // And then format it nicely.
            return element.ToString(SaveOptions.OmitDuplicateNamespaces);
        }

        private static XElement Simplify(this XElement element)
        {
            var newElement = new XElement(element.Name);
            foreach (var attribute in element.Attributes())
            {
                newElement.Add(attribute);
            }
            foreach (var node in element.Nodes())
            {
                if (node is XElement)
                {
                    newElement.Add(((XElement) node).Simplify());
                }
                else if (node is XText)
                {
                    newElement.Add(((XText) node).Value.Trim());
                }
            }
            return newElement;
        }
    }
}