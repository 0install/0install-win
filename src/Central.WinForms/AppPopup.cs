// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Windows.Forms;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using SharedResources = ZeroInstall.Central.Properties.Resources;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Popup window for adding/removing/integrating an app.
    /// </summary>
    public sealed partial class AppPopup : Form
    {
        private readonly FeedUri _interfaceUri;
        private readonly bool _machineWide;

        /// <summary>
        /// Creates a new app popup.
        /// </summary>
        /// <param name="interfaceUri">The interface URI of the application.</param>
        /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        public AppPopup(FeedUri interfaceUri, AppStatus status, bool machineWide)
        {
            InitializeComponent();
            Deactivate += delegate { Close(); };

            _interfaceUri = interfaceUri;
            _machineWide = machineWide;
            SetStatus(status);
        }

        /// <summary>
        /// Shows the popup at the screen coordinates of the specified <paramref name="control"/>.
        /// </summary>
        public void ShowAt(Control control)
        {
            Location = control.PointToScreen(new(control.Width - Width, 0));
            Show(control);
        }

        private void SetStatus(AppStatus status)
        {
            switch (status)
            {
                case AppStatus.Candidate:
                    AddApp();
                    iconStatus.Image = Resources.AppCandidate;
                    break;

                case AppStatus.Added:
                    iconStatus.Image = Resources.AppAdded;
                    labelStatus.Text = SharedResources.MyAppsAdded;
                    buttonIntegrate.Text = SharedResources.Integrate;
                    buttonRemove.Text = SharedResources.Remove;
                    ShowButtons();
                    break;

                case AppStatus.Integrated:
                    iconStatus.Image = Resources.AppIntegrated;
                    labelStatus.Text = SharedResources.MyAppsAddedAndIntegrate;
                    buttonIntegrate.Text = SharedResources.ModifyIntegration;
                    buttonRemove.Text = SharedResources.Remove;
                    ShowButtons();
                    break;
            }
        }

        private void ShowButtons()
        {
            buttonIntegrate.Visible = buttonRemove.Visible = true;
            buttonIntegrate.Focus();
        }

        private async void AddApp()
        {
            labelStatus.Text = SharedResources.Working;

            var exitCode = await Program.RunCommandAsync(_machineWide, Commands.Desktop.AddApp.Name, "--background", _interfaceUri.ToStringRfc());
            if (exitCode == ExitCode.OK) SetStatus(AppStatus.Added);
            else Close();
        }

        private async void buttonIntegrate_Click(object sender, EventArgs e)
        {
            labelStatus.Text = SharedResources.Working;
            Enabled = false;
            await Program.RunCommandAsync(_machineWide, IntegrateApp.Name, _interfaceUri.ToStringRfc());
            Close();
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            labelStatus.Text = SharedResources.Working;
            Enabled = false;
            await Program.RunCommandAsync(_machineWide, RemoveApp.Name, _interfaceUri.ToStringRfc());
            Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
