using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NDesk.Options;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Publish.Cli
{
    public interface ICommand
    {
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="OptionException">The specified feed file paths were invalid.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="InvalidDataException">A feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">A feed file could not be found.</exception>
        /// <exception cref="IOException">A file could not be read or written or the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a feed file or the catalog file is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An existing digest does not match the newly calculated one.</exception>
        /// <exception cref="KeyNotFoundException">An OpenPGP key could not be found.</exception>
        /// <exception cref="NotSupportedException">A MIME type doesn't belong to a known and supported archive type.</exception>
        ExitCode Execute();
    }
}