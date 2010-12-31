using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Common.Utils;
using Hardcodet.Wpf.TaskbarNotification;
using System.ComponentModel;
using Common.Wpf;
using System.Diagnostics;
using Common;
using System.Collections.ObjectModel;
using ZeroInstall.Store.Feed;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using System.Net;

namespace ZeroInstall.Central.Wpf
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public static MainWindow Instance;

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            Icon = ResHelper.GetImage("0install-wpf", "Icon.ico");

            Instance = this;

            Loaded += MainWindow_Loaded;
        }
        #endregion

        private Boolean IsFiltering
        {
            get
            {
                return tbSearch.Text != "Search" && tbSearch.Text != "";
            }
        }

        public ObservableCollection<AppInfo> AppInfosFiltered
        {
            get
            {
                var appInfos = (ObservableCollection<AppInfo>)GetValue(AppInfosProperty);

                if(IsFiltering)
                {
                    appInfos = new ObservableCollection<AppInfo>(appInfos.Where(appInfo => appInfo.Feed.Name.ToLower().Contains(tbSearch.Text.ToLower())));
                }
                return appInfos;
            }
            set { SetValue(AppInfosProperty, value); }
        }

        public ObservableCollection<AppInfo> AppInfos
        {
            get { return (ObservableCollection<AppInfo>)GetValue(AppInfosProperty); }
            set { SetValue(AppInfosProperty, value); }
        }

        public static readonly DependencyProperty AppInfosProperty =
            DependencyProperty.Register("AppInfos", typeof(ObservableCollection<AppInfo>), typeof(MainWindow), new UIPropertyMetadata(null));


        void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            NotifyPropertyChanged("AppInfosFiltered");
        }

        readonly InstallManager _installManager = new InstallManager();

        public InstallManager InstallManager
        {
            get
            {
                return _installManager;
            }
        }

        [CLSCompliant(false)]
        public static TaskbarIcon TaskbarIcon;

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Test

            App.NativeWnd = Application.Current.MainWindow.GetNativeWnd();

            tbSearch.TextChanged += tbSearch_TextChanged;
            tbSearch.GotKeyboardFocus += tbSearch_GotKeyboardFocus;
            tbSearch.LostKeyboardFocus += tbSearch_LostKeyboardFocus;

            //try
            //{
            //    TcpServerChannel channel = new TcpServerChannel(1002);
            //    ChannelServices.RegisterChannel(channel, false);
            //    RemotingConfiguration.RegisterWellKnownServiceType(
            //    typeof(RemoteServerObject),
            //    "Test",
            //    WellKnownObjectMode.SingleCall);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            AppInfos = new ObservableCollection<AppInfo>();

            // Load App List
            IEnumerable<Feed> interfaces = FeedCacheProvider.Default.GetAll();
            var dirStore = new DirectoryStore();

            //String path = dirStore.GetPath(new ManifestDigest("sha1new=f989434c13d00773910976724af2a8b2906138cc"));

            foreach (Feed feed in interfaces)
            {
                feed.Simplify();

                Implementation foundImplementation = null;

                foreach (Implementation i in feed.Elements.OfType<Implementation>())
                {
                    Console.WriteLine("looking for : " + feed.UriString + "-" + i.ID);

                    String path2 = "NOT FOUND";
                    try
                    {
                        path2 = dirStore.GetPath(i.ManifestDigest);
                        foundImplementation = i;
                    }
                    catch (Exception)
                    {}

                    Console.WriteLine("FOUND = " + path2);
                }

                if (foundImplementation != null)
                {
                    var appInfo = new AppInfo(feed, foundImplementation);
                    AppInfos.Add(appInfo);
                }
            }

            dgApplications.DataContext = this;


            //AppInfos.Add(new AppInfo("Firefox", "3.6.8", "Mozilla Foundation", "10.0 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-firefox.png"));
            //AppInfos.Add(new AppInfo("Seamonkey", "2.01", "Mozilla Foundation", "36.2 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-seamonkey.png"));
            //AppInfos.Add(new AppInfo("Thunderbird", "3.01", "Mozilla Foundation", "16.2 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-thunderbird.png"));
            //AppInfos.Add(new AppInfo("Sunbird", "1.01", "Mozilla Foundation", "6.2 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-sunbird.png"));


            Closing += MainWindow_Closing;

            var icon = new TaskbarIcon();
            TaskbarIcon = icon;

            icon.IconSource = ResHelper.GetImage("0install-wpf", "Icon.ico");

            //icon.IconSource = new BitmapImage(new Uri("pack://application:,,,/ZeroInstall_wpf;component/Icon.ico"));
            icon.ToolTip = "Zero Install - Use F12 or middle mouse button to show apps.";
            icon.ShowBalloonTip("Zero Install 1.0.0", "Use F12 or middle mouse button to show apps.", BalloonIcon.Info);

            icon.TrayLeftMouseUp += icon_TrayLeftMouseUp;

            wbStore.Navigate("http://0install.de/appstore/?client=central&lang=de");

            wbStore.Navigating += wbStore_Navigating;


            bTabApplications.MouseLeftButtonDown += bTabApplications_MouseLeftButtonDown;
            bTabStore.MouseLeftButtonDown += bTabStore_MouseLeftButtonDown;
            bTabSettings.MouseLeftButtonDown += bTabSettings_MouseLeftButtonDown;
            bTabHelp.MouseLeftButtonDown += bTabHelp_MouseLeftButtonDown;


            _viewPanels.Add(gConApplications);
            _viewPanels.Add(gConStore);
            _viewPanels.Add(gConSettings);
            _viewPanels.Add(gConHelp);

            _viewBorders.Add(bTabApplications);
            _viewBorders.Add(bTabStore);
            _viewBorders.Add(bTabSettings);
            _viewBorders.Add(bTabHelp);

            ShowView(0);

        }

        void tbSearch_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (tbSearch.Text == "Search")
            //{
            //    tbSearch.Text = "";
            //}
        }

        void tbSearch_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (tbSearch.Text == "Search")
            {
                tbSearch.Text = "";
            }
        }

        readonly List<Grid> _viewPanels = new List<Grid>();
        readonly List<Border> _viewBorders = new List<Border>();

        private void ShowView(int num)
        {
            _viewPanels.ForEach(v => v.Visibility = Visibility.Collapsed);
            _viewBorders.ForEach(b => b.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));
            _viewBorders.ForEach(b => b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));

            _viewPanels[num].Visibility = Visibility.Visible;
            _viewBorders[num].Background = new SolidColorBrush(Color.FromArgb(255, 237, 237, 237));
            _viewBorders[num].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
        }

        void bTabApplications_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowView(0);
        }
        void bTabStore_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowView(1);
        }
        void bTabSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowView(2);
        }
        void bTabHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowView(3);
        }

        /// <summary>A URL postfix that indicates that the URL points to an installable Zero Install feed.</summary>
        private const string UrlPostfixFeed = "#0install-feed";

        /// <summary>A URL postfix that indicates that the URL should be opened in an external browser.</summary>
        private const string UrlPostfixBrowser = "#external-browser";

        void wbStore_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            switch (e.Uri.Fragment)
            {
                case UrlPostfixFeed:
                    e.Cancel = true;

                    string feedUri = e.Uri.AbsoluteUri.Replace(UrlPostfixFeed, "");

                    // ToDo: Display more details about the feed
                    if (Msg.Ask(this.GetNativeWnd(), "Do you want to install this application?\n" + feedUri, MsgSeverity.Info, "Yes\nInstall the application", "No\nGo back to the list"))
                    {
                        InstallTest(feedUri);

          
                        //App.LaunchHelperApp(this, "0launch-win", "--no-wait " + feedUri);
                        //Close();
                    }
                    break;

                case UrlPostfixBrowser:
                    e.Cancel = true;

                    // Use the system's default web browser to open the URL
                    Process.Start(e.Uri.AbsoluteUri.Replace(UrlPostfixBrowser, ""));
                    break;
            }
        }

        private void InstallTest(string feedUri)
        {
            var c = new WebClient();
            c.DownloadFileCompleted += c_DownloadFileCompleted;
            String feedPath = FileUtils.GetTempFile("0install");
            c.DownloadFileAsync(new Uri(feedUri, UriKind.Absolute), feedPath, feedPath);

        }

        void c_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //List<String> args = (List<String>)e.UserState;
            String feedPath = e.UserState.ToString();
            //String feedUri = args[1];

            Feed feed = Feed.Load(feedPath);
            
            var appInfo = new AppInfo(feed, null);
            AppInfos.Add(appInfo);

            _installManager.Install(appInfo);
        }

        void icon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (Visibility != Visibility.Visible)
            {
                Show();
            }
            else
            {
                Activate();
            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //e.Cancel = true;
            //Hide();
        }

        private void bAddFromUrl_Click(object sender, RoutedEventArgs e)
        {
            App.LaunchHelperApp(this, "0launch-win.exe");

            //InstallWindow w = new InstallWindow();
            //w.ShowDialog();
        }

        internal void UninstalledApp(AppInfo appInfo)
        {
            AppInfos.Remove(appInfo);
        }
    }
}
