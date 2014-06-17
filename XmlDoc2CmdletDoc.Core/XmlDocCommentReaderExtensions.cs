using System;
using System.Xml.Linq;
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
            return new XElement(mamlNs + "description",
                                new XElement(mamlNs + "para",
                                             "TODO: Insert parameter description here."));
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
    }
}
