using System;
using System.Reflection;
using System.Xml.Linq;
using Jolt;

namespace XmlDoc2CmdletDoc.Core.Comments
{
    /// <summary>
    /// Implementation of <see cref="T:XmlDoc2CmdletDoc.Core.Comments.ICommentReader" /> that's based on an instance
    /// of <see cref="T:Jolt.XmlDocCommentReader" /> from the Jolt.Net library.
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

        /// <inheritdoc />
        public XElement GetComments(Type type) => _proxy.GetComments(type);

        /// <inheritdoc />
        public XElement GetComments(FieldInfo fieldInfo) => _proxy.GetComments(fieldInfo);

        /// <inheritdoc />
        public XElement GetComments(PropertyInfo propertyInfo) => _proxy.GetComments(propertyInfo);
    }
}
