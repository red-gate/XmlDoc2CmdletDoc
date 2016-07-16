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
    public class Parameter
    {
        private readonly Type _cmdletType;
        private readonly IEnumerable<ParameterAttribute> _attributes;

        /// <summary>
        /// The <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> that defines the property.
        /// </summary>
        public readonly MemberInfo MemberInfo;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="cmdletType">The type of the cmdlet the parameter belongs to.</param>
        /// <param name="memberInfo">The parameter member of the cmdlet. May represent either a field or property.</param>
        public Parameter(Type cmdletType, MemberInfo memberInfo)
        {
            _cmdletType = cmdletType;
            MemberInfo = memberInfo;
            _attributes = memberInfo.GetCustomAttributes<ParameterAttribute>();
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get { return MemberInfo.Name; } }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public Type ParameterType
        {
            get
            {
                switch (MemberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        var type = ((PropertyInfo) MemberInfo).PropertyType;
                        var innerType = Nullable.GetUnderlyingType(type);
                        return innerType ?? type;
                    case MemberTypes.Field:
                        return ((FieldInfo) MemberInfo).FieldType;
                    default:
                        throw new NotSupportedException("Unsupported type: " + MemberInfo);
                }
            }
        }

        /// <summary>
        /// The names of the parameter sets that the parameter belongs to.
        /// </summary>
        public IEnumerable<string> ParameterSetNames
        {
            get { return _attributes.Select(attr => attr.ParameterSetName); }
        }

        private IEnumerable<ParameterAttribute> GetAttributes(string parameterSetName)
        {
            return parameterSetName == ParameterAttribute.AllParameterSets
                       ? _attributes
                       : _attributes.Where(attr => attr.ParameterSetName == parameterSetName ||
                                                   attr.ParameterSetName == ParameterAttribute.AllParameterSets);
        }

        /// <summary>
        /// Indicates whether or not the parameter is mandatory.
        /// </summary>
        public bool IsRequired(string parameterSetName)
        {
            return GetAttributes(parameterSetName).Any(attr => attr.Mandatory);
        }

        /// <summary>
        /// Indicates whether or not the parameter supports globbing. Currently always returns false.
        /// </summary>
        public bool SupportsGlobbing(string parameterSetName)
        {
            return false; // TODO: How do we determine this correctly?
        }

        /// <summary>
        /// Indicates whether or not the parameter takes its value from the pipeline input.
        /// </summary>
        public bool IsPipeline(string parameterSetName)
        {
            return GetAttributes(parameterSetName).Any(attr => attr.ValueFromPipeline || attr.ValueFromPipelineByPropertyName);
        }

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
        /// The default value of the parameter. This is obtained by instantiating the cmdlet and accessing the parameter
        /// property or field to determine its initial value.
        /// </summary>
        public object GetDefaultValue(ReportWarning reportWarning)
        {
            var cmdlet = Activator.CreateInstance(_cmdletType);
            switch (MemberInfo.MemberType)
            {
                case MemberTypes.Property:
                    var propertyInfo = ((PropertyInfo) MemberInfo);
                    if (!propertyInfo.CanRead)
                    {
                        reportWarning(MemberInfo, "Parameter does not have a getter. Unable to determine its default value");
                        return null;
                    }
                    return propertyInfo.GetValue(cmdlet);
                case MemberTypes.Field:
                    return ((FieldInfo) MemberInfo).GetValue(cmdlet);
                default:
                    throw new NotSupportedException("Unsupported type: " + MemberInfo);
            }
        }

        /// <summary>
        /// The list of enumerated value names. Returns an empty sequence if there are no enumerated values
        /// (normally because the parameter type is not an Enum type).
        /// </summary>
        public IEnumerable<string> EnumValues
        {
            get
            {
                if (MemberInfo.MemberType == MemberTypes.Property)
                {
                    var type = ParameterType;
                    if (type.IsEnum)
                    {
                        return type
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
                var aliasAttribute = (AliasAttribute)MemberInfo
                    .GetCustomAttributes(typeof(AliasAttribute), true)
                    .FirstOrDefault();
                return aliasAttribute == null ? new List<string>() : aliasAttribute.AliasNames;
            }
        }
    }
}