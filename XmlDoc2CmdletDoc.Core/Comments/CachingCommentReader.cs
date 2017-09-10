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
            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            _cache = new Dictionary<MemberInfo, XElement>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public XElement GetComments(Type type)
        {
            return _cache.TryGetValue(type, out XElement element) ? element : _cache[type] = _proxy.GetComments(type);
        }

        public XElement GetComments(FieldInfo fieldInfo)
        {
            return _cache.TryGetValue(fieldInfo, out XElement element) ? element : _cache[fieldInfo] = _proxy.GetComments(fieldInfo);
        }

        public XElement GetComments(PropertyInfo propertyInfo)
        {
            return _cache.TryGetValue(propertyInfo, out XElement element) ? element : _cache[propertyInfo] = _proxy.GetComments(propertyInfo);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}