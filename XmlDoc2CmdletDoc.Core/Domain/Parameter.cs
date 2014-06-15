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
        private readonly MemberInfo _memberInfo;
        private readonly IEnumerable<ParameterAttribute> _attributes;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="cmdletType">The type of the cmdlet the parameter belongs to.</param>
        /// <param name="memberInfo">The parameter member of the cmdlet. May represent either a field or property.</param>
        public Parameter(Type cmdletType, MemberInfo memberInfo)
        {
            _cmdletType = cmdletType;
            _memberInfo = memberInfo;
            _attributes = memberInfo.GetCustomAttributes<ParameterAttribute>();
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get { return _memberInfo.Name; } }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public Type ParameterType
        {
            get
            {
                switch (_memberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        return ((PropertyInfo) _memberInfo).PropertyType;
                    case MemberTypes.Field:
                        return ((FieldInfo) _memberInfo).FieldType;
                    default:
                        throw new NotSupportedException("Unsupported type: " + _memberInfo);
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
        public object DefaultValue
        {
            get
            {
                var cmdlet = Activator.CreateInstance(_cmdletType);
                switch (_memberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        return ((PropertyInfo) _memberInfo).GetValue(cmdlet);
                    case MemberTypes.Field:
                        return ((FieldInfo) _memberInfo).GetValue(cmdlet);
                    default:
                        throw new NotSupportedException("Unsupported type: " + _memberInfo);
                }
            }
        }
    }
}