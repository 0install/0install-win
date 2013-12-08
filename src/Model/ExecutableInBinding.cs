using System.ComponentModel;
using System.Xml.Serialization;
using ZeroInstall.Model.Design;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Make a chosen <see cref="Implementation"/> available as an executable at runtime.
    /// </summary>
    [XmlType("executable-in-binding", Namespace = Feed.XmlNamespace)]
    public abstract class ExecutableInBinding : Binding
    {
        /// <summary>
        /// The name of the <see cref="Command"/> in the <see cref="Implementation"/> to launch; leave <see langword="null"/> for <see cref="Model.Command.NameRun"/>.
        /// </summary>
        [Description("The name of the command in the implementation to launch; leave empty for 'run'.")]
        [XmlAttribute("command"), DefaultValue("")]
        [TypeConverter(typeof(CommandNameConverter))]
        public string Command { get; set; }
    }
}