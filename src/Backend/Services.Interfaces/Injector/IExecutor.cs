using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Injector
{
    /// <summary>
    /// Executes a set of <see cref="Selections"/> as a program using dependency injection.
    /// </summary>
    public interface IExecutor
    {
        /// <summary>
        /// The specific <see cref="Store.Model.Implementation"/>s chosen for the <see cref="Dependency"/>s.
        /// </summary>
        Selections Selections { get; }

        /// <summary>
        /// An alternative executable to to run from the main <see cref="Store.Model.Implementation"/> instead of <see cref="Element.Main"/>. May not contain command-line arguments! Whitespaces do not need to be escaped.
        /// </summary>
        string Main { get; set; }

        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers. May contain command-line arguments. Whitespaces must be escaped!
        /// </summary>
        string Wrapper { get; set; }

        /// <summary>
        /// Starts the program as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="selections">The specific <see cref="ImplementationSelection"/>s chosen by the solver.</param>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The newly created <see cref="Process"/>; <see langword="null"/> if no external process was started.</returns>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Store.Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">The main executable could not be launched or if a problem occurred while creating a hard link.</exception>
        [CanBeNull]
        Process Start([NotNull] Selections selections, [NotNull] params string[] arguments);

        /// <summary>
        /// Locates an implementation on the disk (usually in an <see cref="IStore"/>).
        /// </summary>
        /// <param name="implementation">The <see cref="ImplementationBase"/> to be located.</param>
        /// <returns>A fully qualified path pointing to the implementation's location on the local disk.</returns>
        /// <exception cref="ImplementationNotFoundException">The <paramref name="implementation"/> is not cached yet.</exception>
        [NotNull]
        string GetImplementationPath([NotNull] ImplementationSelection implementation);
    }
}
