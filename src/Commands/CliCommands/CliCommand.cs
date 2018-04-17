// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NanoByte.Common.Values;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Represents a command issued via the command-line. Not to be confused with <see cref="Command"/>!
    /// </summary>
    /// <remarks>Specific sub-classes of this class are used to handle a commands like "0install COMMAND [OPTIONS]".</remarks>
    public abstract class CliCommand : CommandBase
    {
        #region Metadata
        /// <summary>
        /// The name of this command as used in command-line arguments in lower-case.
        /// </summary>
        public virtual string Name
        {
            get
            {
                var field = GetType().GetField("Name", BindingFlags.Public | BindingFlags.Static);
                return field?.GetValue(null).ToString();
            }
        }

        /// <summary>
        /// A short description of what this command does.
        /// </summary>
        [NotNull]
        public abstract string Description { get; }

        /// <summary>
        /// The additional arguments to be displayed after the command name in the help text.
        /// </summary>
        [NotNull]
        public abstract string Usage { get; }

        /// <summary>
        /// The minimum number of <see cref="AdditionalArgs"/> allowed. Checked in <see cref="Parse"/>.
        /// </summary>
        protected virtual int AdditionalArgsMin => 0;

        /// <summary>
        /// The maximum number of <see cref="AdditionalArgs"/> allowed. Checked in <see cref="Parse"/>.
        /// </summary>
        protected virtual int AdditionalArgsMax => int.MaxValue;

        /// <summary>
        /// The help text describing the available command-line options and their effects.
        /// </summary>
        protected string HelpText
        {
            get
            {
                using (var buffer = new MemoryStream())
                {
                    var writer = new StreamWriter(buffer);
                    writer.WriteLine(Resources.Usage + " 0install " + Name + " " + Usage);
                    writer.WriteLine();
                    writer.WriteLine(Description);
                    if (Options.Count != 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine(Resources.Options);
                        Options.WriteOptionDescriptions(writer);
                    }
                    writer.Flush();

                    return buffer.ReadToString();
                }
            }
        }

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        protected internal readonly OptionSet Options = new OptionSet();
        #endregion

        #region State
        /// <summary>
        /// A callback object used when the the user needs to be asked questions or informed about download and IO tasks.
        /// </summary>
        // Type covariance: ServiceLocator -> FrontendCommand, ITaskHandler -> ICommandHandler
        [NotNull]
        public new ICommandHandler Handler { get; }

        /// <summary>Feeds to add, terms to search for, etc.</summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Using a List<T> for performance reasons")]
        [NotNull, ItemNotNull]
        protected readonly List<string> AdditionalArgs = new List<string>();

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        protected CliCommand([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Handler = handler;

            Options.Add("?|h|help", () => Resources.OptionHelp, _ =>
            {
                Handler.Output(Resources.CommandLineArguments, HelpText);
                throw new OperationCanceledException(); // Don't handle any of the other arguments
            });
            Options.Add("background", () => Resources.OptionBackground, _ => Handler.Background = true);
            Options.Add("batch", () => Resources.OptionBatch, _ =>
            {
                if (Handler.Verbosity >= Verbosity.Verbose) throw new OptionException(string.Format(Resources.ExclusiveOptions, "--batch", "--verbose"), "verbose");
                Handler.Verbosity = Verbosity.Batch;
            });
            Options.Add("v|verbose", () => Resources.OptionVerbose, _ =>
            {
                if (Handler.Verbosity == Verbosity.Batch) throw new OptionException(string.Format(Resources.ExclusiveOptions, "--batch", "--verbose"), "batch");
                Handler.Verbosity++;
            });
        }
        #endregion

        /// <summary>
        /// Parses command-line arguments and stores the result in the command.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <exception cref="OperationCanceledException">The user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException"><paramref name="args"/> contains unknown options.</exception>
        /// <exception cref="IOException">A problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">More privileges are required.</exception>
        /// <exception cref="UriFormatException">The URI or local path specified is invalid.</exception>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public virtual void Parse([NotNull, ItemNotNull] IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException(nameof(args));
            #endregion

            // Automatically show help for missing args
            if (AdditionalArgsMin > 0 && !args.Any()) args = new[] {"--help"};

            AdditionalArgs.AddRange(Options.Parse(args).Where(x => !string.IsNullOrEmpty(x)));

            if (AdditionalArgs.Count < AdditionalArgsMin) throw new OptionException(Resources.MissingArguments, null);
            if (AdditionalArgsMin == 1 && string.IsNullOrEmpty(AdditionalArgs[0])) throw new OptionException(Resources.MissingArguments, null);

            if (AdditionalArgs.Count > AdditionalArgsMax) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + AdditionalArgs.Skip(AdditionalArgsMax).JoinEscapeArguments(), null);
        }

        /// <summary>
        /// Executes the commands specified by the command-line arguments. Must call <see cref="Parse"/> first!
        /// </summary>
        /// <returns>The exit status code to end the process with.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="OptionException">The number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted or an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">An operation failed due to insufficient rights.</exception>
        /// <exception cref="InvalidDataException">A problem occurred while deserializing an XML file.</exception>
        /// <exception cref="SignatureException">The signature data could not be handled for some reason.</exception>
        /// <exception cref="FormatException">An URI, local path, version number, etc. is invalid.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="SolverException">The <see cref="ISolver"/> was unable to provide <see cref="Selections"/> that fulfill the <see cref="Requirements"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/>.</exception>
        /// <remarks>When inheriting this method is usually replaced.</remarks>
        public abstract ExitCode Execute();

        /// <summary>
        /// Generates a localized instruction string describing multiple selectable values.
        /// </summary>
        /// <param name="values">The values to list.</param>
        protected static string SupportedValues<T>(params T[] values)
            => string.Format(Resources.SupportedValues, StringUtils.Join(", ", values.Select(AttributeUtils.ConvertToString)));

        /// <summary>
        /// Generates a localized instruction string describing multiple selectable enum values.
        /// </summary>
        /// <typeparam name="T">The enum type to list values for.</typeparam>
        protected static string SupportedValues<T>()
            => SupportedValues(Enum.GetValues(typeof(T)).Cast<T>().ToArray());
    }
}
