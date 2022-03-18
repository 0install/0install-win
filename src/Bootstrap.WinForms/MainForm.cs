// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;

namespace ZeroInstall
{
    /// <summary>
    /// The main GUI for the Bootstrapper.
    /// </summary>
    public sealed partial class MainForm : Form
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public MainForm(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;

            InitializeComponent();

            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (ArgumentException) // Running from network path, can't extract icon
            {}

            var embeddedConfig = EmbeddedConfig.Load();
            if (embeddedConfig.AppName != null)
            {
                Text = $"{embeddedConfig.AppName} (Zero Install)";
                labelAppName.Text = embeddedConfig.AppName;
            }

            HandleCreated += delegate
            {
                this.EnableWindowDrag();
                labelBorder.EnableWindowDrag();
                pictureBoxLogo.EnableWindowDrag();
            };
        }

        public IProgress<TaskSnapshot> GetProgressControl(string taskName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));
            #endregion

            taskControl.TaskName = taskName;
            taskControl.Visible = true;
            return taskControl;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Never allow the user to directly close the window
            e.Cancel = true;

            // Start proper cancellation instead
            Cancel();
        }

        private void buttonClose_Click(object sender, EventArgs e) => Cancel();

        /// <summary>
        /// Hides the window and then starts canceling the current process asynchronously.
        /// </summary>
        private void Cancel()
        {
            Hide();

            _cancellationTokenSource.Cancel();
        }
    }
}
