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
    /// A binary inside a MacOS X application bundle.
    /// </summary>
    public sealed class MacOSApp : PosixExecutable
    {
        /// <inheritdoc/>
        internal override bool Analyze(FileInfo file)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            if (!base.Analyze(file)) return false;
            if (!RelativePath.GetLeftPartAtLastOccurrence('/').EndsWith(".app/Contents/MacOS")) return false;

            // TODO: Parse MacOS plist
            Name = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            Architecture = new Architecture(OS.MacOSX, Cpu.All);
            return true;
        }
    }
}
