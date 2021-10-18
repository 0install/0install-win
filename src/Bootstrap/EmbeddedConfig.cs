// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Configuration;
using NanoByte.Common;
using NanoByte.Common.Streams;
using ZeroInstall.Model;

namespace ZeroInstall
{
    public enum BootstrapMode
    {
        None,
        Run,
        Integrate,
    }

    /// <summary>
    /// Represents configuration embedded into the executable itself.
    /// This is used to create a customized bootstrapper that uses Zero Install to run or integrate another application.
    /// </summary>
    public class EmbeddedConfig
    {
        /// <summary>
        /// The name of the target application to bootstrap.
        /// </summary>
        public string? AppName { get; }

        /// <summary>
        /// The feed URI of the target application to bootstrap.
        /// </summary>
        public FeedUri? AppUri { get; }

        /// <summary>
        /// The application bootstrapping mode to use.
        /// </summary>
        public BootstrapMode AppMode { get; }

        /// <summary>
        /// Additional command-line arguments to pass to the application (if <see cref="AppMode"/> is <see cref="BootstrapMode.Run"/>) or <c>0install-win integrate</c> (if <see cref="AppMode"/> is <see cref="BootstrapMode.Integrate"/>).
        /// </summary>
        public string? AppArgs { get; }

        /// <summary>
        /// The GnuPG key fingerprint to trust for signing the application's feed.
        /// </summary>
        public string? AppFingerprint { get; }

        /// <summary>
        /// Loads the embedded configuration.
        /// </summary>
        private EmbeddedConfig()
        {
            string[] lines = typeof(EmbeddedConfig).GetEmbeddedString("EmbeddedConfig.txt").SplitMultilineText();

            string? ReadConfig(string key, int lineNumber, string placeholder)
            {
                string setting = ConfigurationManager.AppSettings[key];
                if (!string.IsNullOrEmpty(setting))
                {
                    Log.Info($"AppSettings config: {key}: {setting}");
                    return setting;
                }

                string line = lines[lineNumber].TrimEnd();
                if (!string.IsNullOrEmpty(line) && !(line.Contains(placeholder) && line.StartsWith("--") && line.EndsWith("--")))
                {
                    Log.Info($"Embedded config: {key}: {line}");
                    return line;
                }

                return null;
            }

            AppUri = ReadConfig("app_uri", lineNumber: 0, nameof(AppUri))?.To(x => new FeedUri(x));
            AppName = ReadConfig("app_name", lineNumber: 1, placeholder: nameof(AppName));
            AppMode = GetAppMode(ReadConfig("app_mode", lineNumber: 2, placeholder: nameof(AppMode)));
            AppArgs = ReadConfig("app_args", lineNumber: 3, placeholder: nameof(AppArgs));
            AppFingerprint = ReadConfig("app_fingerprint", lineNumber: 4, placeholder: nameof(AppFingerprint));
        }

        private static BootstrapMode GetAppMode(string? value)
            => value switch
            {
                "run" => BootstrapMode.Run,
                "integrate" => BootstrapMode.Integrate,
                _ => BootstrapMode.None
            };

        /// <summary>
        /// The embedded config as a singleton.
        /// </summary>
        public static readonly EmbeddedConfig Instance = new();
    }
}
