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
    /// Fluent-style builder for a process execution environment.
    /// </summary>
    public interface IEnvironmentBuilder
    {
        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers. May contain command-line arguments. Whitespaces must be escaped!
        /// </summary>
        /// <returns>The execution environment. Reference to self for fluent API use.</returns>
        IEnvironmentBuilder AddWrapper([CanBeNull] string wrapper);

        /// <summary>
        /// Appends user specified <paramref name="arguments"/> to the command-line.
        /// </summary>
        /// <returns>The execution environment. Reference to self for fluent API use.</returns>
        IEnvironmentBuilder AddArguments([NotNull, ItemNotNull] params string[] arguments);

        /// <summary>
        /// Builds a <see cref="ProcessStartInfo"/> for starting the program.
        /// </summary>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to start the new <see cref="Process"/>.</returns>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        [NotNull]
        ProcessStartInfo ToStartInfo();

        /// <summary>
        /// Starts the program.
        /// </summary>
        /// <returns>The newly created <see cref="Process"/>.</returns>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/> or the main executable could not be launched.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        [NotNull]
        Process Start();
    }
}