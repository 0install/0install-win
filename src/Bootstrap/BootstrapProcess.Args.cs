// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Reflection;
using NanoByte.Common.Streams;
using NDesk.Options;

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
    private readonly List<string> _userArgs = [];

    /// <summary>Run in off-line mode, not downloading anything.</summary>
    private bool _offline;

    /// <summary>Download all files required to run off-line later.</summary>
    private bool _prepareOffline;

    /// <summary>Do not run the application after downloading it.</summary>
    private bool _noRun;

    /// <summary>Wait for the application to exit after running it.</summary>
    private bool _wait;

    /// <summary>Custom path for storing implementations.</summary>
    private string? _storePath;

    /// <summary>Do not integrate the application into the desktop environment.</summary>
    private bool _noIntegrate;

    /// <summary>Override command-line arguments for '0install integrate'.</summary>
    private string? _integrateArgs;

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
            if (BootstrapConfig.Instance is {AppUri: not null, AppName: {} appName})
            {
                writer.WriteLine("This bootstrapper downloads and {0} {1} using Zero Install.", BootstrapConfig.Instance.IntegrateArgs == null ? "runs" : "integrates", appName);
                writer.WriteLine("Usage: {0} [OPTIONS] [[--] APP-ARGS]", exeName);
                writer.WriteLine();
                writer.WriteLine("Samples:");
                writer.WriteLine("  {0}               Run {1}.", exeName, appName);
                writer.WriteLine("  {0} --offline     Run {1} without downloading anything.", exeName, appName);
                writer.WriteLine("  {0} -x            Run {1} with argument '-x'.", exeName, appName);
                writer.WriteLine("  {0} -- --offline  Run {1} with argument '--offline'.", exeName, appName);
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
            {"o|offline", () => "Run in off-line mode, not downloading anything.", _ => _offline = true},
            {"r|refresh", () => "Fetch fresh copies of all used feeds.", _ => FeedManager.Refresh = true},
            {"prepare-offline", () => "Download all files required to run off-line later.", _ => _prepareOffline = true}
        };
        if (handler.IsGui)
            _options.Add("background", () => "Hide the graphical user interface.", _ => _handler.Background = true);

        if (BootstrapConfig.Instance.CustomizableStorePath)
            _options.Add("store-path=", () => "Custom {PATH} for storing implementations.", x => _storePath = x);

        if (BootstrapConfig.Instance is {AppUri: not null, AppName: {} appName})
        {
            _options.Add("0install-version=", () => "Use a specific {{VERSION}} of Zero Install.", (VersionRange range) => _version = range);
            _options.Add("version=", () => $"Use a specific {{VERSION}} of {appName}.", (VersionRange range) => _appVersion = range);
            _options.Add("no-run", () => $"Do not run {appName} after downloading it.", _ => _noRun = true);
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
                _options.Add("wait", () => "Wait for {_embeddedConfig.AppName} to exit after running it.", _ => _wait = true);
            }
            if (BootstrapConfig.Instance.IntegrateArgs != null)
            {
                _options.Add("no-integrate", () => $"Do not integrate {appName} into the desktop environment.", _ => _noIntegrate = true);
                _options.Add("integrate-args=", () => "Override command-line arguments for '0install integrate'.", x => _integrateArgs = x);
                _options.Add("machine", () => $"Integrate {appName} machine-wide (for the entire computer) instead of just for the current user.", _ => _machineWide = true);
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
    /// Handles 0install command-line options that are also applicable to the bootstrapper.
    /// </summary>
    private void ApplySharedOptions()
    {
        foreach (string arg in _userArgs)
        {
            switch (arg)
            {
                case "--batch":
                    _handler.Verbosity = Verbosity.Batch;
                    break;
                case "--verbose":
                    _handler.Verbosity = Verbosity.Verbose;
                    break;
                case "--background":
                    _handler.Background = true;
                    break;
                case "--offline":
                    _offline = true;
                    break;
                case "--refresh":
                    FeedManager.Refresh = true;
                    break;
            }
        }
    }

    /// <summary>
    /// Adds bootstrapper command-line options that are also applicable to 0install to <paramref name="args"/>.
    /// </summary>
    private void AddSharedOptions(IList<string> args)
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

        if (_handler.Background && !args.Contains("central"))
            AddArg("--background");

        if (args.Intersect(["select", "download", "run", "add", "integrate"]).Any())
        {
            if (_offline) AddArg("--offline");
            if (FeedManager.Refresh) AddArg("--refresh");
        }
    }
}
