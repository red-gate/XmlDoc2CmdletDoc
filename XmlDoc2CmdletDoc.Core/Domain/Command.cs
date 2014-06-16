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
        /// <param name="cmdletCmdletType">The type of the cmdlet. Must be a sub-class of <see cref="Cmdlet"/>
        /// and have a <see cref="CmdletAttribute"/>.</param>
        public Command(Type cmdletCmdletType)
        {
            if (cmdletCmdletType == null) throw new ArgumentNullException("cmdletCmdletType");
            CmdletType = cmdletCmdletType;
            _attribute = CmdletType.GetCustomAttribute<CmdletAttribute>();
            if (_attribute == null) throw new ArgumentException("Missing CmdletAttribute", "cmdletCmdletType");
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
                return CmdletType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                    .Where(member => member.GetCustomAttribute<ParameterAttribute>() != null)
                    .Select(member => new Parameter(CmdletType, member));
            }
        }

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
