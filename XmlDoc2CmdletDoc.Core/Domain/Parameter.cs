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
        /// Indicates whether or not the parameter is mandatory.
        /// </summary>
        public bool IsRequired { get { return _attributes.Any(attr => attr.Mandatory); } }

        /// <summary>
        /// Indicates whether or not the parameter supports globbing. Currently always returns false.
        /// </summary>
        public bool SupportsGlobbing { get { return false; } } // TODO: How do we determine this correctly?

        /// <summary>
        /// Indicates whether or not the parameter takes its value from the pipeline input.
        /// </summary>
        public bool IsPipeline { get { return _attributes.Any(attr => attr.ValueFromPipeline || attr.ValueFromPipelineByPropertyName); } }

        /// <summary>
        /// The position of the parameter, or <em>null</em> if no position is defined.
        /// </summary>
        public int? Position
        {
            get
            {
                var position = _attributes.First().Position;
                return position == int.MinValue ? (int?) null : position;
            }
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