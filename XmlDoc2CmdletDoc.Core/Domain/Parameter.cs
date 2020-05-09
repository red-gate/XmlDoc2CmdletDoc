using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace XmlDoc2CmdletDoc.Core.Domain
{
    /// <summary>
    /// Represents a single parameter of a cmdlet.
    /// </summary>
    public abstract class Parameter
    {
        /// <summary>
        /// The type of the cmdlet this parameter is defined on.
        /// </summary>
        protected readonly Type _cmdletType;
        private readonly IEnumerable<ParameterAttribute> _attributes;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="cmdletType">The type of the cmdlet the parameter belongs to.</param>
        /// <param name="attributes">The parameter attributes of the cmdlet.</param>
        public Parameter(Type cmdletType, IEnumerable<ParameterAttribute> attributes)
        {
            _cmdletType = cmdletType ?? throw new ArgumentNullException(nameof(cmdletType));
            _attributes = attributes;
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public abstract Type ParameterType { get; }

        /// <summary>
        /// The type of this parameter's member - method, constructor, property, and so on.
        /// </summary>
        public abstract MemberTypes MemberType { get; }

        /// <summary>
        /// Indicates whether or not the parameter supports globbing.
        /// </summary>
        public abstract bool SupportsGlobbing { get; }

        /// <summary>
        /// The names of the parameter sets that the parameter belongs to.
        /// </summary>
        public IEnumerable<string> ParameterSetNames => _attributes.Select(attr => attr.ParameterSetName);

        private IEnumerable<ParameterAttribute> GetAttributes(string parameterSetName) =>
            parameterSetName == ParameterAttribute.AllParameterSets
                ? _attributes
                : _attributes.Where(attr => attr.ParameterSetName == parameterSetName ||
                                            attr.ParameterSetName == ParameterAttribute.AllParameterSets);

        /// <summary>
        /// Indicates whether or not the parameter is mandatory.
        /// </summary>
        public bool IsRequired(string parameterSetName) => GetAttributes(parameterSetName).Any(attr => attr.Mandatory);

        /// <summary>
        /// Indicates whether or not the parameter takes its value from the pipeline input.
        /// </summary>
        public bool IsPipeline(string parameterSetName) =>
            GetAttributes(parameterSetName).Any(attr => attr.ValueFromPipeline || attr.ValueFromPipelineByPropertyName);

        /// <summary>
        /// Indicates whether or not the parameter takes its value from the pipeline input.
        /// </summary>
        public string GetIsPipelineAttribute(string parameterSetName)
        {
            var attributes = GetAttributes(parameterSetName).ToList();
            bool byValue = attributes.Any(attr => attr.ValueFromPipeline);
            bool byParameterName = attributes.Any(attr => attr.ValueFromPipelineByPropertyName);
            return byValue
                       ? byParameterName
                             ? "true (ByValue, ByPropertyName)"
                             : "true (ByValue)"
                       : byParameterName
                           ? "true (ByPropertyName)"
                           : "false";
        }

        /// <summary>
        /// The position of the parameter, or <em>null</em> if no position is defined.
        /// </summary>
        public string GetPosition(string parameterSetName)
        {
            var attribute = GetAttributes(parameterSetName).FirstOrDefault();
            if (attribute == null) return null;
            return attribute.Position == int.MinValue ? "named" : Convert.ToString(attribute.Position);
        }

        /// <summary>
        /// The default value of the parameter. This may be obtained by instantiating the cmdlet and accessing the parameter
        /// property or field to determine its initial value.
        /// </summary>
        public abstract object GetDefaultValue(ReportWarning reportWarning);

        /// <summary>
        /// The list of enumerated value names. Returns an empty sequence if there are no enumerated values
        /// (normally because the parameter type is not an Enum type).
        /// </summary>
        public IEnumerable<string> EnumValues
        {
            get
            {
                if (MemberType == MemberTypes.Property)
                {
                    Type enumType = null;

                    if (ParameterType.IsEnum)
                        enumType = ParameterType;
                    else
                    {
                        foreach (var @interface in ParameterType.GetInterfaces())
                        {
                            if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            {
                                var genericArgument = @interface.GetGenericArguments()[0];

                                if (genericArgument.IsEnum)
                                    enumType = genericArgument;

                                break;
                            }
                        }
                    }

                    if (enumType != null)
                    {
                        return enumType
                               .GetFields(BindingFlags.Public | BindingFlags.Static)
                               .Select(field => field.Name);
                    }
                }

                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// The list of parameter aliases.
        /// </summary>
        public IEnumerable<string> Aliases
        {
            get
            {
                var aliasAttribute = (AliasAttribute)GetCustomAttributes<AliasAttribute>()
                                                     .FirstOrDefault();
                return aliasAttribute?.AliasNames ?? new List<string>();
            }
        }

        /// <summary>
        /// Retrieves custom attributes defined on the parameter.
        /// </summary>
        /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
        public abstract object[] GetCustomAttributes<T>() where T : Attribute;
    }
}