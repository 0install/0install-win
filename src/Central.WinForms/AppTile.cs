// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoByte.Common;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Store;
using Icon = ZeroInstall.Model.Icon;
using SharedResources = ZeroInstall.Central.Properties.Resources;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Represents an application as a tile with buttons for launching, managing, etc..
    /// </summary>
    public sealed partial class AppTile : UserControl, IAppTile
    {
        // Static resource preload
        private static readonly string _runButtonText = SharedResources.Run;
        private static readonly Bitmap _addImage = Resources.AppAdd, _removeImage = Resources.AppRemove, _integrateImage = Resources.AppIntegrate, _modifyImage = Resources.AppModify;
        private static readonly string _addText = SharedResources.MyAppsAdd, _removeText = SharedResources.MyAppsRemove, _integrateText = SharedResources.Integrate, _modifyIntegrationText = SharedResources.ModifyIntegration;
        private static readonly string _runWithOptionsText = SharedResources.RunWithOptions, _updateText = SharedResources.Update;

        /// <summary>Apply operations machine-wide instead of just for the current user.</summary>
        private readonly bool _machineWide;

        /// <summary>The icon store used to retrieve icons specified in <see cref="Feed"/>; can be <c>null</c>.</summary>
        private readonly IIconStore? _iconStore;

        /// <inheritdoc/>
        public FeedUri InterfaceUri { get; }

        /// <inheritdoc/>
        public string AppName => labelName.Text;

        private AppStatus _status;

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AppStatus Status
        {
            get => _status;
            set
            {
                #region Sanity checks
                if (value is (< AppStatus.Candidate or > AppStatus.Integrated)) throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(AppStatus));
                if (InvokeRequired) throw new InvalidOperationException("Property set from a non UI thread.");
                #endregion

                _status = value;

                UpdateButtons();
            }
        }

        private Feed? _feed;

        /// <inheritdoc/>
        public Feed? Feed
        {
            get { return _feed; }
            set
            {
                #region Sanity checks
                if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
                #endregion

                _feed = value;
                if (value == null)
                {
                    buttonRunWithOptions.Visible = false;
                    return;
                }
                else buttonRunWithOptions.Visible = true;

                labelSummary.Text = value.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture);
                SetIcon(value.Icons.GetIcon(Icon.MimeTypePng) ?? value.Icons.GetIcon(Icon.MimeTypeIco));
            }
        }

        /// <summary>
        /// Creates a new application tile.
        /// </summary>
        /// <param name="interfaceUri">The interface URI of the application this tile represents.</param>
        /// <param name="appName">The name of the application this tile represents.</param>
        /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
        /// <param name="iconStore">The icon store used to retrieve icons specified in <see cref="Feed"/>; can be <c>null</c>.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        public AppTile(FeedUri interfaceUri, string appName, AppStatus status, IIconStore? iconStore = null, bool machineWide = false)
        {
            _machineWide = machineWide;
            _iconStore = iconStore;

            InitializeComponent();
            buttonRun.Text = buttonRun2.Text = _runButtonText;
            buttonRunWithOptions.Text = _runWithOptionsText;
            buttonUpdate.Text = _updateText;

            buttonAdd.Image = _addImage;
            buttonAdd.AccessibleName = _addText;
            toolTip.SetToolTip(buttonAdd, _addText);
            buttonRemove.Image = _removeImage;
            buttonRemove.Text = _removeText;
            buttonIntegrate.Image = _integrateImage;

            InterfaceUri = interfaceUri ?? throw new ArgumentNullException(nameof(interfaceUri));
            labelName.Text = appName ?? throw new ArgumentNullException(nameof(appName));
            labelSummary.Text = "";
            Status = status;

            CreateHandle();
        }

        private async void SetIcon(Icon? icon)
        {
            pictureBoxIcon.Image = await GetIconAsync(icon);
        }

        private static readonly SemaphoreSlim _iconSemaphore = new(initialCount: 5);

        private async Task<Image> GetIconAsync(Icon? icon)
        {
            if (icon != null && _iconStore != null)
            {
                await _iconSemaphore.WaitAsync(); // Limit number of concurrent icon downloads
                try
                {
                    return await Task.Run(() => Image.FromFile(_iconStore.GetPath(icon)));
                }
                #region Error handling
                catch (OperationCanceledException)
                {}
                catch (UriFormatException ex)
                {
                    Log.Warn(ex);
                }
                catch (WebException ex)
                {
                    Log.Warn(ex);
                }
                catch (IOException ex)
                {
                    Log.Warn($"Failed to store {icon}");
                    Log.Warn(ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warn($"Failed to store {icon}");
                    Log.Warn(ex);
                }
                catch (Exception ex)
                {
                    Log.Warn($"Failed to parse {icon}");
                    Log.Warn(ex);
                }
                #endregion
                finally
                {
                    _iconSemaphore.Release();
                }
            }

            return Resources.AppIcon; // Fallback default icon
        }

        /// <summary>
        /// Updates the visibility and icons of buttons based on the <see cref="Status"/>.
        /// </summary>
        private void UpdateButtons()
        {
            buttonAdd.Enabled = buttonAdd.Visible = (Status == AppStatus.Candidate);

            string integrateText = (Status == AppStatus.Integrated) ? _modifyIntegrationText : _integrateText;
            buttonIntegrate.AccessibleName = integrateText;
            toolTip.SetToolTip(buttonIntegrate, integrateText);
            buttonIntegrate.Image = (Status == AppStatus.Integrated) ? _modifyImage : _integrateImage;
            buttonIntegrate.Visible = (Status >= AppStatus.Added);
            buttonIntegrate.Enabled = true;
            buttonIntegrate2.Text = integrateText;
        }

        private void LinkClicked(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;
            try
            {
                ProcessUtils.Start(InterfaceUri.OriginalString);
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private async void buttonRun_Click(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;
            if (Feed != null && Feed.NeedsTerminal) new SelectCommandDialog(new(InterfaceUri, Feed)).Show(this);
            else await Program.RunCommandAsync(Run.Name, "--no-wait", InterfaceUri.ToStringRfc());
        }

        private void buttonRunWithOptions_Click(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake || Feed == null) return;
            new SelectCommandDialog(new(InterfaceUri, Feed)).Show(this);
        }

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;
            await Program.RunCommandAsync(Commands.Basic.Update.Name, InterfaceUri.ToStringRfc());
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;

            // Disable button while operation is running
            buttonAdd.Enabled = false;

            await Program.RunCommandAsync(_machineWide, AddApp.Name, InterfaceUri.ToStringRfc());
            UpdateButtons();
        }

        private async void buttonIntegrate_Click(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;

            // Disable buttons while operation is running
            buttonIntegrate.Enabled = false;

            await Program.RunCommandAsync(_machineWide, IntegrateApp.Name, InterfaceUri.ToStringRfc());
            UpdateButtons();
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;

            if (Status == AppStatus.Integrated)
                if (!Msg.YesNo(this, string.Format(SharedResources.AppRemoveConfirm, AppName), MsgSeverity.Warn))
                    return;

            // Disable buttons while operation is running
            buttonIntegrate.Enabled = false;

            await Program.RunCommandAsync(_machineWide, RemoveApp.Name, InterfaceUri.ToStringRfc());
            UpdateButtons();
        }
    }
}
