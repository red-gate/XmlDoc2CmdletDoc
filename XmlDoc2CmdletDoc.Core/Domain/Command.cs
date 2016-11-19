using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace XmlDoc2CmdletDoc.Core.Domain
{
    /// <summary>
    /// Represents a single cmdlet.
    /// </summary>
    public class Command
    {
        private readonly CmdletAttribute _attribute;

        /// <summary>
        /// Creates a new instance based on the specified cmdlet type.
        /// </summary>
        /// <param name="cmdletType">The type of the cmdlet. Must be a sub-class of <see cref="Cmdlet"/>
        /// and have a <see cref="CmdletAttribute"/>.</param>
        public Command(Type cmdletType)
        {
            if (cmdletType == null) throw new ArgumentNullException(nameof(cmdletType));
            CmdletType = cmdletType;
            _attribute = CmdletType.GetCustomAttribute<CmdletAttribute>();
            if (_attribute == null) throw new ArgumentException("Missing CmdletAttribute", nameof(cmdletType));
        }

        /// <summary>
        /// The type of the cmdlet for this command.
        /// </summary>
        public readonly Type CmdletType;

        /// <summary>
        /// The cmdlet verb.
        /// </summary>
        public string Verb { get { return _attribute.VerbName; } }

        /// <summary>
        /// The cmdlet noun.
        /// </summary>
        public string Noun { get { return _attribute.NounName; } }

        /// <summary>
        /// The cmdlet name, of the form verb-noun.
        /// </summary>
        public string Name { get { return Verb + "-" + Noun; } }

        /// <summary>
        /// The output types declared by the command.
        /// </summary>
        public IEnumerable<Type> OutputTypes
        {
            get
            {
                return CmdletType.GetCustomAttributes<OutputTypeAttribute>()
                                 .SelectMany(attr => attr.Type)
                                 .Select(pstype => pstype.Type)
                                 .Distinct()
                                 .OrderBy(type => type.FullName);
            }
        }

        /// <summary>
        /// The parameters belonging to the command.
        /// </summary>
        public IEnumerable<Parameter> Parameters
        {
            get
            {
                var parameters = CmdletType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                                           .Where(member => member.GetCustomAttributes<ParameterAttribute>().Any())
                                           .Select(member => new Parameter(CmdletType, member));
                if (typeof(IDynamicParameters).IsAssignableFrom(CmdletType))
                {
                    foreach (var nestedType in CmdletType.GetNestedTypes(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        parameters = parameters.Concat(nestedType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                                                                 .Where(member => member.GetCustomAttributes<ParameterAttribute>().Any())
                                                                 .Select(member => new Parameter(nestedType, member)));
                    }
                }
                return parameters.ToList();
            }
        }

        /// <summary>
        /// The command's parameters that belong to the specified parameter set.
        /// </summary>
        /// <param name="parameterSetName">The name of the parameter set.</param>
        /// <returns>
        /// The command's parameters that belong to the specified parameter set.
        /// </returns>
        public IEnumerable<Parameter> GetParameters(string parameterSetName)
        {
            return parameterSetName == ParameterAttribute.AllParameterSets
                       ? Parameters
                       : Parameters.Where(p => p.ParameterSetNames.Contains(parameterSetName) ||
                                               p.ParameterSetNames.Contains(ParameterAttribute.AllParameterSets));
        }

        /// <summary>
        /// The names of the parameter sets that the parameters belongs to.
        /// </summary>
        public IEnumerable<string> ParameterSetNames { get { return Parameters.SelectMany(p => p.ParameterSetNames).Distinct(); } }
    }
}
