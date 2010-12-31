/*
 * Copyright 2010 Dennis Keil
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.ObjectModel;
using System.Windows;
using Common.Collections;
using Common.Wpf;
using Common;
using ZeroInstall.Launcher;
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Store.Implementation;
using System.ComponentModel;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

namespace ZeroInstall.Central.Wpf
{
    public class InstallAction : Common.Wpf.Model, IHandler
    {
        private AppInfo _appInfo;
        public AppInfo AppInfo
        {
            get
            {
                return _appInfo;
            }
            set
            {
                _appInfo = value;
                NotifyPropertyChanged("AppInfo");
            }
        }

        public InstallAction(AppInfo appInfo)
        {
            AppInfo = appInfo;
            AppInfo.StateProgressPercent = -1;
            AppInfo.StateVisible = true;
            AppInfo.StateAction = "Installation";
        }

        public event Action Finished;
        
        public void Start()
        {
            //State = "Initialising...";
            AppInfo.StateProgressPercent = 0;

            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += delegate
            {
                new Controller(AppInfo.Feed.UriString, SolverProvider.Default, Policy.CreateDefault(), this).DownloadUncachedImplementations();
            };
            backgroundWorker.RunWorkerCompleted += b_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        void b_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AppInfo.StateAction = "";
            AppInfo.StateVisible = false;
            AppInfo.RefreshImplementation();

            // Create desktop link
            var dirStore = new DirectoryStore();
            String path = dirStore.GetPath(AppInfo.Implementation.ManifestDigest);
                
            String exePath = path + @"\" + AppInfo.Implementation.Main;

            Link.Update(Environment.SpecialFolder.Desktop, exePath, AppInfo.Feed.Name + ".lnk", true);

            Finished();
        }

        public bool Batch { get; set; }

        public void RunDownloadTask(ITask task)
        {
            //State = "Downloading";
            AppInfo.StateProgressColor = Color.FromArgb(255, 1, 211, 40);
            task.ProgressChanged += sender => AppInfo.StateProgressPercent = Math.Max(0, task.Progress) * 100;
            task.RunSync();
        }

        public void RunIOTask(ITask task)
        {
            //State = "Extracting";
            AppInfo.StateProgressColor = Color.FromArgb(255, 255, 255, 0);
            task.ProgressChanged += sender => AppInfo.StateProgressPercent = Math.Max(0, task.Progress) * 50;
            task.RunSync();
        }

        public bool AcceptNewKey(string information)
        {
            //AppInfo.StateProgressPercent = 10;
            //State = "Ask key";
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


        InstallAction _currentInstallAction;


        public InstallManager()
        {
            InstallActions = new ObservableCollection<InstallAction>();
        }

        public void Install(AppInfo appInfo)
        {
            InstallActions.Add(new InstallAction(appInfo));

            TryInstallNext();
        }

        private void TryInstallNext()
        {
            if(InstallActions.Count == 0) return;
            if (_currentInstallAction != null) return;

            _currentInstallAction = EnumUtils.GetFirst(InstallActions);

            _currentInstallAction.Finished += currentInstallAction_Finished;
            _currentInstallAction.Start();
        }

        void currentInstallAction_Finished()
        {
            MainWindow.TaskbarIcon.ShowBalloonTip(_currentInstallAction.AppInfo.Feed.Name, "Application has been installed.", BalloonIcon.Info);
            //Msg.Inform(App.NativeWnd, currentInstallAction.FeedUri + " has been installed.", MsgSeverity.Info);

            InstallActions.RemoveAt(0);
            _currentInstallAction = null;
            TryInstallNext();
        }
    }
}
