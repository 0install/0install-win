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
using System.Linq;
using Common.Wpf;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using System.IO;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;
using Common;

namespace ZeroInstall.Central.Wpf
{
    public class AppInfo : Common.Wpf.Model
    {
        #region Old

        //private String iconUrl = "";
        //public String IconUrl
        //{
        //    get
        //    {
        //        return iconUrl;
        //    }
        //    set
        //    {
        //        iconUrl = value;
        //        NotifyPropertyChanged("IconUrl");
        //    }
        //}

        //private String iconUrl = "";
        //public String IconUrl
        //{
        //    get
        //    {
        //        return iconUrl;
        //    }
        //    set
        //    {
        //        iconUrl = value;
        //        NotifyPropertyChanged("IconUrl");
        //    }
        //}


        //private String name = "";
        //public String Name
        //{
        //    get
        //    {
        //        return name;
        //    }
        //    set
        //    {
        //        name = value;
        //        NotifyPropertyChanged("Name");
        //    }
        //}

        //private String version = "";
        //public String Version
        //{
        //    get
        //    {
        //        return version;
        //    }
        //    set
        //    {
        //        version = value;
        //        NotifyPropertyChanged("Version");
        //    }
        //}

        //private String publisher = "";
        //public String Publisher
        //{
        //    get
        //    {
        //        return publisher;
        //    }
        //    set
        //    {
        //        publisher = value;
        //        NotifyPropertyChanged("Publisher");
        //    }
        //}

        //public AppInfo(String name, String version, String publisher, String size, String iconUrl)
        //{
        //    Name = name;
        //    Version = version;
        //    Publisher = publisher;
        //    Size = size;
        //    IconUrl = iconUrl;
        //}

        #endregion

        public Boolean IsStateIdle
        {
            get
            {
                return StateAction.Length == 0;
            }
        }

        private Boolean _stateVisible;
        public Boolean StateVisible
        {
            get
            {
                return _stateVisible;
            }
            set
            {
                _stateVisible = value;
                NotifyPropertyChanged("StateVisible");
            }
        }

        private Color _stateProgressColor = Color.FromArgb(255, 1, 211, 40);
        public Color StateProgressColor
        {
            get
            {
                return _stateProgressColor;
            }
            set
            {
                _stateProgressColor = value;
                NotifyPropertyChanged("StateProgressColor");
                NotifyPropertyChanged("StateProgressColorBrush");
            }
        }

        public Brush StateProgressColorBrush
        {
            get
            {
                return new SolidColorBrush(StateProgressColor);
            }
        }


        private double _stateProgressPercent;
        public double StateProgressPercent
        {
            get
            {
                return _stateProgressPercent;
            }
            set
            {
                _stateProgressPercent = value;
                NotifyPropertyChanged("StateProgressPercent");
                NotifyPropertyChanged("StateProgressStr");
            }
        }

        public String StateProgressStr
        {
            get
            {
                if (StateProgressPercent > 0)
                {
                    return String.Format("{0:0}%", StateProgressPercent);
                }
                else
                {
                    return "Waiting";
                }
            }
        }

        private String _stateAction = "";
        public String StateAction
        {
            get
            {
                return _stateAction;
            }
            set
            {
                _stateAction = value;
                NotifyPropertyChanged("StateAction");
                NotifyPropertyChanged("IsStateIdle");
            }
        }

        private String _size = "";
        public String Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                NotifyPropertyChanged("Size");
            }
        }

        private Feed _feed;
        public Feed Feed
        {
            get
            {
                return _feed;
            }
            set
            {
                _feed = value;
                NotifyPropertyChanged("Feed");
            }
        }

        private Implementation _implementation;
        public Implementation Implementation
        {
            get
            {
                return _implementation;
            }
            set
            {
                _implementation = value;
                NotifyPropertyChanged("Implementation");
            }
        }


        private static String FormatBytes(long bytes)
        {
            const int scale = 1024;
            var orders = new[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }


        public ICommand ExecuteCommand { get; private set; }
        public ICommand UninstallCommand { get; private set; }

        public static bool CanExecuteCommand(object parameter)
        {
            return true;
        }

        public void PerformExecuteCommand(object parameter)
        {
            try
            {
                var dirStore = new DirectoryStore();
                String path = dirStore.GetPath(Implementation.ManifestDigest);
                
                Process.Start(new ProcessStartInfo(path + @"\" + _implementation.Main));

                MainWindow.TaskbarIcon.ShowBalloonTip(Feed.Name, "Application is starting.", BalloonIcon.Info);
            }
            catch (Exception ex)
            { 
                System.Windows.MessageBox.Show("Error executing " + Feed.Name + ". Details: " + ex);
            }

            //Launcher l = new Launcher(Implementation.ID, new Launcher.Solver.Selections()
        }

        public static bool CanUninstallCommand(object parameter)
        {
            return true;
        }

        public void PerformUninstallCommand(object parameter)
        {
            if (!Msg.Ask(
                App.NativeWnd,
                "Do you really want to uninstall \"" + Feed.Name + "\"?",
                MsgSeverity.Warn,
                "Yes\nUninstall the application",
                "No\nGo back to the list"
                ))
            {
                return;
            }

            try
            {
                var dirStore = new DirectoryStore();
                String path = dirStore.GetPath(Implementation.ManifestDigest);

                Directory.Delete(path, true);

                // Delete desktop link
                String exePath = path + @"\" + Implementation.Main;

                Link.Update(Environment.SpecialFolder.Desktop, exePath, Feed.Name + ".lnk", false);


                MainWindow.Instance.UninstalledApp(this);

                MainWindow.TaskbarIcon.ShowBalloonTip(Feed.Name, "Application has been uninstalled.", BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error uninstalling " + Feed.Name + ". Details: " + ex);
            }

        }

        public AppInfo(Feed feed, Implementation implementation)
        {
            Feed = feed;
            Implementation = implementation;

            ExecuteCommand = new RelayCommand(PerformExecuteCommand, CanExecuteCommand);
            UninstallCommand = new RelayCommand(PerformUninstallCommand, CanUninstallCommand);

            RefreshImplementation();
        }

        private void RefreshSize()
        {
            try
            {
                var dirStore = new DirectoryStore();
                String path = dirStore.GetPath(Implementation.ManifestDigest);
                var dirInfo = new DirectoryInfo(path);
                Size = FormatBytes(dirInfo.GetSize());
            }
            catch (Exception)
            { Size = "Unknown"; }
        }

        public void RefreshImplementation()
        {
            var dirStore = new DirectoryStore();

            _feed.Simplify();

            Implementation foundImplementation = null;

            foreach (Implementation i in _feed.Elements.OfType<Implementation>())
            {
                Console.WriteLine("looking for : " + _feed.UriString + "-" + i.ID);

                String path = "NOT FOUND";
                try
                {
                    path = dirStore.GetPath(i.ManifestDigest);
                    foundImplementation = i;
                }
                catch (Exception)
                {}

                Console.WriteLine("FOUND = " + path);
            }

            if (foundImplementation != null)
            {
                Implementation = foundImplementation;
            }

            RefreshSize();
        }
    }
}
