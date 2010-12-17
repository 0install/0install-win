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

    public class TempFeedHandler : IFeedHandler
    {
        public bool AcceptNewKey(string information)
        {
            return true;
        }
    }


    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
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
            this.Icon = ResHelper.GetImage("0install-wpf", "Icon.ico");

            Instance = this;

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }
        #endregion

        private Boolean IsFiltering
        {
            get
            {
                return this.tbSearch.Text != "Search" && this.tbSearch.Text != "";
            }
        }

        public ObservableCollection<AppInfo> AppInfosFiltered
        {
            get
            {
                ObservableCollection<AppInfo> appInfos = (ObservableCollection<AppInfo>)GetValue(AppInfosProperty);

                if(this.IsFiltering)
                {
                    appInfos = new ObservableCollection<AppInfo>(appInfos.Where(appInfo => appInfo.Feed.Name.ToLower().Contains(this.tbSearch.Text.ToLower())));
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
            this.NotifyPropertyChanged("AppInfosFiltered");
        }

        InstallManager installManager = new InstallManager();

        public InstallManager InstallManager
        {
            get
            {
                return this.installManager;
            }
        }

        [CLSCompliant(false)]
        public static TaskbarIcon TaskbarIcon;

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Test

            App.NativeWnd = App.Current.MainWindow.GetNativeWnd();

            this.tbSearch.TextChanged += new TextChangedEventHandler(tbSearch_TextChanged);
            this.tbSearch.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(tbSearch_GotKeyboardFocus);
            this.tbSearch.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(tbSearch_LostKeyboardFocus);

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

            this.AppInfos = new ObservableCollection<AppInfo>();

            // Load App List
            IEnumerable<Feed> interfaces = new FeedCache().GetAll();
            DirectoryStore dirStore = new DirectoryStore();

            //String path = dirStore.GetPath(new ManifestDigest("sha1new=f989434c13d00773910976724af2a8b2906138cc"));

            foreach (Feed feed in interfaces)
            {
                feed.Simplify();

                Implementation foundImplementation = null;

                foreach (Element el in feed.Elements.ToList())
                {
                    if (el is Implementation)
                    {
                        Implementation i = (Implementation)el;
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
                }

                if (foundImplementation != null)
                {
                    AppInfo appInfo = new AppInfo(feed, foundImplementation);
                    this.AppInfos.Add(appInfo);
                }
            }

            this.dgApplications.DataContext = this;


            //this.AppInfos.Add(new AppInfo("Firefox", "3.6.8", "Mozilla Foundation", "10.0 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-firefox.png"));
            //this.AppInfos.Add(new AppInfo("Seamonkey", "2.01", "Mozilla Foundation", "36.2 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-seamonkey.png"));
            //this.AppInfos.Add(new AppInfo("Thunderbird", "3.01", "Mozilla Foundation", "16.2 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-thunderbird.png"));
            //this.AppInfos.Add(new AppInfo("Sunbird", "1.01", "Mozilla Foundation", "6.2 MB", "pack://application:,,,/ZeroInstall_wpf;component/Resources/logo-sunbird.png"));


            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            TaskbarIcon icon = new TaskbarIcon();
            MainWindow.TaskbarIcon = icon;

            icon.IconSource = ResHelper.GetImage("0install-wpf", "Icon.ico");

            //icon.IconSource = new BitmapImage(new Uri("pack://application:,,,/ZeroInstall_wpf;component/Icon.ico"));
            icon.ToolTip = "Zero Install - Use F12 or middle mouse button to show apps.";
            icon.ShowBalloonTip("Zero Install 1.0.0", "Use F12 or middle mouse button to show apps.", BalloonIcon.Info);

            icon.TrayLeftMouseUp += new RoutedEventHandler(icon_TrayLeftMouseUp);

            this.wbStore.Navigate("http://0install.de/appstore/?client=central&lang=de");

            this.wbStore.Navigating += new NavigatingCancelEventHandler(wbStore_Navigating);


            this.bTabApplications.MouseLeftButtonDown += new MouseButtonEventHandler(bTabApplications_MouseLeftButtonDown);
            this.bTabStore.MouseLeftButtonDown += new MouseButtonEventHandler(bTabStore_MouseLeftButtonDown);
            this.bTabSettings.MouseLeftButtonDown += new MouseButtonEventHandler(bTabSettings_MouseLeftButtonDown);
            this.bTabHelp.MouseLeftButtonDown += new MouseButtonEventHandler(bTabHelp_MouseLeftButtonDown);


            viewPanels.Add(this.gConApplications);
            viewPanels.Add(this.gConStore);
            viewPanels.Add(this.gConSettings);
            viewPanels.Add(this.gConHelp);

            viewBorders.Add(this.bTabApplications);
            viewBorders.Add(this.bTabStore);
            viewBorders.Add(this.bTabSettings);
            viewBorders.Add(this.bTabHelp);

            this.ShowView(0);

        }

        void tbSearch_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (this.tbSearch.Text == "Search")
            //{
            //    this.tbSearch.Text = "";
            //}
        }

        void tbSearch_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (this.tbSearch.Text == "Search")
            {
                this.tbSearch.Text = "";
            }
        }

        List<Grid> viewPanels = new List<Grid>();
        List<Border> viewBorders = new List<Border>();

        private void ShowView(int num)
        {
            viewPanels.ForEach(v => v.Visibility = System.Windows.Visibility.Collapsed);
            viewBorders.ForEach(b => b.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));
            viewBorders.ForEach(b => b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));

            viewPanels[num].Visibility = System.Windows.Visibility.Visible;
            viewBorders[num].Background = new SolidColorBrush(Color.FromArgb(255, 237, 237, 237));
            viewBorders[num].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
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
                    if (Msg.Ask(this.GetNativeWnd(), "Do you want to install this application?\n" + feedUri, MsgSeverity.Information, "Yes\nInstall the application", "No\nGo back to the list"))
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
            WebClient c = new WebClient();
            c.DownloadFileCompleted += new AsyncCompletedEventHandler(c_DownloadFileCompleted);
            String feedPath = FileUtils.GetTempFile("0install");
            c.DownloadFileAsync(new Uri(feedUri, UriKind.Absolute), feedPath, feedPath);

        }

        void c_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //List<String> args = (List<String>)e.UserState;
            String feedPath = e.UserState.ToString();
            //String feedUri = args[1];

            Feed feed = Feed.Load(feedPath);
            
            AppInfo appInfo = new AppInfo(feed, null);
            this.AppInfos.Add(appInfo);

            this.installManager.Install(appInfo);
        }

        void icon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (this.Visibility != System.Windows.Visibility.Visible)
            {
                this.Show();
            }
            else
            {
                this.Activate();
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
            //this.Hide();
        }

        private void bAddFromUrl_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.LaunchHelperApp(this, "0launch-win.exe");

            //InstallWindow w = new InstallWindow();
            //w.ShowDialog();
        }

        internal void UninstalledApp(AppInfo appInfo)
        {
            this.AppInfos.Remove(appInfo);
        }
    }
}
