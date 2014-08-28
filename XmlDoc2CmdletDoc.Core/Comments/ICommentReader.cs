using System;
using System.Reflection;
using System.Xml.Linq;

namespace XmlDoc2CmdletDoc.Core.Comments
{
    /// <summary>
    /// Abstracts the mechanism for retrieving the XML Doc comments for types, fields and properties.
    /// </summary>
    public interface ICommentReader
    {
        /// <summary>
        /// Retrieves the XML Doc comment for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type's XML Doc comment, or <em>null</em> if the type doesn't have a comment.</returns>
        XElement GetComments(Type type);

        /// <summary>
        /// Retrieves the XML Doc comment for a field.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        /// <returns>The field's XML Doc comment, or <em>null</em> if the field doesn't have a comment.</returns>
        XElement GetComments(FieldInfo fieldInfo);

        /// <summary>
        /// Retrieves the XML Doc comment for a property.
        /// </summary>
        /// <param name="propertyInfo">The property.</param>
        /// <returns>The property's XML Doc comment, or <em>null</em> if the property doesn't have a comment.</returns>
        XElement GetComments(PropertyInfo propertyInfo);
    }
}