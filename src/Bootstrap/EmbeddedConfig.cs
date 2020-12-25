// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
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
        /// Only relevant if <see cref="AppMode"/> is not <see cref="BootstrapMode.None"/>.
        /// </summary>
        public string AppName { get; }

        /// <summary>
        /// The feed URI of the target application to bootstrap.
        /// Only relevant if <see cref="AppMode"/> is not <see cref="BootstrapMode.None"/>.
        /// </summary>
        public FeedUri AppUri { get; }

        /// <summary>
        /// The application bootstrapping mode to use.
        /// </summary>
        public BootstrapMode AppMode { get; }

        /// <summary>
        /// Loads the embedded configuration.
        /// </summary>
        private EmbeddedConfig()
        {
            var lines = typeof(EmbeddedConfig).GetEmbeddedString("EmbeddedConfig.txt").SplitMultilineText();

            try
            {
                AppUri = new(ConfigurationManager.AppSettings["app_uri"] ?? lines[0].TrimEnd());
                Log.Info("Embedded config: AppUri: " + AppUri);

                AppName = ConfigurationManager.AppSettings["app_name"] ?? lines[1].TrimEnd();
                Log.Info("Embedded config: AppName: " + AppName);

                AppMode = GetAppMode(ConfigurationManager.AppSettings["app_mode"] ?? lines[2].TrimEnd());
                Log.Info("Embedded config: AppMode: " + AppMode);
            }
            catch (UriFormatException)
            {
                // No (valid) feed URI set
            }
        }

        private static BootstrapMode GetAppMode(string value)
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
