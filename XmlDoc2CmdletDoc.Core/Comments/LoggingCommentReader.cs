using System;
using System.Reflection;
using System.Xml.Linq;

namespace XmlDoc2CmdletDoc.Core.Comments
{
    /// <summary>
    /// <see cref="ICommentReader"/> implementation that decorates an existing instance, reporting a warning whenever a comment lookup occurs and an XML Doc comment
    /// cannot be found.
    /// </summary>
    public class LoggingCommentReader : ICommentReader
    {
        private readonly ICommentReader _proxy;
        private readonly ReportWarning _reportWarning;

        /// <summary>
        /// Creates a new instance that decorates the specified <paramref name="proxy"/>.
        /// </summary>
        /// <param name="proxy">The decorated proxy.</param>
        /// <param name="reportWarning">Used to report failed comment lookups.</param>
        public LoggingCommentReader(ICommentReader proxy, ReportWarning reportWarning)
        {
            if (proxy == null) throw new ArgumentNullException("proxy");
            if (reportWarning == null) throw new ArgumentNullException("reportWarning");

            _proxy = proxy;
            _reportWarning = reportWarning;
        }

        public XElement GetComments(Type type) { return CheckComment(_proxy.GetComments(type), type); }

        public XElement GetComments(FieldInfo fieldInfo) { return CheckComment(_proxy.GetComments(fieldInfo), fieldInfo); }

        public XElement GetComments(PropertyInfo propertyInfo) { return CheckComment(_proxy.GetComments(propertyInfo), propertyInfo); }

        private XElement CheckComment(XElement commentElement, MemberInfo memberInfo)
        {
            if (commentElement == null)
            {
                _reportWarning(memberInfo, "No XML doc comment found.");
            }
            return commentElement;
        }
    }
}