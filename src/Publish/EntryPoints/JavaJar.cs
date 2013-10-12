﻿/*
 * Copyright 2010-2013 Bastian Eicher
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

using System;
using System.IO;
using Common.Utils;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A Java JAR archive.
    /// </summary>
    public sealed class JavaJar : Java
    {
        /// <inheritdoc/>
        internal override bool Analyze(FileInfo file)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            if (!base.Analyze(file)) return false;
            if (!StringUtils.EqualsIgnoreCase(file.Extension, ".jar")) return false;

            // TODO: Parse JAR metadata
            return true;
        }

        /// <inheritdoc/>
        public override Command Command
        {
            get
            {
                if (HasDependencies)
                {
                    return new Command
                    {
                        Name = Command.NameRun,
                        Bindings = {new EnvironmentBinding {Name = "CLASSPATH", Insert = RelativePath}},
                        Runner = new Runner {Interface = "http://0install.de/feeds/jar-launcher.xml", Versions = RuntimeVersion}
                    };
                }
                else
                {
                    return new Command
                    {
                        Name = Command.NameRun,
                        Arguments = {"-jar"},
                        Path = RelativePath,
                        Runner = new Runner {Interface = "http://repo.roscidus.com/java/openjdk-jre", Versions = RuntimeVersion}
                    };
                }
            }
        }
    }
}
