// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections.Generic;
using System.Linq;
using System.Web;
using NanoByte.Common;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.Utils
{
    /// <summary>
    /// Exports all <see cref="CliCommand"/> help texts as HTML.
    /// </summary>
    public class HtmlHelpExporter : HelpExporterBase
    {
        protected override string CommandListHeader() => $@"<table>
<tr>
  <th>{Resources.Command}</th>
  <th>{Resources.Description}</th>
</tr>";

        protected override string CommandListEntry(string name, string description) => $@"<tr>
  <td><a href='#{name?.Replace(" ", "_")}'><nobr><code>{name}</code></nobr></a></td>
  <td>{HtmlEncode(description)}</td>
</tr>";

        protected override string CommandListFooter() => @"</table>";

        protected override string CommandDetails(string name, string description, string usage) => $@"<a name='{name?.Replace(" ", "_")}'></a><h1>{name}</h1>
<p>{HtmlEncode(description)}</p>
<p><b>{Resources.Usage}</b> <code>0install {name} {usage}</code></p>";

        protected override string OptionListHeader() => $@"<table>
<tr>
  <th>Option</th>
  <th>{Resources.Description}</th>
</tr>";

        protected override string OptionListEntry(IEnumerable<string> prototypes, string description) => $@"<tr>
  <td>{StringUtils.Join("<br/>", prototypes.Select(x => $"<nobr><code>{HtmlEncode(x)}</code></nobr>"))}</td>
  <td>{HtmlEncode(description)}</td>
</tr>";

        protected override string OptionListFooter() => @"</table>";

        private static string HtmlEncode(string value)
            => HttpUtility.HtmlEncode(value)
                          .Replace("\n", "<br/>")
                          .Replace("{", "<code>")
                          .Replace("}", "</code>");
    }
}
