using System;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms
{
    public partial class OverlayBindingControl : UserControl
    {
        /// <summary>
        /// The <see cref="OverlayBinding" /> to be displayed and modified by this control.
        /// </summary>
        private OverlayBinding _overlayBinding = new OverlayBinding();

        public OverlayBindingControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The <see cref="PackageImplementation" /> to be displayed and modified by this control. If <see langword="null"/>, the control resets.
        /// </summary>
        public OverlayBinding OverlayBinding
        {
            get { 
                return _overlayBinding;
            }
            set
            {
                _overlayBinding = value ?? new OverlayBinding();
                UpdateControl();
            }
        }

        /// <summary>
        /// Clear all elements in this control.
        /// </summary>
        private void ClearControl()
        {
            hintTextBoxSrc.Text = String.Empty;
            hintTextBoxMountPoint.Text = String.Empty;
        }

        /// <summary>
        /// Clear all elements in this control and set their values from <see cref="_overlayBinding"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            hintTextBoxSrc.Text = _overlayBinding.Source;
            hintTextBoxMountPoint.Text = _overlayBinding.MountPoint;
        }

        /// <summary>
        /// Sets _overlayBinding.Source if hintTextBoxMountPoint.Text" is a valid source.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxSrc_TextChanged(object sender, EventArgs e)
        {
            // TODO string auf glütigen Pfad überprüfen
            _overlayBinding.Source = String.IsNullOrEmpty(hintTextBoxSrc.Text) ? String.Empty : hintTextBoxSrc.Text;
        }

        /// <summary>
        /// Sets _overlayBinding.MountPoint if hintTextBoxMountPoint.Text is a valid mount point.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxMountPoint_TextChanged(object sender, EventArgs e)
        {
            // TODO string auf gültigen Pfad überprüfen
            _overlayBinding.MountPoint = String.IsNullOrEmpty(hintTextBoxMountPoint.Text) ? String.Empty : hintTextBoxMountPoint.Text;
        }
    }
}
