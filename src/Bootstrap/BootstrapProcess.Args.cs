// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Reflection;
using NanoByte.Common.Streams;
using NDesk.Options;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall;

partial class BootstrapProcess
{
    /// <summary>The command-line argument parser used to evaluate user input.</summary>
    private readonly OptionSet _options;

    /// <summary>A specific version of Zero Install to download.</summary>
    private VersionRange? _version;

    /// <summary>A specific version of the target application to download.</summary>
    private VersionRange? _appVersion;

    /// <summary>Arguments passed through to the target process.</summary>
    private readonly List<string> _userArgs = new();

    /// <summary>Download all files required to run off-line later.</summary>
    private bool _prepareOffline;

    /// <summary>Do not run the application after downloading it.</summary>
    private bool _noRun;

    /// <summary>Do not integrate the application into the desktop environment.</summary>
    private bool _noIntegrate;

    /// <summary>Integrate the application machine-wide (for the entire computer) instead of just for the current user.</summary>
    private bool _machineWide;

    /// <summary>A directory to search for feeds and archives to import.</summary>
    private string? _contentDir;

    private string HelpText
    {
        get
        {
            string exeName = Path.GetFileNameWithoutExtension(new Uri(Assembly.GetEntryAssembly()!.CodeBase).LocalPath);

            using var buffer = new MemoryStream();
            var writer = new StreamWriter(buffer);
            if (_embeddedConfig is {AppUri: not null, AppName: not null})
            {
                writer.WriteLine("This bootstrapper downloads and {0} {1} using Zero Install.", _embeddedConfig.IntegrateArgs == null ? "runs" : "integrates", _embeddedConfig.AppName);
                writer.WriteLine("Usage: {0} [OPTIONS] [[--] APP-ARGS]", exeName);
                writer.WriteLine();
                writer.WriteLine("Samples:");
                writer.WriteLine("  {0}               Run {1}.", exeName, _embeddedConfig.AppName);
                writer.WriteLine("  {0} --offline     Run {1} without downloading anything.", exeName, _embeddedConfig.AppName);
                writer.WriteLine("  {0} -x            Run {1} with argument '-x'.", exeName, _embeddedConfig.AppName);
                writer.WriteLine("  {0} -- --offline  Run {1} with argument '--offline'.", exeName, _embeddedConfig.AppName);
            }
            else
            {
                writer.WriteLine("This bootstrapper downloads and runs Zero Install.");
                writer.WriteLine("Usage: {0} [OPTIONS] [[--] 0INSTALL-ARGS]", exeName);
                writer.WriteLine();
                writer.WriteLine("Samples:");
                writer.WriteLine("  {0} self deploy  Deploy Zero Install to this computer.", exeName);
                writer.WriteLine("  {0} central      Open main Zero Install GUI.", exeName);
                writer.WriteLine("  {0} run vlc      Run VLC via Zero Install.", exeName);
                writer.WriteLine("  {0} -- --help    Show help for Zero Install instead of Bootstrapper.", exeName);
            }
            writer.WriteLine();
            writer.WriteLine("Options:");
            _options.WriteOptionDescriptions(writer);
            writer.Flush();
            return buffer.ReadToString();
        }
    }

    /// <summary>
    /// Creates a new bootstrap process.
    /// </summary>
    /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
    public BootstrapProcess(IBootstrapHandler handler)
        : base(handler)
    {
        _handler = handler;

        _options = new OptionSet
        {
            {
                "?|h|help", () => "Show the built-in help text.", _ =>
                {
                    _handler.Output("Help", HelpText);
                    throw new OperationCanceledException(); // Don't handle any of the other arguments
                }
            },
            {
                "batch", () => "Automatically answer questions with defaults when possible. Avoid unnecessary console output (e.g. progress bars).", _ =>
                {
                    if (_handler.Verbosity >= Verbosity.Verbose) throw new OptionException("Cannot combine --batch and --verbose", "verbose");
                    _handler.Verbosity = Verbosity.Batch;
                }
            },
            {
                "v|verbose", () => "More verbose output. Use twice for even more verbose output.", _ =>
                {
                    if (_handler.Verbosity == Verbosity.Batch) throw new OptionException("Cannot combine --batch and --verbose", "batch");
                    _handler.Verbosity++;
                }
            },
            {
                "feed=", () => "Specify an alternative {FEED} for Zero Install.", feed => Config.SelfUpdateUri = new(feed)
            },
            {
                "content-dir=", () => "Specifies a {DIRECTORY} to search for feeds and archives to import. The default is a directory called 'content'.", path =>
                {
                    if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"Directory '{path}' not found.");
                    _contentDir = path;
                }
            },
            {
                "o|offline", () => "Run in off-line mode, not downloading anything.", _ => Config.NetworkUse = NetworkLevel.Offline
            },
            {
                "r|refresh", () => "Fetch fresh copies of all used feeds.", _ => FeedManager.Refresh = true
            },
            {
                "prepare-offline", () => "Download all files required to run off-line later.", _ => _prepareOffline = true
            }
        };
        if (handler.IsGui)
            _options.Add("background", () => "Hide the graphical user interface.", _ => _handler.Background = true);

        if (_embeddedConfig is {AppUri: not null, AppName: not null})
        {
            _options.Add("zero-install-version=", () => $"Use a specific {{VERSION}} of Zero Install.", (VersionRange range) => _version = range);
            _options.Add("version=", () => $"Use a specific {{VERSION}} of {_embeddedConfig.AppName}.", (VersionRange range) => _appVersion = range);
            _options.Add("no-run", () => $"Do not run {_embeddedConfig.AppName} after downloading it.", _ => _noRun = true);
            _options.Add("s|silent", () => "Equivalent to --no-run --batch.", _ =>
            {
                _noRun = true;
                _handler.Verbosity = Verbosity.Batch;
            });
            if (handler.IsGui)
            {
                _options.Add("S|verysilent", () => "Equivalent to --no-run --batch --background.", _ =>
                {
                    _noRun = true;
                    _handler.Verbosity = Verbosity.Batch;
                    _handler.Background = true;
                });
            }
            if (_embeddedConfig.IntegrateArgs != null)
            {
                _options.Add("no-integrate", () => $"Do not integrate {_embeddedConfig.AppName} into the desktop environment.", _ => _noIntegrate = true);
                if (!_embeddedConfig.IntegrateArgs.Contains("--machine"))
                    _options.Add("machine", () => $"Integrate {_embeddedConfig.AppName} machine-wide (for the entire computer) instead of just for the current user.", _ => _machineWide = true);
            }
        }
        else
        {
            _options.Add("version=", () => $"Use a specific {{VERSION}} of Zero Install.", (VersionRange range) => _version = range);
        }

        // Work-around to disable interspersed arguments (needed for passing arguments through to sub-processes)
        _options.Add("<>", value =>
        {
            _userArgs.Add(value);

            // Stop using options parser, treat everything from here on as unknown
            _options.Clear();
        });
    }

    /// <summary>
    /// Handles arguments passed to 0install that are also applicable to the bootstrapper.
    /// </summary>
    private void ShareArgsWithZeroInstall()
    {
        foreach (string arg in _userArgs)
        {
            switch (arg)
            {
                case "--batch":
                    _handler.Verbosity = Verbosity.Batch;
                    break;
                case "--verbose" or "-v":
                    _handler.Verbosity = Verbosity.Verbose;
                    break;
                case "--background":
                    _handler.Background = true;
                    break;
                case "--offline" or "-o":
                    Config.NetworkUse = NetworkLevel.Offline;
                    break;
                case "--refresh" or "-r":
                    FeedManager.Refresh = true;
                    break;
            }
        }
    }

    /// <summary>
    /// Adds arguments passed to the bootstrapper that are also applicable to 0install to <paramref name="args"/>.
    /// </summary>
    private void ShareArgsWithZeroInstall(IList<string> args)
    {
        void AddArg(string arg, bool allowDuplicate = false)
        {
            if (allowDuplicate || !args.Contains(arg))
                args.Insert(0, arg);
        }

        switch (_handler.Verbosity)
        {
            case Verbosity.Batch when !args.Contains("-v") && !args.Contains("--verbose"):
                AddArg("--batch");
                break;
            case Verbosity.Verbose when !args.Contains("--batch"):
                AddArg("--verbose");
                break;
            case Verbosity.Debug when !args.Contains("--batch"):
                AddArg("--verbose");
                AddArg("--verbose", allowDuplicate: true);
                break;
        }

        if (!args.Contains("central"))
        {
            if (_handler.Background) AddArg("--background");
            if (Config.NetworkUse == NetworkLevel.Offline) AddArg("--offline");
            if (FeedManager.Refresh) AddArg("--refresh");
        }
    }
}
