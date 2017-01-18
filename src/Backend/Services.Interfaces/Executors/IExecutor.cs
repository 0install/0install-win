using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Executors
{
    /// <summary>
    /// Executes a <see cref="Selections"/> document as a program using dependency injection.
    /// </summary>
    public interface IExecutor
    {
        /// <summary>
        /// Starts a program as described by a <see cref="Selections"/> document.
        /// </summary>
        /// <param name="selections">The set of <see cref="Implementation"/>s be injected into the execution environment.</param>
        /// <returns>The newly created <see cref="Process"/>; <c>null</c> if no external process was started.</returns>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/> or the main executable could not be launched.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        [CanBeNull]
        Process Start([NotNull] Selections selections);

        /// <summary>
        /// Starts building an execution environment for a <see cref="Selections"/> document.
        /// </summary>
        /// <param name="selections">The set of <see cref="Implementation"/>s be injected into the execution environment.</param>
        /// <param name="overrideMain">An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="Element.Main"/>. May not contain command-line arguments! Whitespaces do not need to be escaped.</param>
        /// <returns>A fluent-style builder for a process execution environment.</returns>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        [NotNull]
        IEnvironmentBuilder Inject([NotNull] Selections selections, string overrideMain = null);
    }
}
