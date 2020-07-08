using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace XmlDoc2CmdletDoc.Core.Domain
{
    /// <summary>
    /// Represents a single parameter of a cmdlet that is identified via reflection.
    /// </summary>
    public class ReflectionParameter : Parameter
    {
        /// <summary>
        /// The <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> that defines the property.
        /// </summary>
        public readonly MemberInfo MemberInfo;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="cmdletType">The type of the cmdlet the parameter belongs to.</param>
        /// <param name="memberInfo">The parameter member of the cmdlet. May represent either a field or property.</param>
        public ReflectionParameter(Type cmdletType, MemberInfo memberInfo) : base(cmdletType, memberInfo?.GetCustomAttributes<ParameterAttribute>())
        {
            MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public override string Name => MemberInfo.Name;

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public override Type ParameterType
        {
            get
            {
                Type GetType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

                switch (MemberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        return GetType(((PropertyInfo)MemberInfo).PropertyType);
                    case MemberTypes.Field:
                        return GetType(((FieldInfo)MemberInfo).FieldType);
                    default:
                        throw new NotSupportedException("Unsupported type: " + MemberInfo);
                }
            }
        }

        /// <summary>
        /// The type of this parameter's member - method, constructor, property, and so on.
        /// </summary>
        public override MemberTypes MemberType => MemberInfo.MemberType;

        /// <inheritdoc />
        public override bool SupportsGlobbing => MemberInfo.GetCustomAttributes<SupportsWildcardsAttribute>(true).Any();

        /// <summary>
        /// The default value of the parameter. This is obtained by instantiating the cmdlet and accessing the parameter
        /// property or field to determine its initial value.
        /// </summary>
        public override object GetDefaultValue(ReportWarning reportWarning)
        {
            var cmdlet = Activator.CreateInstance(_cmdletType);
            switch (MemberInfo.MemberType)
            {
                case MemberTypes.Property:
                    var propertyInfo = ((PropertyInfo)MemberInfo);
                    if (!propertyInfo.CanRead)
                    {
                        reportWarning(MemberInfo, "Parameter does not have a getter. Unable to determine its default value");
                        return null;
                    }
                    return propertyInfo.GetValue(cmdlet);
                case MemberTypes.Field:
                    return ((FieldInfo)MemberInfo).GetValue(cmdlet);
                default:
                    throw new NotSupportedException("Unsupported type: " + MemberInfo);
            }
        }

        /// <summary>
        /// Retrieves custom attributes defined on the parameter.
        /// </summary>
        /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
        public override object[] GetCustomAttributes<T>() => MemberInfo.GetCustomAttributes(typeof(T), true);
    }
}