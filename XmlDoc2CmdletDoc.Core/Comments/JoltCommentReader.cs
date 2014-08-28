using System;
using System.Reflection;
using System.Xml.Linq;
using Jolt;

namespace XmlDoc2CmdletDoc.Core.Comments
{
    /// <summary>
    /// Implementation of <see cref="ICommentReader"/> that's based on an instance of <see cref="XmlDocCommentReader"/> from the Jolt.Net library.
    /// </summary>
    public class JoltCommentReader : ICommentReader
    {
        private readonly XmlDocCommentReader _proxy;

        /// <summary>
        /// Creates a new instances that delegates to the specified <paramref name="proxy"/>.
        /// </summary>
        /// <param name="proxy">The <see cref="XmlDocCommentReader"/> used to supply XML Doc comments.</param>
        public JoltCommentReader(XmlDocCommentReader proxy)
        {
            if (proxy == null) throw new ArgumentNullException("proxy");
            _proxy = proxy;
        }

        public XElement GetComments(Type type) { return _proxy.GetComments(type); }
        public XElement GetComments(FieldInfo fieldInfo) { return _proxy.GetComments(fieldInfo); }
        public XElement GetComments(PropertyInfo propertyInfo) { return _proxy.GetComments(propertyInfo); }
    }
}