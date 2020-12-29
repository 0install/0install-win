// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoByte.Common.Native;
using ZeroInstall.Central.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Central.WinForms
{
    public partial class IntroDialog : Form
    {
        private static readonly FeedUri _coolApp = new(FeedUri.FakePrefix + "http://cool_app/");
        private static readonly FeedUri _commonApp = new(FeedUri.FakePrefix + "http://common_app/");
        private static readonly FeedUri _otherApp = new(FeedUri.FakePrefix + "http://other_app/");

        public IntroDialog()
        {
            InitializeComponent();
        }

        private async void IntroDialog_Load(object sender, EventArgs e)
        {
            this.CenterOnParent();
            await Play();
        }

        private async void buttonReplay_Click(object sender, EventArgs e)
            => await Play();

        private void buttonClose_Click(object sender, EventArgs e)
            => Close();

        private async Task Play()
        {
            buttonReplay.Visible = buttonClose.Visible = false;
            labelVideo.Visible = true;
            tabControlApps.Visible = false;
            tabControlApps.SelectTab(tabPageCatalog);
            tileListCatalog.TextSearch.Text = "";
            labelSubtitles.Visible = false;

            SetupTiles();

            await PlayWelcome();
            await PlayCatalogSearch();
            await PlayRunApp();
            await PlayAddApp();
            await PlayMyApps();
            await PlayIntegrateApp();
            await PlayThanks();

            tabControlApps.Hide();
            labelVideo.Hide();
            buttonReplay.Visible = buttonClose.Visible = true;
        }

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

        private async Task PlayWelcome()
        {
            await Task.Delay(3000);
            PrintSubtitles(Resources.IntroSubtitlesWelcome);

            await Task.Delay(6000);
            labelSubtitles.Hide();
        }

        private async Task PlayCatalogSearch()
        {
            await Task.Delay(1000);
            PrintSubtitles(Resources.IntroSubtitlesCatalogSearch);

            await Task.Delay(3000);
            tabControlApps.Show();

            await Task.Delay(2500);
            arrowSearch.Show();

            await Task.Delay(500);
            arrowSearch.Hide();

            await Task.Delay(500);
            arrowSearch.Show();

            await Task.Delay(1500);
            await TypeTextAsync(tileListCatalog.TextSearch, "Cool");
            arrowSearch.Hide();

            await Task.Delay(2000);
            labelSubtitles.Hide();
        }

        private async Task PlayRunApp()
        {
            await Task.Delay(1000);
            PrintSubtitles(Resources.IntroSubtitlesRunApp);

            await Task.Delay(2000);
            await FlashRectangleAsync(GetCatalogTile(_coolApp).buttonRun);

            await Task.Delay(4000);
            GetCatalogTile(_coolApp).Refresh();

            await Task.Delay(1000);
            labelSubtitles.Hide();
        }

        private async Task PlayAddApp()
        {
            await Task.Delay(2000);
            PrintSubtitles(Resources.IntroSubtitlesAddApp);

            await Task.Delay(4000);
            await FlashRectangleAsync(GetCatalogTile(_coolApp).buttonIntegrate);

            await Task.Delay(2000);
            GetCatalogTile(_coolApp).Status = AppStatus.Added;

            await Task.Delay(3000);
            GetCatalogTile(_coolApp).Refresh();

            await Task.Delay(1000);
            labelSubtitles.Hide();
        }

        private async Task PlayMyApps()
        {
            await Task.Delay(2000);
            arrowMyApps.Show();

            await Task.Delay(500);
            arrowMyApps.Hide();

            await Task.Delay(500);
            arrowMyApps.Show();

            await Task.Delay(1500);
            tabControlApps.SelectTab(tabPageAppList);

            await Task.Delay(1000);
            PrintSubtitles(Resources.IntroSubtitlesMyApps);

            await Task.Delay(4000);
            arrowMyApps.Hide();

            await Task.Delay(1000);
            labelSubtitles.Hide();
        }

        private async Task PlayIntegrateApp()
        {
            await Task.Delay(1000);
            PrintSubtitles(Resources.IntroSubtitlesIntegrateApp);

            await Task.Delay(5000);
            await FlashRectangleAsync(GetMyAppsTile(_coolApp).buttonIntegrate);

            await Task.Delay(2000);
            GetMyAppsTile(_coolApp).Status = AppStatus.Integrated;

            await Task.Delay(3000);
            GetMyAppsTile(_coolApp).Refresh();

            await Task.Delay(1500);
            labelSubtitles.Hide();
        }

        private async Task PlayThanks()
        {
            await Task.Delay(2000);
            PrintSubtitles(Resources.IntroSubtitlesThanks);
            await Task.Delay(4000);
        }

        private AppTile GetCatalogTile(FeedUri interfaceUri)
            => (AppTile)tileListCatalog.GetTile(interfaceUri) ?? throw new InvalidOperationException();

        private AppTile GetMyAppsTile(FeedUri interfaceUri)
            => (AppTile)tileListMyApps.GetTile(interfaceUri) ?? throw new InvalidOperationException();

        private void PrintSubtitles(string text)
        {
            labelSubtitles.Text = text;
            labelSubtitles.Visible = true;
        }

        private static async Task TypeTextAsync(TextBox textBox, string text)
        {
            for (int i = 1; i <= text.Length; i++)
            {
                textBox.Text = text.Substring(0, i);
                textBox.SelectionStart = i;
                textBox.SelectionLength = 0;
                await Task.Delay(500);
            }
        }

        private static async Task FlashRectangleAsync(Control target)
        {
            DrawRectangle(target);
            await Task.Delay(500);
            target.Parent?.Refresh();
            await Task.Delay(500);
            DrawRectangle(target);
        }

        private static void DrawRectangle(Control target)
        {
            using var graphics = target.Parent?.CreateGraphics();
            using var pen = new Pen(Color.Red, 4);
            graphics?.DrawRectangle(pen, new Rectangle(target.Location, target.Size));
        }
    }
}
