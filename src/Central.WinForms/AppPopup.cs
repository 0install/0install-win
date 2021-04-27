// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Windows.Forms;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Popup window for adding/removing/integrating an app.
    /// </summary>
    public sealed partial class AppPopup : Form
    {
        private readonly FeedUri _interfaceUri;
        private readonly bool _machineWide;
        private AppStatus _status;

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
            _status = status;

            HandleCreated += delegate { RefreshStatus(); };
        }

        /// <summary>
        /// Shows the popup at the screen coordinates of the specified <paramref name="control"/>.
        /// </summary>
        public void ShowAt(Control control)
        {
            Location = control.PointToScreen(new(control.Width - Width, 0));
            Show(control);
        }

        private void RefreshStatus()
        {
            switch (_status)
            {
                case AppStatus.Candidate:
                    AddApp();
                    iconStatus.Image = AppResources.CandidateImage;
                    break;

                case AppStatus.Added:
                    iconStatus.Image = AppResources.AddedImage;
                    labelStatus.Text = AppResources.AddedText;
                    buttonIntegrate.Text = AppResources.IntegrateText;
                    buttonRemove.Text = AppResources.RemoveText;
                    ShowButtons();
                    break;

                case AppStatus.Integrated:
                    iconStatus.Image = AppResources.IntegratedImage;
                    labelStatus.Text = AppResources.IntegratedText;
                    buttonIntegrate.Text = AppResources.ModifyText;
                    buttonRemove.Text = AppResources.RemoveText;
                    ShowButtons();
                    break;
            }
        }

        private void ShowButtons()
        {
            buttonIntegrate.Image = AppResources.IntegratedImage;
            buttonRemove.Image = AppResources.CandidateImage;
            buttonIntegrate.Visible = buttonRemove.Visible = true;
            buttonIntegrate.Focus();
        }

        private async void AddApp()
        {
            labelStatus.Text = AppResources.Working;

            var exitCode = await Program.RunCommandAsync(_machineWide, Commands.Desktop.AddApp.Name, "--background", _interfaceUri.ToStringRfc());
            if (exitCode == ExitCode.OK)
            {
                _status = AppStatus.Added;
                RefreshStatus();
            }
            else Close();
        }

        private async void buttonIntegrate_Click(object sender, EventArgs e)
        {
            labelStatus.Text = AppResources.Working;
            Enabled = false;
            await Program.RunCommandAsync(_machineWide, IntegrateApp.Name, _interfaceUri.ToStringRfc());
            Close();
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            labelStatus.Text = AppResources.Working;
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
