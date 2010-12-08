using System;
using System.Collections.ObjectModel;
using System.Windows;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using Common.Wpf;
using Common;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

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
        
        public void Start()
        {
            //this.State = "Initialising...";
            this.AppInfo.StateProgressPercent = 0;

            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += delegate
            {
                new Controller(AppInfo.Feed.UriString, SolverProvider.Default, Policy.CreateDefault(this)).DownloadUncachedImplementations();
            };
            backgroundWorker.RunWorkerCompleted += b_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        void b_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.AppInfo.StateAction = "";
            this.AppInfo.StateVisible = false;
            this.AppInfo.RefreshImplementation();

            // Create desktop link
            DirectoryStore dirStore = new DirectoryStore();
            String path = dirStore.GetPath(this.AppInfo.Implementation.ManifestDigest);
                
            String exePath = path + @"\" + this.AppInfo.Implementation.Main;

            Link.Update(Environment.SpecialFolder.Desktop, exePath, this.AppInfo.Feed.Name + ".lnk", true);

            this.Finished();
        }

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

            this.currentInstallAction = EnumUtils.GetFirst(InstallActions);

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
