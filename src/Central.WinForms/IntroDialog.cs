// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common.Native;
using ZeroInstall.Central.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Central.WinForms
{
    public partial class IntroDialog : Form
    {
        #region Startup
        public IntroDialog()
        {
            InitializeComponent();
        }

        private void IntroDialog_Load(object sender, EventArgs e)
        {
            this.CenterOnParent();

            PlayIntro();
        }

        private void PlayIntro()
        {
            buttonReplay.Visible = buttonClose.Visible = false;
            labelVideo.Visible = true;
            tabControlApps.Visible = false;
            tabControlApps.SelectTab(tabPageCatalog);
            tileListCatalog.TextSearch.Text = "";
            labelSubtitles.Visible = false;

            SetupTiles();
            FillActions();
            ScheduleNextAction();
        }
        #endregion

        #region Event handlers
        private void timerActions_Tick(object sender, EventArgs e)
        {
            timerActions.Enabled = false;
            _actions.Dequeue().Value();
            ScheduleNextAction();
        }

        private void buttonReplay_Click(object sender, EventArgs e)
            => PlayIntro();

        private void buttonClose_Click(object sender, EventArgs e)
            => Close();
        #endregion

        //--------------------//

        #region Tiles
        private static readonly FeedUri _coolApp = new FeedUri(FeedUri.FakePrefix + "http://cool_app/");
        private static readonly FeedUri _commonApp = new FeedUri(FeedUri.FakePrefix + "http://common_app/");
        private static readonly FeedUri _otherApp = new FeedUri(FeedUri.FakePrefix + "http://other_app/");

        private void SetupTiles()
        {
            tileListCatalog.Clear();
            tileListCatalog.QueueNewTile(_coolApp, Resources.IntroCoolApp, AppStatus.Candidate).Feed =
                new Feed {Summaries = {Resources.IntroCoolAppSummary}};
            tileListCatalog.QueueNewTile(_commonApp, Resources.IntroCommonApp, AppStatus.Candidate).Feed =
                new Feed {Summaries = {Resources.IntroCommonAppSummary}};
            tileListCatalog.QueueNewTile(_otherApp, Resources.IntroOtherApp, AppStatus.Candidate).Feed =
                new Feed {Summaries = {Resources.IntroOtherAppSummary}};
            tileListCatalog.AddQueuedTiles();

            tileListMyApps.Clear();
            tileListMyApps.QueueNewTile(_coolApp, Resources.IntroCoolApp, AppStatus.Added).Feed =
                new Feed {Summaries = {Resources.IntroCoolAppSummary}};
            tileListMyApps.AddQueuedTiles();
        }
        #endregion

        #region Actions
        private TimedActionQueue _actions;

        private AppTile GetCatalogTile(FeedUri interfaceUri) => (AppTile)tileListCatalog.GetTile(interfaceUri);

        private AppTile GetMyAppsTile(FeedUri interfaceUri) => (AppTile)tileListMyApps.GetTile(interfaceUri);

        private void FillActions() => _actions = new TimedActionQueue
        {
            // Welcome
            {3000, () => PrintSubtitles(Resources.IntroSubtitlesWelcome)},
            {6000, labelSubtitles.Hide},
            // Catalog search
            {1000, () => PrintSubtitles(Resources.IntroSubtitlesCatalogSearch)},
            {3000, tabControlApps.Show},
            {2500, arrowSearch.Show},
            {500, arrowSearch.Hide},
            {500, arrowSearch.Show},
            {1500, () => TypeText(tileListCatalog.TextSearch, "C")},
            {500, () => TypeText(tileListCatalog.TextSearch, "Co")},
            {500, () => TypeText(tileListCatalog.TextSearch, "Coo")},
            {500, () => TypeText(tileListCatalog.TextSearch, "Cool")},
            {500, arrowSearch.Hide},
            {2000, labelSubtitles.Hide},
            // Run app
            {1000, () => PrintSubtitles(Resources.IntroSubtitlesRunApp)},
            {2000, () => FlashRectangle(GetCatalogTile(_coolApp).buttonRun)},
            {4000, GetCatalogTile(_coolApp).Refresh},
            {1000, labelSubtitles.Hide},
            // Add app
            {2000, () => PrintSubtitles(Resources.IntroSubtitlesAddApp)},
            {4000, () => FlashRectangle(GetCatalogTile(_coolApp).buttonAdd)},
            {2000, () => { GetCatalogTile(_coolApp).Status = AppStatus.Added; }},
            {3000, GetCatalogTile(_coolApp).Refresh},
            {1000, labelSubtitles.Hide},
            // My apps
            {2000, arrowMyApps.Show},
            {500, arrowMyApps.Hide},
            {500, arrowMyApps.Show},
            {1500, () => tabControlApps.SelectTab(tabPageAppList)},
            {1000, () => PrintSubtitles(Resources.IntroSubtitlesMyApps)},
            {4000, arrowMyApps.Hide},
            {1000, labelSubtitles.Hide},
            // Integrate app
            {1000, () => PrintSubtitles(Resources.IntroSubtitlesIntegrateApp)},
            {5000, () => FlashRectangle(GetMyAppsTile(_coolApp).buttonIntegrate)},
            {2000, () => { GetMyAppsTile(_coolApp).Status = AppStatus.Integrated; }},
            {3000, GetMyAppsTile(_coolApp).Refresh},
            {1500, labelSubtitles.Hide},
            // Thanks
            {2000, () => PrintSubtitles(Resources.IntroSubtitlesThanks)},
            {
                4000, () =>
                {
                    tabControlApps.Hide();
                    labelVideo.Hide();
                    buttonReplay.Visible = buttonClose.Visible = true;
                }
            }
        };
        #endregion

        #region Actions helpers
        private class TimedActionQueue : Queue<KeyValuePair<int, Action>>
        {
            public void Add(int time, Action action) => Enqueue(new KeyValuePair<int, Action>(time, action));
        }

        private void PrintSubtitles(string text)
        {
            labelSubtitles.Text = text;
            labelSubtitles.Visible = true;
        }

        private static void TypeText(TextBox textBox, string text)
        {
            textBox.Text = text;
            textBox.SelectionStart = text.Length;
            textBox.SelectionLength = 0;
        }

        private static void FlashRectangle(Control target)
        {
            DrawRectangle(target);
            Thread.Sleep(500);
            target.Parent.Refresh();
            Thread.Sleep(500);
            DrawRectangle(target);
        }

        private static void DrawRectangle(Control target)
        {
            using var graphics = target.Parent.CreateGraphics();
            using var pen = new Pen(Color.Red, 4);
            graphics.DrawRectangle(pen, new Rectangle(target.Location, target.Size));
        }

        private void ScheduleNextAction()
        {
            if (_actions.Count > 0)
            {
                timerActions.Interval = _actions.Peek().Key;
                timerActions.Enabled = true;
            }
        }
        #endregion
    }
}
