/*
 * Copyright 2010-2015 Bastian Eicher
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

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains extension methods for <see cref="Architecture"/>, <see cref="OS"/> and <see cref="Cpu"/>.
    /// </summary>
    public static class ArchitectureExtensions
    {
        /// <summary>
        /// Determines whether an <paramref name="implementation"/> architecture (the current instance) can run on a <paramref name="system"/> architecture.
        /// </summary>
        public static bool IsCompatible(this Architecture implementation, Architecture system)
        {
            return implementation.OS.IsCompatible(system.OS) && implementation.Cpu.IsCompatible(system.Cpu);
        }

        /// <summary>
        /// Determines whether an <paramref name="implementation"/> OS is compatible with a <paramref name="system"/> OS.
        /// </summary>
        public static bool IsCompatible(this OS implementation, OS system)
        {
            if (implementation == OS.Unknown || system == OS.Unknown) return false;

            // Exact OS match or platform-neutral implementation
            if (implementation == system || implementation == OS.All || system == OS.All) return true;

            // Compatible supersets
            if (implementation == OS.Windows && system == OS.Cygwin) return true;
            if (implementation == OS.Darwin && system == OS.MacOSX) return true;
            if (implementation == OS.Posix && system <= OS.Posix) return true;

            // No match
            return false;
        }

        /// <summary>
        /// Determines whether an <paramref name="implementation"/> CPU is compatible with a <paramref name="system"/> CPU.
        /// </summary>
        public static bool IsCompatible(this Cpu implementation, Cpu system)
        {
            if (implementation == Cpu.Unknown || system == Cpu.Unknown) return false;

            // Exact CPU match or platform-neutral implementation
            if (implementation == system || implementation == Cpu.All || system == Cpu.All) return true;

            // Compatible supersets
            if (implementation >= Cpu.I386 && implementation <= Cpu.I686 && system >= implementation && system <= Cpu.I686) return true;

            // No match
            return false;
        }
    }
}
