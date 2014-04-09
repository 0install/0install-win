using NanoByte.Common.Controls;

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
            this.labelWorking = new System.Windows.Forms.Label();
            this.progressBarWorking = new System.Windows.Forms.ProgressBar();
            this.selectionsControl = new ZeroInstall.Commands.WinForms.SelectionsControl();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.trackingControl = new TaskControl();
            this.buttonModifySelectionsDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(372, 127);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "(Cancel)";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Zero Install";
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // buttonHide
            // 
            this.buttonHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHide.Location = new System.Drawing.Point(291, 127);
            this.buttonHide.Name = "buttonHide";
            this.buttonHide.Size = new System.Drawing.Size(75, 23);
            this.buttonHide.TabIndex = 2;
            this.buttonHide.Text = "(Hide)";
            this.toolTip.SetToolTip(this.buttonHide, "Hides the window and continues running the process as a tray icon");
            this.buttonHide.UseVisualStyleBackColor = true;
            this.buttonHide.Click += new System.EventHandler(this.buttonHide_Click);
            // 
            // labelWorking
            // 
            this.labelWorking.AutoSize = true;
            this.labelWorking.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWorking.Location = new System.Drawing.Point(8, 18);
            this.labelWorking.Name = "labelWorking";
            this.labelWorking.Size = new System.Drawing.Size(70, 20);
            this.labelWorking.TabIndex = 0;
            this.labelWorking.Text = "(Solving)";
            // 
            // progressBarWorking
            // 
            this.progressBarWorking.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarWorking.Location = new System.Drawing.Point(12, 41);
            this.progressBarWorking.Name = "progressBarWorking";
            this.progressBarWorking.Size = new System.Drawing.Size(435, 23);
            this.progressBarWorking.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarWorking.TabIndex = 4;
            // 
            // selectionsControl
            // 
            this.selectionsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectionsControl.Location = new System.Drawing.Point(12, 12);
            this.selectionsControl.Name = "selectionsControl";
            this.selectionsControl.Size = new System.Drawing.Size(435, 109);
            this.selectionsControl.TabIndex = 1;
            this.selectionsControl.Visible = false;
            // 
            // trackingControl
            // 
            this.trackingControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackingControl.Location = new System.Drawing.Point(13, 12);
            this.trackingControl.Name = "trackingControl";
            this.trackingControl.Size = new System.Drawing.Size(434, 54);
            this.trackingControl.TabIndex = 5;
            this.trackingControl.Visible = false;
            // 
            // buttonModifySelectionsDone
            // 
            this.buttonModifySelectionsDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonModifySelectionsDone.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonModifySelectionsDone.Location = new System.Drawing.Point(12, 127);
            this.buttonModifySelectionsDone.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.buttonModifySelectionsDone.Name = "buttonModifySelectionsDone";
            this.buttonModifySelectionsDone.Size = new System.Drawing.Size(75, 23);
            this.buttonModifySelectionsDone.TabIndex = 6;
            this.buttonModifySelectionsDone.Text = "(Done)";
            this.buttonModifySelectionsDone.UseVisualStyleBackColor = true;
            this.buttonModifySelectionsDone.Visible = false;
            this.buttonModifySelectionsDone.Click += new System.EventHandler(this.buttonModifySelectionsDone_Click);
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 162);
            this.Controls.Add(this.buttonModifySelectionsDone);
            this.Controls.Add(this.trackingControl);
            this.Controls.Add(this.buttonHide);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.selectionsControl);
            this.Controls.Add(this.progressBarWorking);
            this.Controls.Add(this.labelWorking);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(375, 150);
            this.Name = "ProgressForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Zero Install";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ProgressForm_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonHide;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private SelectionsControl selectionsControl;
        private System.Windows.Forms.Label labelWorking;
        private System.Windows.Forms.ProgressBar progressBarWorking;
        private System.Windows.Forms.ToolTip toolTip;
        private TaskControl trackingControl;
        private System.Windows.Forms.Button buttonModifySelectionsDone;

    }
}
