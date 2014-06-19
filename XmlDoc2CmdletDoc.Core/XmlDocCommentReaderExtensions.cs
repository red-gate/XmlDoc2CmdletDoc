using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Jolt;
using XmlDoc2CmdletDoc.Core.Domain;

namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// Extension methods for <see cref="XmlDocCommentReader"/>.
    /// </summary>
    public static class XmlDocCommentReaderExtensions
    {
        private static readonly XNamespace mshNs = XNamespace.Get("http://msh");
        private static readonly XNamespace mamlNs = XNamespace.Get("http://schemas.microsoft.com/maml/2004/10");
        private static readonly XNamespace commandNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/command/2004/10");
        private static readonly XNamespace devNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/2004/10");

        private static readonly IXmlNamespaceResolver resolver;

        static XmlDocCommentReaderExtensions()
        {
            var manager = new XmlNamespaceManager(new NameTable());
            manager.AddNamespace("", mshNs.NamespaceName);
            manager.AddNamespace("maml", mamlNs.NamespaceName);
            manager.AddNamespace("command", commandNs.NamespaceName);
            manager.AddNamespace("dev", devNs.NamespaceName);
            resolver = manager;
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> element for a cmdlet's synopsis.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <returns>A description element for the cmdlet's synopsis.</returns>
        public static XElement GetCommandSynopsisElement(this XmlDocCommentReader commentReader, Command command)
        {
            var commentsElement = commentReader.GetComments(command.CmdletType);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "synopsis");
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> element for a cmdlet's full description.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <returns>A description element for the cmdlet's full description.</returns>
        public static XElement GetCommandDescriptionElement(this XmlDocCommentReader commentReader, Command command)
        {
            var commentsElement = commentReader.GetComments(command.CmdletType);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "description");
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:alertSet&gt;</em> element for a cmdlet's notes.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;maml:alertSet&gt;</em> element for the cmdlet's notes.</returns>
        public static XElement GetAlertSetElement(this XmlDocCommentReader commentReader, Command command)
        {
            var comments = commentReader.GetComments(command.CmdletType);
            if (comments == null)
            {
                return null;
            }

            // First see if there's an alertSet element in the comments
            var alertSet = comments.XPathSelectElement("//maml:alertSet", resolver);
            if (alertSet != null)
            {
                return alertSet;
            }

            // Next, search for a list element of type <em>alertSet</em>.
            var list = comments.XPathSelectElement("//list[@type='alertSet']");
            if (list == null)
            {
                return null;
            }
            alertSet = new XElement(mamlNs + "alertSet");
            foreach (var item in list.XPathSelectElements("item"))
            {
                var term = item.XPathSelectElement("term");
                var description = item.XPathSelectElement("description");
                if (term != null && description != null)
                {
                    var alertTitle = new XElement(mamlNs + "title", Tidy(term.Value));

                    var alert = new XElement(mamlNs + "alert");
                    var paras = description.XPathSelectElements("para").ToList();
                    if (paras.Any())
                    {
                        paras.ForEach(para => alert.Add(new XElement(mamlNs + "para", Tidy(para.Value))));
                    }
                    else
                    {
                        alert.Add(new XElement(mamlNs + "para", Tidy(description.Value)));
                    }

                    alertSet.Add(alertTitle, alert);
                }
            }
            return alertSet;
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> element for a parameter.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A description element for the parameter.</returns>
        public static XElement GetParameterDescriptionElement(this XmlDocCommentReader commentReader, Parameter parameter)
        {
            var commentsElement = commentReader.GetComments(parameter.MemberInfo);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "description");
        }

        /// <summary>
        /// Obtains a <em>&lt;dev:defaultValue&gt;</em> element for a parameter.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value element for the parameter's default value, or <em>null</em> if a default value could not be obtained.</returns>
        public static XElement GetParameterDefaultValueElement(this XmlDocCommentReader commentReader, Parameter parameter)
        {
            var defaultValue = parameter.DefaultValue; // TODO: Get the default value from the doc comments?
            if (defaultValue != null)
            {
                return new XElement(devNs + "defaultValue", defaultValue.ToString());
            }
            return null;
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> element for a type.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="type">The type for which a description is required.</param>
        /// <returns>A description for the type, or an empty description element if no description is available.</returns>
        public static XElement GetTypeDescriptionElement(this XmlDocCommentReader commentReader, Type type)
        {
            var commentsElement = commentReader.GetComments(type);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "description");
        }

        /// <summary>
        /// Helper method to retrieve the XML doc commments from a <see cref="MemberInfo"/> that represents either a property or a field.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="memberInfo">The member whose comments are to be retrieved.</param>
        /// <returns>The XML doc commments for the <paramref name="memberInfo"/>, or<em>null</em> if they are not available.</returns>
        private static XElement GetComments(this XmlDocCommentReader commentReader, MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return commentReader.GetComments((FieldInfo) memberInfo);
                case MemberTypes.Property:
                    return commentReader.GetComments((PropertyInfo) memberInfo);
                default:
                    throw new NotSupportedException("Member type not supported: " + memberInfo.MemberType);

            }
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> from an XML doc comment.
        /// </summary>
        /// <param name="commentsElement">The XML doc comments element as retrieved from an <see cref="XmlDocCommentReader"/>. May be <c>null</c>.</param>
        /// <param name="typeAttribute">
        /// <para>An identifier used to select specific content from the XML doc comment.</para>
        /// <para>The XML doc comment may contain multiple <em>&lt;maml:description&gt;</em> elements, but only one with an
        /// <em>id=&quot;identifier&quot;</em> attribute will be used.</para>
        /// <para>Alternatively, the <em>&lt;summary&gt;</em> element may contain multiple <em>&lt;para&gt;</em> elements. Only those
        /// starting with <em>IDENTIFIER:</em> (the identifier must be upper-cased) will be used to provide content for the description.</para>
        /// </param>
        /// <returns>A description element derived from the XML doc comment, or an empty description element if a description could not be obtained.</returns>
        private static XElement GetMamlDescriptionElementFromXmlDocComment(XElement commentsElement, string typeAttribute)
        {
            if (commentsElement != null)
            {
                // Examine the XML doc comment first for an embedded <maml:description> element.
                var mamlDescriptionElement = commentsElement.XPathSelectElement(string.Format("//maml:description[@type='{0}']", typeAttribute), resolver);
                if (mamlDescriptionElement != null)
                {
                    mamlDescriptionElement = new XElement(mamlDescriptionElement); // Deep clone the element, as we're about to modify it.
                    mamlDescriptionElement.RemoveAttributes(); // Intended to remove the xmlns:maml namespace declaration and id attribute. Assumes there aren't any further attributes.
                    return mamlDescriptionElement;
                }

                // Next try <para type="typeAttribuyt"> elements.
                var paraElements = commentsElement.XPathSelectElements(string.Format("//para[@type='{0}']", typeAttribute)).ToList();
                if (paraElements.Any())
                {
                    var descriptionElement = new XElement(mamlNs + "description");
                    paraElements.ForEach(para => descriptionElement.Add(new XElement(mamlNs + "para", Tidy(para.Value))));
                    return descriptionElement;
                }
            }

            // At this point, we've failed to provide a description from the XML doc commment, so return an empty description.
            return new XElement(mamlNs + "description",
                                new XElement(mamlNs + "para"));
        }

        /// <summary>
        /// Tidies up the text retrieved from an XML doc comment. Multiple whitespace characters, including CR/LF,
        /// are replaced with a single space, and leading and trailing whitespace is removed.
        /// </summary>
        /// <param name="value">The string to tidy.</param>
        /// <returns>The tidied string.</returns>
        private static string Tidy(string value)
        {
            return new Regex(@"\s{2,}").Replace(value, " ").Trim();
        }
    }
}
