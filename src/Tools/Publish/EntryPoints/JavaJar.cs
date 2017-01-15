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

using System.IO;
using NanoByte.Common;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A Java JAR archive.
    /// </summary>
    public sealed class JavaJar : Java
    {
        /// <inheritdoc/>
        internal override bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            if (!base.Analyze(baseDirectory, file)) return false;
            if (!StringUtils.EqualsIgnoreCase(file.Extension, @".jar")) return false;

            // TODO: Parse JAR metadata
            Name = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            GuiOnly = false;
            return true;
        }

        /// <inheritdoc/>
        public override Command CreateCommand()
        {
            return ExternalDependencies
                ? new Command
                {
                    Name = CommandName,
                    Bindings = {new EnvironmentBinding {Name = "CLASSPATH", Insert = RelativePath}},
                    Runner = new Runner
                    {
                        InterfaceUri = new FeedUri("http://repo.roscidus.com/java/jar-launcher"),
                        Command = NeedsTerminal ? Command.NameRun : Command.NameRunGui,
                        Versions = (VersionRange)MinimumRuntimeVersion
                    }
                }
                : new Command
                {
                    Name = CommandName,
                    Path = RelativePath,
                    Runner = new Runner
                    {
                        InterfaceUri = new FeedUri("http://repo.roscidus.com/java/openjdk-jre"),
                        Command = NeedsTerminal ? Command.NameRun : Command.NameRunGui,
                        Arguments = {@"-jar"},
                        Versions = (VersionRange)MinimumRuntimeVersion
                    }
                };
        }
    }
}
