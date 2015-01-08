﻿/*
 * Copyright 2010-2014 Bastian Eicher
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

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A script written in Perl.
    /// </summary>
    public sealed class PerlScript : InterpretedScript
    {
        /// <inheritdoc/>
        internal override bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            if (!base.Analyze(baseDirectory, file)) return false;
            return
                StringUtils.EqualsIgnoreCase(file.Extension, @".pl") ||
                HasShebang(file, "perl");
        }

        /// <inheritdoc/>
        protected override FeedUri InterpreterInterface { get { return new FeedUri("http://0install.de/feeds/Perl.xml"); } }
    }
}
