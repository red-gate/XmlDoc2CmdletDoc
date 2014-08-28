using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace XmlDoc2CmdletDoc.Core.Comments
{
    /// <summary>
    /// Implementation of <see cref="ICommentReader"/> that decorates an existing reader, typically <see cref="JoltCommentReader"/>,
    /// and performs simple modifications to the XML Doc comments. Modifications include expanding &lt;see cref="xxxx"&gt; elements.
    /// </summary>
    public class RewritingCommentReader : ICommentReader
    {
        private readonly ICommentReader _proxy;

        /// <summary>
        /// Creates a new instance that decorates the specified <paramref name="proxy"/>.
        /// </summary>
        /// <param name="proxy">The proxy source of comments.</param>
        public RewritingCommentReader(ICommentReader proxy) {
            if (proxy == null) throw new ArgumentNullException("proxy");
            _proxy = proxy;
        }

        public XElement GetComments(Type type) { return RewriteComment(_proxy.GetComments(type)); }
        public XElement GetComments(FieldInfo fieldInfo) { return RewriteComment(_proxy.GetComments(fieldInfo)); }
        public XElement GetComments(PropertyInfo propertyInfo) { return RewriteComment(_proxy.GetComments(propertyInfo)); }

        private static XElement RewriteComment(XElement element)
        {
            if (element != null)
            {
                foreach (var childElement in element.Elements().ToList())
                {
                    WalkElements(childElement, CollapseSeeElement);
                }
            }
            return element;
        }

        private static void WalkElements(XElement element, Func<XElement, bool> action)
        {
            var stack = new Stack<XElement>();
            stack.Push(element);
            while (stack.Count > 0)
            {
                var currentElement = stack.Pop();
                if (action(currentElement))
                {
                    foreach (var childElement in currentElement.Elements())
                    {
                        stack.Push(childElement);
                    }
                }
            }
        }

        private static bool CollapseSeeElement(XElement element)
        {
            if (element.Name.LocalName == "see")
            {
                var attr = element.Attribute("cref");
                if (attr != null)
                {
                    var text = element.Value;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = attr.Value;
                        var lastPeriodIndex = text.LastIndexOf('.');
                        if (lastPeriodIndex != -1)
                        {
                            text = text.Substring(lastPeriodIndex + 1);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        element.ReplaceWith(text);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
