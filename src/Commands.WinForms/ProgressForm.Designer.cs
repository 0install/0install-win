using System.Diagnostics.CodeAnalysis;

namespace ZeroInstall.Commands.WinForms
{
    partial class ProgressForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_pendingResult", Justification = "Is owned and disposed by external caller.")]
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.buttonHide = new System.Windows.Forms.Button();
            this.selectionsControl = new ZeroInstall.Commands.WinForms.SelectionsControl();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.taskControl = new NanoByte.Common.Controls.TaskControl();
            this.buttonCustomizeSelectionsDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(447, 226);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "(Cancel)";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon) (resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Zero Install";
            this.notifyIcon.BalloonTipClicked += new System.EventHandler(this.notifyIcon_BalloonTipClicked);
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // buttonHide
            // 
            this.buttonHide.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHide.Location = new System.Drawing.Point(366, 226);
            this.buttonHide.Name = "buttonHide";
            this.buttonHide.Size = new System.Drawing.Size(75, 23);
            this.buttonHide.TabIndex = 2;
            this.buttonHide.Text = "(Hide)";
            this.toolTip.SetToolTip(this.buttonHide, "Hides the window and continues running the process as a tray icon");
            this.buttonHide.UseVisualStyleBackColor = true;
            this.buttonHide.Click += new System.EventHandler(this.buttonHide_Click);
            // 
            // selectionsControl
            // 
            this.selectionsControl.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.selectionsControl.Location = new System.Drawing.Point(12, 12);
            this.selectionsControl.Name = "selectionsControl";
            this.selectionsControl.Size = new System.Drawing.Size(505, 208);
            this.selectionsControl.TabIndex = 1;
            this.selectionsControl.Visible = false;
            // 
            // taskControl
            // 
            this.taskControl.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.taskControl.Location = new System.Drawing.Point(24, 24);
            this.taskControl.Name = "taskControl";
            this.taskControl.Size = new System.Drawing.Size(486, 54);
            this.taskControl.TabIndex = 5;
            this.taskControl.Visible = false;
            // 
            // buttonCustomizeSelectionsDone
            // 
            this.buttonCustomizeSelectionsDone.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCustomizeSelectionsDone.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCustomizeSelectionsDone.Location = new System.Drawing.Point(12, 226);
            this.buttonCustomizeSelectionsDone.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.buttonCustomizeSelectionsDone.Name = "buttonCustomizeSelectionsDone";
            this.buttonCustomizeSelectionsDone.Size = new System.Drawing.Size(75, 23);
            this.buttonCustomizeSelectionsDone.TabIndex = 6;
            this.buttonCustomizeSelectionsDone.Text = "(Done)";
            this.buttonCustomizeSelectionsDone.UseVisualStyleBackColor = true;
            this.buttonCustomizeSelectionsDone.Visible = false;
            this.buttonCustomizeSelectionsDone.Click += new System.EventHandler(this.buttonCustomizeSelectionsDone_Click);
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(534, 261);
            this.Controls.Add(this.buttonCustomizeSelectionsDone);
            this.Controls.Add(this.taskControl);
            this.Controls.Add(this.buttonHide);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.selectionsControl);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(375, 225);
            this.Name = "ProgressForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProgressForm_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.ProgressForm_VisibleChanged);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonCustomizeSelectionsDone;
        private System.Windows.Forms.Button buttonHide;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private ZeroInstall.Commands.WinForms.SelectionsControl selectionsControl;
        private NanoByte.Common.Controls.TaskControl taskControl;
        private System.Windows.Forms.ToolTip toolTip;
        #endregion
    }
}
