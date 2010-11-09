using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using Common.Storage;
using Common.Wpf;
using Common;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Arguments;
using ZeroInstall.Injector.Solver;
using Common.Utils;
using ZeroInstall.Store.Implementation;
using System.ComponentModel;
using System.IO;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Store.Feed;
using System.Threading;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;
using System.Diagnostics;

namespace ZeroInstall.Central.Wpf
{
    public class InstallAction : Common.Wpf.Model, IHandler
    {
        private AppInfo appInfo;
        public AppInfo AppInfo
        {
            get
            {
                return this.appInfo;
            }
            set
            {
                this.appInfo = value;
                this.NotifyPropertyChanged("AppInfo");
            }
        }

        public InstallAction(AppInfo appInfo)
        {
            this.AppInfo = appInfo;
            this.AppInfo.StateProgressPercent = -1;
            this.AppInfo.StateVisible = true;
            this.AppInfo.StateAction = "Installation";
        }

        public event Action Finished;

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The arguments to be parsed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <param name="results">The options detected by the parsing process.</param>
        /// <returns>The operation mode selected by the parsing process.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        public OperationMode ParseArgs(IEnumerable<string> args, IHandler handler, out ParseResults results)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Prepare a structure for storing settings found in the arguments
            var mode = OperationMode.Normal;
            var parseResults = new ParseResults { Policy = Policy.CreateDefault(handler) };

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"i|import", unused => mode = OperationMode.Import},
                {"l|list", unused => mode = OperationMode.List},
                {"f|feed", unused => mode = OperationMode.Manage},
                {"V|version", unused => mode = OperationMode.Version},

                // Policy options
                {"before=", version => parseResults.Policy.Constraint.BeforeVersion = new ImplementationVersion(version)},
                {"not-before=", version => parseResults.Policy.Constraint.NotBeforeVersion = new ImplementationVersion(version)},
                {"s|source", unused => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Cpu.Source)},
                {"os=", os => parseResults.Policy.Architecture = new Architecture(Architecture.ParseOS(os), parseResults.Policy.Architecture.Cpu)},
                {"cpu=", cpu => parseResults.Policy.Architecture = new Architecture(parseResults.Policy.Architecture.OS, Architecture.ParseCpu(cpu))},
                {"o|offline", unused =>  parseResults.Policy.InterfaceCache.NetworkLevel = NetworkLevel.Offline},
                {"r|refresh", unused => parseResults.Policy.InterfaceCache.Refresh = true},
                {"with-store=", path => parseResults.Policy.AdditionalStore = new DirectoryStore(path)},

                // Special operations
                {"d|download-only", unused => parseResults.DownloadOnly = true},
                {"set-selections=", file => parseResults.SelectionsFile = file},
                {"batch", unused => handler.Batch = true},

                // Launcher options
                {"m|main=", newMain => parseResults.Main = newMain},
                {"w|wrapper=", newWrapper => parseResults.Wrapper = newWrapper},

                // Operation modifiers
                {"no-wait", unused => parseResults.NoWait = true}
            };
            #endregion

            #region Feed and arguments
            var targetArgs = new List<string>();
            parseResults.AdditionalArgs = targetArgs;
            options.Add("<>", v =>
            {
                if (parseResults.Feed == null)
                {
                    if (v.StartsWith("-")) throw new ArgumentException("Unknown options");

                    parseResults.Feed = v;
                    options.Clear();
                }
                else targetArgs.Add(v);
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            options.Parse(args);

            // Return the now filled results structure
            results = parseResults;
            return mode;
        }
        #endregion

        public void Start()
        {
            //this.State = "Initialising...";
            this.AppInfo.StateProgressPercent = 0;

            String[] args = new String[2];
            args[0] = "--no-wait";
            args[1] = this.AppInfo.Feed.UriString;


            var handler = this;
            ParseResults results;
            OperationMode mode;

            try { mode = ParseArgs(args, handler, out results); }
            #region Error handling
            catch (ArgumentException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Warning);
                return;
            }
            #endregion

            results.DownloadOnly = true;

            switch (mode)
            {
                case OperationMode.Normal:

                    BackgroundWorker b = new BackgroundWorker();

                    b.DoWork += new DoWorkEventHandler(delegate (object sender, DoWorkEventArgs e)
                    {
                        handler.Execute(results);
                    });

                    b.RunWorkerCompleted += new RunWorkerCompletedEventHandler(b_RunWorkerCompleted);
                    b.RunWorkerAsync();

                    //Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    //    {
                    //        handler.Execute(results);
                    //    }));
                    //t.Start();
                    break;

                case OperationMode.List:
                case OperationMode.Import:
                case OperationMode.Manage:
                    Msg.Inform(null, "Not implemented yet!", MsgSeverity.Error);
                    break;

                case OperationMode.Version:
                    // ToDo: Read version number from assembly data
                    Msg.Inform(null, "Zero Install for Windows Injector v1.0", MsgSeverity.Information);
                    break;

                default:
                    Msg.Inform(null, "Unknown operation mode", MsgSeverity.Error);
                    break;
            }
            //MessageBox.Show("Started");
            //this.Finished();
            //throw new NotImplementedException();
        }

        void b_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.AppInfo.StateAction = "";
            this.AppInfo.StateVisible = false;
            this.AppInfo.RefreshImplementation();

            // Create desktop link
            String implementationsPath = Path.Combine(Locations.GetUserCacheDir("0install.net"), "implementations");
            DirectoryStore dirStore = new DirectoryStore(implementationsPath);
            String path = dirStore.GetPath(this.AppInfo.Implementation.ManifestDigest);
                
            String exePath = path + @"\" + this.AppInfo.Implementation.Main;

            Link.Update(Environment.SpecialFolder.Desktop, exePath, this.AppInfo.Feed.Name + ".lnk", true);

            this.Finished();
        }

        #region Execute
        public void Execute(ParseResults results)
        {
            // ToDo: Alternative policy for DryRun
            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

            if (results.SelectionsFile == null) controller.Solve();
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            if (!results.SelectOnly)
            {
                // ToDo: Add progress callbacks
                controller.DownloadUncachedImplementations();
            }

            if (results.GetSelections)
            {
                Console.Write(controller.GetSelections().WriteToString());
            }
            else if (!results.DownloadOnly && !results.SelectOnly)
            {
                var launcher = controller.GetLauncher();
                launcher.Main = results.Main;
                launcher.Wrapper = results.Wrapper;

                var startInfo = launcher.GetStartInfo(StringUtils.Concatenate(results.AdditionalArgs, " "));

                try
                {
                    if (results.NoWait) ProcessUtils.RunDetached(startInfo);
                    else ProcessUtils.RunReplace(startInfo);
                }
                #region Error hanlding
                catch (ImplementationNotFoundException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (MissingMainException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (Win32Exception ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (BadImageFormatException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                #endregion
            }
        }
        #endregion

        public bool Batch
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void StartingDownload(IProgress download)
        {
            //this.State = "Downloading";
            this.AppInfo.StateProgressColor = Color.FromArgb(255, 1, 211, 40);
            download.ProgressChanged += new ProgressEventHandler(delegate(IProgress sender)
            {
                this.AppInfo.StateProgressPercent = Math.Max(0, download.Progress) * 100;
            });
        }

        public void StartingExtraction(IProgress extraction)
        {
            //this.State = "Extracting";
            this.AppInfo.StateProgressColor = Color.FromArgb(255, 255, 255, 0);
            extraction.ProgressChanged += new ProgressEventHandler(delegate(IProgress sender)
            {
                this.AppInfo.StateProgressPercent = Math.Max(0, extraction.Progress) * 50;
            });
        }

        public void StartingManifest(IProgress manifest)
        {
            //this.State = "Installing";
            manifest.ProgressChanged += new ProgressEventHandler(delegate(IProgress sender)
            {
                this.AppInfo.StateProgressPercent = 50 + Math.Max(0, manifest.Progress) * 50;
            });
        }

        public bool AcceptNewKey(string information)
        {
            //this.AppInfo.StateProgressPercent = 10;
            //this.State = "Ask key";
            return true;
        }
    }

    public class InstallManager : DependencyObject
    {
        public ObservableCollection<InstallAction> InstallActions
        {
            get { return (ObservableCollection<InstallAction>)GetValue(InstallActionsProperty); }
            set { SetValue(InstallActionsProperty, value); }
        }

        public static readonly DependencyProperty InstallActionsProperty =
            DependencyProperty.Register("InstallActions", typeof(ObservableCollection<InstallAction>), typeof(InstallManager), new UIPropertyMetadata(null));


        InstallAction currentInstallAction;


        public InstallManager()
        {
            this.InstallActions = new ObservableCollection<InstallAction>();
        }

        public void Install(AppInfo appInfo)
        {
            this.InstallActions.Add(new InstallAction(appInfo));

            this.TryInstallNext();
        }

        private void TryInstallNext()
        {
            if(this.InstallActions.Count == 0) return;
            if (this.currentInstallAction != null) return;

            this.currentInstallAction = this.InstallActions[0];

            this.currentInstallAction.Finished += new Action(currentInstallAction_Finished);
            this.currentInstallAction.Start();
        }

        void currentInstallAction_Finished()
        {
            MainWindow.TaskbarIcon.ShowBalloonTip(this.currentInstallAction.AppInfo.Feed.Name, "Application has been installed.", BalloonIcon.Info);
            //Msg.Inform(App.NativeWnd, this.currentInstallAction.FeedUri + " has been installed.", MsgSeverity.Information);

            this.InstallActions.RemoveAt(0);
            this.currentInstallAction = null;
            this.TryInstallNext();
        }
    }
}
