using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace XmlDoc2CmdletDoc.Core.Comments
{
    /// <summary>
    /// Implementation of <see cref="ICommentReader"/> that decorates a proxy instance by caching comment lookups.
    /// </summary>
    public class CachingCommentReader : ICommentReader
    {
        private readonly ICommentReader _proxy;
        private readonly IDictionary<MemberInfo, XElement> _cache;

        /// <summary>
        /// Creates a new instances that delegates to the specified <paramref name="proxy"/>.
        /// </summary>
        /// <param name="proxy">The decorated comment reader.</param>
        public CachingCommentReader(ICommentReader proxy)
        {
            if (proxy == null) throw new ArgumentNullException("proxy");
            _proxy = proxy;
            _cache = new Dictionary<MemberInfo, XElement>();
        }

        public XElement GetComments(Type type)
        {
            XElement element;
            return _cache.TryGetValue(type, out element) ? element : _cache[type] = _proxy.GetComments(type);
        }

        public XElement GetComments(FieldInfo fieldInfo)
        {
            XElement element;
            return _cache.TryGetValue(fieldInfo, out element) ? element : _cache[fieldInfo] = _proxy.GetComments(fieldInfo);
        }

        public XElement GetComments(PropertyInfo propertyInfo)
        {
            XElement element;
            return _cache.TryGetValue(propertyInfo, out element) ? element : _cache[propertyInfo] = _proxy.GetComments(propertyInfo);
        }
    }
}