using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Storage;
using Common.Wpf;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using System.IO;
using System.Windows.Media;
using System.Windows.Input;
using ZeroInstall.Injector;
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
        //        return this.iconUrl;
        //    }
        //    set
        //    {
        //        this.iconUrl = value;
        //        this.NotifyPropertyChanged("IconUrl");
        //    }
        //}

        //private String iconUrl = "";
        //public String IconUrl
        //{
        //    get
        //    {
        //        return this.iconUrl;
        //    }
        //    set
        //    {
        //        this.iconUrl = value;
        //        this.NotifyPropertyChanged("IconUrl");
        //    }
        //}


        //private String name = "";
        //public String Name
        //{
        //    get
        //    {
        //        return this.name;
        //    }
        //    set
        //    {
        //        this.name = value;
        //        this.NotifyPropertyChanged("Name");
        //    }
        //}

        //private String version = "";
        //public String Version
        //{
        //    get
        //    {
        //        return this.version;
        //    }
        //    set
        //    {
        //        this.version = value;
        //        this.NotifyPropertyChanged("Version");
        //    }
        //}

        //private String publisher = "";
        //public String Publisher
        //{
        //    get
        //    {
        //        return this.publisher;
        //    }
        //    set
        //    {
        //        this.publisher = value;
        //        this.NotifyPropertyChanged("Publisher");
        //    }
        //}

        //public AppInfo(String name, String version, String publisher, String size, String iconUrl)
        //{
        //    this.Name = name;
        //    this.Version = version;
        //    this.Publisher = publisher;
        //    this.Size = size;
        //    this.IconUrl = iconUrl;
        //}

        #endregion

        public Boolean IsStateIdle
        {
            get
            {
                return this.StateAction.Length == 0;
            }
        }

        private Boolean stateVisible = false;
        public Boolean StateVisible
        {
            get
            {
                return this.stateVisible;
            }
            set
            {
                this.stateVisible = value;
                this.NotifyPropertyChanged("StateVisible");
            }
        }

        private Color stateProgressColor = Color.FromArgb(255, 1, 211, 40);
        public Color StateProgressColor
        {
            get
            {
                return this.stateProgressColor;
            }
            set
            {
                this.stateProgressColor = value;
                this.NotifyPropertyChanged("StateProgressColor");
                this.NotifyPropertyChanged("StateProgressColorBrush");
            }
        }

        public Brush StateProgressColorBrush
        {
            get
            {
                return new SolidColorBrush(this.StateProgressColor);
            }
        }


        private double stateProgressPercent = 0;
        public double StateProgressPercent
        {
            get
            {
                return this.stateProgressPercent;
            }
            set
            {
                this.stateProgressPercent = value;
                this.NotifyPropertyChanged("StateProgressPercent");
                this.NotifyPropertyChanged("StateProgressStr");
            }
        }

        public String StateProgressStr
        {
            get
            {
                if (this.StateProgressPercent > 0)
                {
                    return String.Format("{0:0}%", this.StateProgressPercent);
                }
                else
                {
                    return "Waiting";
                }
            }
        }

        private String stateAction = "";
        public String StateAction
        {
            get
            {
                return this.stateAction;
            }
            set
            {
                this.stateAction = value;
                this.NotifyPropertyChanged("StateAction");
                this.NotifyPropertyChanged("IsStateIdle");
            }
        }

        private String size = "";
        public String Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
                this.NotifyPropertyChanged("Size");
            }
        }

        private Feed feed;
        public Feed Feed
        {
            get
            {
                return this.feed;
            }
            set
            {
                this.feed = value;
                this.NotifyPropertyChanged("Feed");
            }
        }

        private Implementation implementation;
        public Implementation Implementation
        {
            get
            {
                return this.implementation;
            }
            set
            {
                this.implementation = value;
                this.NotifyPropertyChanged("Implementation");
            }
        }


        private String FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
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

        public bool CanExecuteCommand(object parameter)
        {
            return true;
        }

        public void PerformExecuteCommand(object parameter)
        {
            try
            {
                DirectoryStore dirStore = new DirectoryStore(implementationsPath);
                String path = dirStore.GetPath(this.Implementation.ManifestDigest);
                
                Process.Start(new ProcessStartInfo(path + @"\" + this.implementation.Main));

                MainWindow.TaskbarIcon.ShowBalloonTip(this.Feed.Name, "Application is starting.", BalloonIcon.Info);
            }
            catch (Exception ex)
            { 
                System.Windows.MessageBox.Show("Error executing " + this.Feed.Name + ". Details: " + ex.ToString());
            }

            //Launcher l = new Launcher(this.Implementation.ID, new Injector.Solver.Selections()
        }

        public bool CanUninstallCommand(object parameter)
        {
            return true;
        }

        public void PerformUninstallCommand(object parameter)
        {
            if (!Msg.Ask(
                App.NativeWnd,
                "Do you really want to uninstall \"" + this.Feed.Name + "\"?",
                MsgSeverity.Warning,
                "Yes\nUninstall the application",
                "No\nGo back to the list"
                ))
            {
                return;
            }

            try
            {
                DirectoryStore dirStore = new DirectoryStore(implementationsPath);
                String path = dirStore.GetPath(this.Implementation.ManifestDigest);

                Directory.Delete(path, true);

                // Delete desktop link
                String exePath = path + @"\" + this.Implementation.Main;

                Link.Update(Environment.SpecialFolder.Desktop, exePath, this.Feed.Name + ".lnk", false);


                MainWindow.Instance.UninstalledApp(this);

                MainWindow.TaskbarIcon.ShowBalloonTip(this.Feed.Name, "Application has been uninstalled.", BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error uninstalling " + this.Feed.Name + ". Details: " + ex.ToString());
            }

        }

        public AppInfo(Feed feed, Implementation implementation)
        {
            this.Feed = feed;
            this.Implementation = implementation;

            ExecuteCommand = new RelayCommand(PerformExecuteCommand, CanExecuteCommand);
            UninstallCommand = new RelayCommand(PerformUninstallCommand, CanUninstallCommand);

            RefreshImplementation();
        }

        String implementationsPath = Path.Combine(Locations.GetUserCacheDir("0install.net"), "implementations");

        private void RefreshSize()
        {
            try
            {
                DirectoryStore dirStore = new DirectoryStore(implementationsPath);
                String path = dirStore.GetPath(this.Implementation.ManifestDigest);
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                this.Size = this.FormatBytes(dirInfo.GetSize());
            }
            catch (Exception)
            { this.Size = "Unknown"; }
        }

        public void RefreshImplementation()
        {
            DirectoryStore dirStore = new DirectoryStore(implementationsPath);

            feed.Simplify();

            Implementation foundImplementation = null;

            foreach (Element el in feed.Elements.ToList())
            {
                if (el is Implementation)
                {
                    Implementation i = (Implementation)el;
                    Console.WriteLine("looking for : " + feed.UriString + "-" + i.ID);

                    String path = "NOT FOUND";
                    try
                    {
                        path = dirStore.GetPath(i.ManifestDigest);
                        foundImplementation = i;
                    }
                    catch (Exception)
                    { }

                    Console.WriteLine("FOUND = " + path);
                }
            }

            if (foundImplementation != null)
            {
                this.Implementation = foundImplementation;
            }

            RefreshSize();
        }
    }
}
