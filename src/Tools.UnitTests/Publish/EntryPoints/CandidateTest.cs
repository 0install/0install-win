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
using System.Reflection;
using Common;
using Common.Utils;
using NUnit.Framework;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Base class for test fixtures that test <see cref="Candidate"/>s.
    /// </summary>
    public abstract class CandidateTest : TemporayDirectoryTest
    {
        /// <summary>
        /// Ensures <see cref="Candidate.Analyze"/> correctly identifies a reference file.
        /// </summary>
        /// <param name="reference">Baseline to compare a new <see cref="Candidate"/> against. Also used to determine the reference file to <see cref="Deploy"/>.</param>
        /// <param name="executable">Set to <see langword="true"/> to mark the file as Unix executable.</param>
        protected void TestAnalyze<T>(T reference, bool executable = false) where T : Candidate, new()
        {
            var candidate = new T {BaseDirectory = Directory};
            Assert.IsTrue(candidate.Analyze(Deploy(reference, executable)));
            Assert.AreEqual(reference, candidate);
        }

        /// <summary>
        /// Deploys a reference file for a <see cref="Candidate"/> from an internal resource.
        /// </summary>
        /// <param name="reference">Uses <see cref="Candidate.RelativePath"/> as the resource name.</param>
        /// <param name="xbit">Set to <see langword="true"/> to mark the file as Unix executable.</param>
        /// <returns></returns>
        protected FileInfo Deploy(Candidate reference, bool xbit)
        {
            var file = new FileInfo(Path.Combine(Directory.FullName, reference.RelativePath));
            using (var stream = file.Create())
                GetResource(reference.RelativePath).CopyTo(stream);

            if (xbit)
            {
                if (UnixUtils.IsUnix) FileUtils.SetExecutable(file.FullName, true);
                else FlagUtils.SetExternalFlag(Path.Combine(Directory.FullName, ".xbit"), reference.RelativePath);
            }

            return file;
        }

        #region Resources
        private static readonly Assembly _testDataAssembly = Assembly.GetAssembly(typeof(CandidateTest));

        private static Stream GetResource(string name)
        {
            return _testDataAssembly.GetManifestResourceStream(typeof(CandidateTest), name);
        }
        #endregion
    }
}
