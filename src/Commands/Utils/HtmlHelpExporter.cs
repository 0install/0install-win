/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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

        private static string HtmlEncode(string value) => HttpUtility.HtmlEncode(value)
            .Replace("\n", "<br/>")
            .Replace("{", "<code>")
            .Replace("}", "</code>");
    }
}
