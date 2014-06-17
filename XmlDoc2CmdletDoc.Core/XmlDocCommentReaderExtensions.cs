using System;
using System.Linq;
using System.Reflection;
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

        private static readonly IXmlNamespaceResolver xmlNamespaceResolver;

        static XmlDocCommentReaderExtensions()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            xmlNamespaceManager.AddNamespace("", mshNs.NamespaceName);
            xmlNamespaceManager.AddNamespace("maml", mamlNs.NamespaceName);
            xmlNamespaceManager.AddNamespace("command", commandNs.NamespaceName);
            xmlNamespaceManager.AddNamespace("dev", devNs.NamespaceName);
            xmlNamespaceResolver = xmlNamespaceManager;
        }

        public static XElement GetCommandSynopsisElement(this XmlDocCommentReader commentReader, Command command)
        {
            return new XElement(mamlNs + "description",
                                new XElement(mamlNs + "para",
                                             "TODO: Insert the SYNOPSIS text here."));
        }

        public static XElement GetCommandDescriptionElement(this XmlDocCommentReader commentReader, Command command)
        {
            return new XElement(mamlNs + "description",
                                new XElement(mamlNs + "para",
                                             "TODO: Insert the DESCRIPTION here."));
        }

        public static XElement GetParameterDescriptionElement(this XmlDocCommentReader commentReader, Parameter parameter)
        {
            var comments = commentReader.GetComments(parameter.MemberInfo);
            if (comments != null)
            {
                var descriptionElement = comments.XPathSelectElement("maml:description", xmlNamespaceResolver);
                if (descriptionElement != null)
                {
                    descriptionElement = new XElement(descriptionElement);
                    descriptionElement.RemoveAttributes();
                    return descriptionElement;
                }
                var summaryElement = comments.XPathSelectElement("summary", xmlNamespaceResolver);
                if (summaryElement != null)
                {
                    return new XElement(mamlNs + "description",
                                        new XElement(mamlNs + "para", summaryElement.Value));
                }
            }
            return new XElement(mamlNs + "description",
                                new XElement(mamlNs + "para"));
        }

        public static XElement GetParameterDefaultValueElement(this XmlDocCommentReader commentReader, Parameter parameter)
        {
            var defaultValue = parameter.DefaultValue; // TODO: Get the default value from the doc comments?
            if (defaultValue != null)
            {
                return new XElement(devNs + "defaultValue", defaultValue.ToString());
            }
            return null;
        }

        public static XElement GetTypeDescriptionElement(this XmlDocCommentReader commentReader, Type type)
        {
            return new XElement(mamlNs + "description"); // TODO: Get a brief description to the type.
        }

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
    }
}
