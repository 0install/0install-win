// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Threading;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Commands.Basic;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Store.Icons;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Represents an application as a tile with buttons for launching, managing, etc..
    /// </summary>
    public sealed partial class AppTile : UserControl
    {
        /// <summary>Apply operations machine-wide instead of just for the current user.</summary>
        private readonly bool _machineWide;

        /// <summary>The icon store used to retrieve icons specified in <see cref="Feed"/>; can be <c>null</c>.</summary>
        private readonly IIconStore? _iconStore;

        /// <summary>
        /// The interface URI of the application this tile represents.
        /// </summary>
        public FeedUri InterfaceUri { get; }

        /// <summary>
        /// The name of the application this tile represents.
        /// </summary>
        public string AppName => labelName.Text;

        private AppTileStatus _status;

        /// <summary>
        /// Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.
        /// </summary>
        /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AppTileStatus Status
        {
            get => _status;
            set
            {
                if (InvokeRequired) throw new InvalidOperationException("Property set from a non UI thread.");
                _status = value;
                RefreshStatus();
            }
        }

        private Feed? _feed;

        /// <summary>
        /// A <see cref="Feed"/> from which the tile extracts relevant application metadata such as summaries and icons.
        /// </summary>
        /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
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
                SetIcon(
                    value.Icons.GetIcon(Model.Icon.MimeTypePng)
                 ?? value.Icons.GetIcon(Model.Icon.MimeTypeIco));
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
        public AppTile(FeedUri interfaceUri, string appName, AppTileStatus status, IIconStore? iconStore = null, bool machineWide = false)
        {
            _machineWide = machineWide;
            _iconStore = iconStore;

            InitializeComponent();
            buttonRun.Text = buttonRun2.Text = AppResources.RunText;
            buttonRunWithOptions.Text = AppResources.RunWithOptionsText;
            buttonUpdate.Text = AppResources.UpdateText;

            InterfaceUri = interfaceUri ?? throw new ArgumentNullException(nameof(interfaceUri));
            labelName.Text = appName ?? throw new ArgumentNullException(nameof(appName));
            labelSummary.Text = "";
            _status = status;

            HandleCreated += delegate { RefreshStatus(); };

            CreateHandle();
        }

        private void RefreshStatus()
        {
            (string text, var image) = (_status switch
            {
                AppTileStatus.Candidate => (AppResources.CandidateText, AppResources.CandidateImage),
                AppTileStatus.Added => (AppResources.AddedText, AppResources.AddedImage),
                AppTileStatus.Integrated => (AppResources.IntegrateText, AppResources.IntegratedImage),
                _ => throw new InvalidOperationException()
            });

            buttonIntegrate.AccessibleName = text;
            buttonIntegrate.Image = image.Get(this.GetDpiScale());
            toolTip.SetToolTip(buttonIntegrate, text);
        }

        private static readonly SemaphoreSlim _iconSemaphore = new(initialCount: 5);

        private static readonly JobQueue _iconUpdates = NewJobQueue();

        private static JobQueue NewJobQueue()
        {
            var cts = new CancellationTokenSource();
            Application.ApplicationExit += delegate { cts.Cancel(); };
            return new(cts.Token);
        }

        private async void SetIcon(Model.Icon? icon)
        {
            if (icon == null || _iconStore == null)
            {
                pictureBoxIcon.Image = ImageResources.AppIcon;
                return;
            }

            await _iconSemaphore.WaitAsync();
            try
            {
                bool stale = false;
                string path = await Task.Run(() => _iconStore.Get(icon, out stale));

                if (stale)
                {
                    // Copy icon into memory to avoid conflicts with background update
                    pictureBoxIcon.Image = await Task.Run(() => Image.FromStream(new MemoryStream(File.ReadAllBytes(path))));

                    _iconUpdates.Enqueue(() =>
                    {
                        try
                        {
                            string newPath = _iconStore.GetFresh(icon);
                            this.Invoke(() => pictureBoxIcon.LoadAsync(newPath));
                        }
                        catch (OperationCanceledException)
                        {}
                        catch (InvalidOperationException) // AppTile already disposed
                        {}
                        catch (Exception ex)
                        {
                            Log.Warn(ex);
                        }
                    });
                }
                else pictureBoxIcon.LoadAsync(path);
            }
            catch (OperationCanceledException)
            {}
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            finally
            {
                _iconSemaphore.Release();
            }
        }

        private void LinkClicked(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;
            try
            {
                Process.Start(InterfaceUri.OriginalString);
            }
            #region Error handling
            catch (Exception ex)
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

        private void buttonIntegrate_Click(object sender, EventArgs e)
        {
            if (InterfaceUri.IsFake) return;
            new AppPopup(InterfaceUri, Status, _machineWide)
               .ShowAt(buttonIntegrate);
        }
    }
}
