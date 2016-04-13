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
        /// Creates a new instances that reads comments from the specified XML Doc comments file.
        /// </summary>
        /// <param name="docCommentsFullPath">The full path of the XML Doc comments file.</param>
        public JoltCommentReader(string docCommentsFullPath)
        {
            if (docCommentsFullPath == null) throw new ArgumentNullException(nameof(docCommentsFullPath));
            _proxy = new XmlDocCommentReader(docCommentsFullPath);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public XElement GetComments(Type type) => _proxy.GetComments(type);
        public XElement GetComments(FieldInfo fieldInfo) => _proxy.GetComments(fieldInfo);
        public XElement GetComments(PropertyInfo propertyInfo) => _proxy.GetComments(propertyInfo);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}