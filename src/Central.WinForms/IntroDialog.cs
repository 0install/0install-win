/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Central.WinForms
{
    public partial class IntroDialog : Form
    {
        #region Startup
        public IntroDialog()
        {
            InitializeComponent();
            Load += delegate { PlayIntro(); };
        }

        private void PlayIntro()
        {
            buttonReplay.Visible = buttonClose.Visible = false;
            labelVideo.Visible = true;
            tabControlApps.Visible = false;
            tabControlApps.SelectTab(tabPageCatalog);
            catalogList.TextSearch.Text = "";
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
        {
            PlayIntro();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        //--------------------//

        #region Tiles
        private void SetupTiles()
        {
            catalogList.Clear();
            catalogList.QueueNewTile(false, "fake:cool_app", Resources.IntroCoolApp, AppStatus.Candidate).Feed =
                new Feed {Summaries = {Resources.IntroCoolAppSummary}};
            catalogList.QueueNewTile(false, "fake:common_app", Resources.IntroCommonApp, AppStatus.Candidate).Feed =
                new Feed {Summaries = {Resources.IntroCommonAppSummary}};
            catalogList.QueueNewTile(false, "fake:other_app", Resources.IntroOtherApp, AppStatus.Candidate).Feed =
                new Feed {Summaries = {Resources.IntroOtherAppSummary}};
            catalogList.AddQueuedTiles();

            appList.Clear();
            appList.QueueNewTile(false, "fake:cool_app", Resources.IntroCoolApp, AppStatus.Added).Feed =
                new Feed {Summaries = {Resources.IntroCoolAppSummary}};
            appList.AddQueuedTiles();
        }
        #endregion

        #region Actions
        private TimedActionQueue _actions;

        private void FillActions()
        {
            _actions = new TimedActionQueue
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
                {1500, () => TypeText(catalogList.TextSearch, "C")},
                {500, () => TypeText(catalogList.TextSearch, "Co")},
                {500, () => TypeText(catalogList.TextSearch, "Coo")},
                {500, () => TypeText(catalogList.TextSearch, "Cool")},
                {500, arrowSearch.Hide},
                {2000, labelSubtitles.Hide},
                // Run app
                {1000, () => PrintSubtitles(Resources.IntroSubtitlesRunApp)},
                {2000, () => FlashRectangle(catalogList.GetTile("fake:cool_app").buttonRun)},
                {4000, catalogList.GetTile("fake:cool_app").Refresh},
                {1000, labelSubtitles.Hide},
                // Add app
                {2000, () => PrintSubtitles(Resources.IntroSubtitlesAddApp)},
                {4000, () => FlashRectangle(catalogList.GetTile("fake:cool_app").buttonAdd)},
                {2000, () => { catalogList.GetTile("fake:cool_app").Status = AppStatus.Added; }},
                {3000, catalogList.GetTile("fake:cool_app").Refresh},
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
                {5000, () => FlashRectangle(appList.GetTile("fake:cool_app").buttonIntegrate)},
                {2000, () => { appList.GetTile("fake:cool_app").Status = AppStatus.Integrated; }},
                {3000, appList.GetTile("fake:cool_app").Refresh},
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
        }
        #endregion

        #region Actions helpers
        private class TimedActionQueue : Queue<KeyValuePair<int, Action>>
        {
            public void Add(int time, Action action)
            {
                Enqueue(new KeyValuePair<int, Action>(time, action));
            }
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
            using (var graphics = target.Parent.CreateGraphics())
            using (var pen = new Pen(Color.Red, 4))
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
