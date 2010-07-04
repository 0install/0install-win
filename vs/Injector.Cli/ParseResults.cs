using System.Collections.Generic;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Cli
{
    #region Enumerations
    /// <summary>
    /// List of operational modes that can be selected via command-line arguments.
    /// </summary>
    public enum ProgramMode
    {
        /// <summary>Launch or download an <see cref="Model.Feed"/>.</summary>
        Normal,
        /// <summary>List known <see cref="Model.Feed"/>s.</summary>
        List,
        /// <summary>Import feed files from local source.</summary>
        Import,
        /// <summary>Add feed aliases.</summary>
        Manage,
        /// <summary>Display version information.</summary>
        Version
    }
    #endregion

    /// <summary>
    /// Structure for storing user-selected argument for an operation.
    /// </summary>
    public struct ParseResults
    {
        /// <summary>The selected operational mode.</summary>
        public ProgramMode Mode;

        /// <summary>User settings controlling the dependency solving process.</summary>
        public Policy Policy;

        /// <summary>Only download <see cref="Implementation"/>s but don't execute them.</summary>
        public bool DownloadOnly;

        /// <summary>Only output what was supposed to be downloaded but don't actually use the network.</summary>
        public bool DryRun;

        /// <summary>Print the selected <see cref="Implementation"/>s to the console instead of executing them.</summary>
        public bool GetSelections;

        /// <summary>Only download feeds and not <see cref="Implementation"/>s.</summary>
        public bool SelectOnly;

        /// <summary>Load <see cref="Selections"/> from this file instead of using an <see cref="ISolver"/>.</summary>
        public string SelectionsFile;

        /// <summary>An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="ImplementationBase.Main"/>.</summary>
        public string Main;

        /// <summary>Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.</summary>
        public string Wrapper;

        /// <summary>The interface to launch, feed to download/add, term to search for, etc.</summary>
        public string Feed;

        /// <summary>Arguments to pass to the launched application, additional feeds to add, additional terms to search for, etc.</summary>
        public IList<string> AdditionalArgs;
    }
}
