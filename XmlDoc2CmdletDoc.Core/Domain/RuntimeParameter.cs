using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace XmlDoc2CmdletDoc.Core.Domain
{
    /// <summary>
    /// Represents a single parameter of a cmdlet that is defined at runtime.
    /// </summary>
    public class RuntimeParameter : Parameter
    {
        /// <summary>
        /// The <see cref="RuntimeDefinedParameter"/> that defines the property.
        /// </summary>
        public readonly RuntimeDefinedParameter RuntimeDefinedParameter;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="cmdletType">The type of the cmdlet the parameter belongs to.</param>
        /// <param name="runtimeDefinedParameter">The dynamic runtime parameter member of the cmdlet.</param>
        public RuntimeParameter(Type cmdletType, RuntimeDefinedParameter runtimeDefinedParameter) : base(cmdletType, runtimeDefinedParameter?.Attributes.OfType<ParameterAttribute>())
        {
            RuntimeDefinedParameter = runtimeDefinedParameter ?? throw new ArgumentNullException(nameof(runtimeDefinedParameter));
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public override string Name => RuntimeDefinedParameter.Name;

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public override Type ParameterType => RuntimeDefinedParameter.ParameterType;

        /// <summary>
        /// The type of this parameter's member - method, constructor, property, and so on.
        /// </summary>
        public override MemberTypes MemberType => MemberTypes.Property; //RuntimeDefinedParameters are always defined as a Property

        /// <summary>
        /// The default value of the parameter. Runtime parameters do not support specifying default values.
        /// </summary>
        public override object GetDefaultValue(ReportWarning reportWarning)
        {
            //RuntimeDefinedParameter objects cannot have a default value.
            return null;
        }

        /// <summary>
        /// Retrieves custom attributes defined on the parameter.
        /// </summary>
        /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
        public override object[] GetCustomAttributes<T>() =>
            RuntimeDefinedParameter.Attributes.OfType<T>().Cast<object>().ToArray();
    }
}