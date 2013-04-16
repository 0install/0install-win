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
using System.Collections.Generic;
using System.Text;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Formats <see cref="Selections"/> for string output.
    /// </summary>
    public static class SelectionsFormatter
    {
        /// <summary>
        /// Generates a human-readable representation of the implementation selection hierachy.
        /// </summary>
        /// <param name="selections">The selections to be displayed.</param>
        /// <param name="store">A store to search for implementation storage locations.</param>
        public static string GetHumanReadable(this Selections selections, IStore store)
        {
            var builder = new StringBuilder();
            PrintNode(selections, builder, new C5.HashSet<string>(), store, "", selections.InterfaceID);
            return (builder.Length == 0 ? "" : builder.ToString(0, builder.Length - Environment.NewLine.Length)); // Remove trailing line-break
        }

        /// <summary>
        /// Helper method for <see cref="GetHumanReadable"/> that recursivley writes information about <see cref="ImplementationSelection"/>s to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="selections">The selections to be displayed.</param>
        /// <param name="builder">The string builder to write the output to.</param>
        /// <param name="handled">A list of interface IDs that have already been handled; used to prevent infinite recursion.</param>
        /// <param name="store">A store to search for implementation storage locations.</param>
        /// <param name="indent">An indention prefix for the current recursion level (to create a visual hierachy).</param>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        private static void PrintNode(Selections selections, StringBuilder builder, C5.HashSet<string> handled, IStore store, string indent, string interfaceID)
        {
            // Prevent infinite recursion
            if (handled.Contains(interfaceID)) return;
            handled.Add(interfaceID);

            builder.AppendLine(indent + "- URI: " + interfaceID);
            try
            {
                var implementation = selections[interfaceID];
                builder.AppendLine(indent + "  Version: " + implementation.Version);
                builder.AppendLine(indent + "  Path: " + (implementation.LocalPath ?? implementation.GetPath(store) ?? Resources.NotCached));

                indent += "    ";

                // Recurse into regular dependencies
                foreach (var dependency in implementation.Dependencies)
                    PrintNode(selections, builder, handled, store, indent, dependency.Interface);

                if (!implementation.Commands.IsEmpty)
                {
                    var command = implementation.Commands.First;

                    // Recurse into command dependencies
                    foreach (var dependency in command.Dependencies)
                        PrintNode(selections, builder, handled, store, indent, dependency.Interface);

                    // Recurse into runner dependency
                    if (command.Runner != null)
                        PrintNode(selections, builder, handled, store, indent, command.Runner.Interface);
                }
            }
            catch (KeyNotFoundException)
            {
                builder.AppendLine(indent + "  " + Resources.NoSelectedVersion);
            }
        }

        /// <summary>
        /// Tries to locate an implementation in an <see cref="IStore"/>.
        /// </summary>
        /// <param name="implementation">The implementation to locate.</param>
        /// <param name="store">The store to search for the implementation storage location.</param>
        /// <returns>A fully qualified path to the directory containing the implementation, a native package name prefixed with <code>package:</code> or <see langword="null"/> if the implementation is not cached yet.</returns>
        private static string GetPath(this ImplementationSelection implementation, IStore store)
        {
            return implementation.ID.StartsWith("package:")
                ? "(" + implementation.ID + ")"
                : store.GetPath(implementation.ManifestDigest);
        }
    }
}