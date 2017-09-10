using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using XmlDoc2CmdletDoc.Core.Comments;
using XmlDoc2CmdletDoc.Core.Domain;

namespace XmlDoc2CmdletDoc.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ICommentReader"/>.
    /// </summary>
    public static class CommentReaderExtensions
    {
        private static readonly XNamespace MshNs = XNamespace.Get("http://msh");
        private static readonly XNamespace MamlNs = XNamespace.Get("http://schemas.microsoft.com/maml/2004/10");
        private static readonly XNamespace CommandNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/command/2004/10");
        private static readonly XNamespace DevNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/2004/10");

        private static readonly IXmlNamespaceResolver Resolver;

        static CommentReaderExtensions()
        {
            var manager = new XmlNamespaceManager(new NameTable());
            manager.AddNamespace("", MshNs.NamespaceName);
            manager.AddNamespace("maml", MamlNs.NamespaceName);
            manager.AddNamespace("command", CommandNs.NamespaceName);
            manager.AddNamespace("dev", DevNs.NamespaceName);
            Resolver = manager;
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> element for a cmdlet's synopsis.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Used to record warnings.</param>
        /// <returns>A description element for the cmdlet's synopsis.</returns>
        public static XElement GetCommandSynopsisElement(this ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var cmdletType = command.CmdletType;
            var commentsElement = commentReader.GetComments(cmdletType);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "synopsis", warningText => reportWarning(cmdletType, warningText));
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> element for a cmdlet's full description.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Used to record warnings.</param>
        /// <returns>A description element for the cmdlet's full description.</returns>
        public static XElement GetCommandDescriptionElement(this ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var cmdletType = command.CmdletType;
            var commentsElement = commentReader.GetComments(cmdletType);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "description", warningText => reportWarning(cmdletType, warningText));
        }

        /// <summary>
        /// Obtains a <em>&lt;command:examples&gt;</em> element for a cmdlet's examples.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>An examples element for the cmdlet.</returns>
        public static XElement GetCommandExamplesElement(this ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var cmdletType = command.CmdletType;
            var comments = commentReader.GetComments(cmdletType);
            if (comments == null)
            {
                reportWarning(cmdletType, "No XML doc comment found.");
                return null;
            }

            var xmlDocExamples = comments.XPathSelectElements("//example").ToList();
            if (!xmlDocExamples.Any())
            {
                reportWarning(cmdletType, "No examples found.");
                return null;
            }

            var examples = new XElement(CommandNs + "examples");
            int exampleNumber = 1;
            foreach (var xmlDocExample in xmlDocExamples)
            {
                var example = GetCommandExampleElement(xmlDocExample, exampleNumber, warningText => reportWarning(cmdletType, warningText));
                if (example != null)
                {
                    examples.Add(example);
                    exampleNumber++;
                }
            }
            return exampleNumber == 1 ? null : examples;
        }

        /// <summary>
        /// Obtains a <em>&lt;command:example&gt;</em> element based on an <em>&lt;example&gt;</em> XML doc comment element.
        /// </summary>
        /// <param name="exampleElement">The XML doc comment example element.</param>
        /// <param name="exampleNumber">The number of the example.</param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>An example element.</returns>
        private static XElement GetCommandExampleElement(XElement exampleElement, int exampleNumber, Action<string> reportWarning)
        {
            var items = exampleElement.XPathSelectElements("para | code").ToList();
            var intros = items.TakeWhile(x => x.Name == "para").ToList();
            var code = items.SkipWhile(x => x.Name == "para").TakeWhile(x => x.Name == "code").FirstOrDefault();
            var paras = items.SkipWhile(x => x.Name == "para").SkipWhile(x => x.Name == "code").ToList();

            var example = new XElement(CommandNs + "example",
                           new XElement(MamlNs + "title", $"----------  EXAMPLE {exampleNumber}  ----------"));

            bool isEmpty = true;
            if (intros.Any())
            {
                var introduction = new XElement(MamlNs + "introduction");
                intros.ForEach(intro => introduction.Add(new XElement(MamlNs + "para", Tidy(intro.Value))));
                example.Add(introduction);
                isEmpty = false;
            }
            if (code != null)
            {
                example.Add(new XElement(DevNs + "code", TidyCode(code.Value)));
                isEmpty = false;
            }
            if (paras.Any())
            {
                var remarks = new XElement(DevNs + "remarks");
                paras.ForEach(para => remarks.Add(new XElement(MamlNs + "para", Tidy(para.Value))));
                example.Add(remarks);
                isEmpty = false;
            }

            if (isEmpty)
            {
                reportWarning($"No para or code elements found for example {exampleNumber}.");
            }

            return example;
        }

        /// <summary>
        /// Obtains a <em>&lt;command:relatedLinks&gt;</em> element for a cmdlet's related links.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>An relatedLinks element for the cmdlet.</returns>
        public static XElement GetCommandRelatedLinksElement(this ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var cmdletType = command.CmdletType;
            var comments = commentReader.GetComments(cmdletType);
            if (comments == null)
            {
                return null;
            }

            var paras = comments.XPathSelectElements("//para[@type='link']").ToList();
            if (!paras.Any()) return null;

            var relatedLinks = new XElement(MamlNs + "relatedLinks");
            foreach (var para in paras)
            {
                var navigationLink = new XElement(MamlNs + "navigationLink",
                                                  new XElement(MamlNs + "linkText", para.Value));
                var uri = para.Attribute("uri");
                if (uri != null)
                {
                    navigationLink.Add(new XElement(MamlNs + "uri", uri.Value));
                }
                relatedLinks.Add(navigationLink);
            }
            return relatedLinks;
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:alertSet&gt;</em> element for a cmdlet's notes.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>A <em>&lt;maml:alertSet&gt;</em> element for the cmdlet's notes.</returns>
        public static XElement GetCommandAlertSetElement(this ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var cmdletType = command.CmdletType;
            var comments = commentReader.GetComments(cmdletType);
            if (comments == null)
            {
                return null;
            }

            // First see if there's an alertSet element in the comments
            var alertSet = comments.XPathSelectElement("//maml:alertSet", Resolver);
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
            alertSet = new XElement(MamlNs + "alertSet");
            foreach (var item in list.XPathSelectElements("item"))
            {
                var term = item.XPathSelectElement("term");
                var description = item.XPathSelectElement("description");
                if (term != null && description != null)
                {
                    var alertTitle = new XElement(MamlNs + "title", Tidy(term.Value));

                    var alert = new XElement(MamlNs + "alert");
                    var paras = description.XPathSelectElements("para").ToList();
                    if (paras.Any())
                    {
                        paras.ForEach(para => alert.Add(new XElement(MamlNs + "para", Tidy(para.Value))));
                    }
                    else
                    {
                        alert.Add(new XElement(MamlNs + "para", Tidy(description.Value)));
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
        /// <param name="reportWarning">Used to record warnings.</param>
        /// <returns>A description element for the parameter.</returns>
        public static XElement GetParameterDescriptionElement(this ICommentReader commentReader, Parameter parameter, ReportWarning reportWarning)
        {
            var memberInfo = parameter.MemberInfo;
            var commentsElement = commentReader.GetComments(memberInfo);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "description", warningText => reportWarning(memberInfo, warningText));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="reportWarning">Used to record warnings.</param>
        /// <returns>A description element for the parameter.</returns>
        public static XElement GetParameterValueElement(this ICommentReader commentReader, Parameter parameter, ReportWarning reportWarning)
        {
            return new XElement(CommandNs + "parameterValue",
                                new XAttribute("required", true),
                                GetSimpleTypeName(parameter.ParameterType));
        }

        /// <summary>
        /// Obtains a <em>&lt;dev:defaultValue&gt;</em> element for a parameter.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="reportWarning">Used to record warnings.</param>
        /// <returns>A default value element for the parameter's default value, or <em>null</em> if a default value could not be obtained.</returns>
        public static XElement GetParameterDefaultValueElement(this ICommentReader commentReader, Parameter parameter, ReportWarning reportWarning)
        {
            var defaultValue = parameter.GetDefaultValue(reportWarning); // TODO: Get the default value from the doc comments?
            if (defaultValue != null)
            {
                if (defaultValue is IEnumerable enumerable)
                {
                    var content = string.Join(", ", enumerable.Cast<object>().Select(element => element.ToString()));
                    if (content != "")
                    {
                        return new XElement(DevNs + "defaultValue", content);
                    }
                }
                else
                {
                    return new XElement(DevNs + "defaultValue", defaultValue.ToString());
                }
            }
            return null;
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> for an <em>&lt;command:inputType&gt;</em> coresponding to a specified parameter that accepts pipeline input.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>A <em>&lt;maml:description&gt;</em> for an <em>&lt;command:inputType&gt;</em> for the parameter, or null if no explicit description is available
        /// for the input type.</returns>
        public static XElement GetInputTypeDescriptionElement(this ICommentReader commentReader, Parameter parameter, ReportWarning reportWarning)
        {
            var parameterMemberInfo = parameter.MemberInfo;
            var commentsElement = commentReader.GetComments(parameterMemberInfo);

            // First try to read the explicit inputType description.
            var inputTypeDescription = GetMamlDescriptionElementFromXmlDocComment(commentsElement, "inputType", _ => { });
            if (inputTypeDescription != null)
            {
                return inputTypeDescription;
            }

            // Then fall back to using the parameter description.
            var parameterDescription = commentReader.GetParameterDescriptionElement(parameter, reportWarning);
            if (parameterDescription == null)
            {
                reportWarning(parameterMemberInfo, "No inputType comment found and no fallback description comment found.");
            }
            return parameterDescription;
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> for a command's output type.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="command">The command.</param>
        /// <param name="outputType">The output type of the command.</param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>A <em>&lt;maml:description&gt;</em> for the command's output type,
        /// or null if no explicit description is available for the output type.</returns>
        public static XElement GetOutputTypeDescriptionElement(this ICommentReader commentReader,
                                                               Command command,
                                                               Type outputType,
                                                               ReportWarning reportWarning)
        {
            // TODO: Get the description from the <remarks type="outputType" cref="<type>"> element
            return commentReader.GetTypeDescriptionElement(outputType, reportWarning);
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> element for a type.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="type">The type for which a description is required.</param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>A description for the type, or null if no description is available.</returns>
        public static XElement GetTypeDescriptionElement(this ICommentReader commentReader, Type type, ReportWarning reportWarning)
        {
            var commentsElement = commentReader.GetComments(type);
            return GetMamlDescriptionElementFromXmlDocComment(commentsElement, "description", warningText => reportWarning(type, warningText));
        }

        /// <summary>
        /// Helper method to retrieve the XML doc commments from a <see cref="MemberInfo"/> that represents either a property or a field.
        /// </summary>
        /// <param name="commentReader">The comment reader.</param>
        /// <param name="memberInfo">The member whose comments are to be retrieved.</param>
        /// <returns>The XML doc commments for the <paramref name="memberInfo"/>, or<em>null</em> if they are not available.</returns>
        private static XElement GetComments(this ICommentReader commentReader, MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return commentReader.GetComments((FieldInfo)memberInfo);

                case MemberTypes.Property:
                    return commentReader.GetComments((PropertyInfo)memberInfo);

                default:
                    throw new NotSupportedException("Member type not supported: " + memberInfo.MemberType);
            }
        }

        /// <summary>
        /// Obtains a <em>&lt;maml:description&gt;</em> from an XML doc comment.
        /// </summary>
        /// <param name="commentsElement">The XML doc comments element as retrieved from an <see cref="ICommentReader"/>. May be <c>null</c>.</param>
        /// <param name="typeAttribute">
        /// <para>An identifier used to select specific content from the XML doc comment.</para>
        /// <para>The first <em>&lt;maml:description&gt;</em> element with a <em>type=&quot;&lt;<paramref name="typeAttribute"/>&gt;&quot;</em>
        /// attribute will be used to provide the description (where <em>&lt;<paramref name="typeAttribute"/>&gt;</em> is the value of the
        /// <paramref name="typeAttribute"/> parameter).</para>
        /// <para>Alternatively, the XML Doc comments may contain multiple <em>&lt;para&gt;</em> elements. Those
        /// with the <em>type=&quot;&lt;<paramref name="typeAttribute"/>&gt;&quot;</em> attribute will be used to provide content for the description.</para>
        /// </param>
        /// <param name="reportWarning">Used to log any warnings.</param>
        /// <returns>A description element derived from the XML doc comment, or null if a description could not be obtained.</returns>
        private static XElement GetMamlDescriptionElementFromXmlDocComment(XElement commentsElement, string typeAttribute, Action<string> reportWarning)
        {
            if (commentsElement != null)
            {
                // Examine the XML doc comment first for an embedded <maml:description> element.
                var mamlDescriptionElement = commentsElement.XPathSelectElement($".//maml:description[@type='{typeAttribute}']", Resolver);
                if (mamlDescriptionElement != null)
                {
                    mamlDescriptionElement = new XElement(mamlDescriptionElement); // Deep clone the element, as we're about to modify it.
                    mamlDescriptionElement.RemoveAttributes(); // Intended to remove the xmlns:maml namespace declaration and id attribute. Assumes there aren't any further attributes.
                    return mamlDescriptionElement;
                }

                // Next try <para type="typeAttribute"> elements.
                var paraElements = commentsElement.XPathSelectElements($".//para[@type='{typeAttribute}']").ToList();
                if (paraElements.Any())
                {
                    var descriptionElement = new XElement(MamlNs + "description");
                    paraElements.ForEach(para => descriptionElement.Add(new XElement(MamlNs + "para", Tidy(para.Value))));
                    return descriptionElement;
                }
            }

            // We've failed to provide a description from the XML doc commment.
            reportWarning($"No {typeAttribute} comment found.");
            return null;
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

        private static string TidyCode(string value)
        {
            // Split the value into separate lines, and eliminate leading and trailing empty lines.
            IEnumerable<string> lines = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                             .SkipWhile(string.IsNullOrWhiteSpace)
                             .Reverse()
                             .SkipWhile(string.IsNullOrWhiteSpace)
                             .Reverse()
                             .ToList();

            // If all of the non-empty lines start with leading whitespace, remove it. (i.e. dedent the code).
            var pattern = new Regex(@"^\s*");
            var nonEmptyLines = lines.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (nonEmptyLines.Any())
            {
                var shortestPrefixLength = nonEmptyLines.Min(s => pattern.Match(s).Value.Length);
                if (shortestPrefixLength > 0)
                {
                    lines = lines.Select(s => s.Length <= shortestPrefixLength ? "" : s.Substring(shortestPrefixLength));
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string GetSimpleTypeName(Type type)
        {
            if (type.IsArray)
            {
                return GetSimpleTypeName(type.GetElementType()) + "[]";
            }

            if (PredefinedSimpleTypeNames.TryGetValue(type, out string result))
            {
                return result;
            }
            return type.Name;
        }

        private static readonly IDictionary<Type, string> PredefinedSimpleTypeNames =
            new Dictionary<Type, string>
            {
                {typeof(object), "object"},
                {typeof(string), "string"},
                {typeof(bool), "bool"},
                {typeof(byte), "byte"},
                {typeof(char), "char"},
                {typeof(short), "short"},
                {typeof(ushort), "ushort"},
                {typeof(int), "int"},
                {typeof(uint), "uint"},
                {typeof(long), "long"},
                {typeof(ulong), "ulong"},
                {typeof(float), "float"},
                {typeof(double), "double"},
            };
    }
}
