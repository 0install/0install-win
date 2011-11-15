namespace ZeroInstall.Central.WinForms
{
    partial class OptionsDialog
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
            this.buttonAdvanced = new System.Windows.Forms.Button();
            this.groupBoxSync = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // buttonAdvanced
            // 
            this.buttonAdvanced.Location = new System.Drawing.Point(12, 227);
            this.buttonAdvanced.Name = "buttonAdvanced";
            this.buttonAdvanced.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvanced.TabIndex = 1002;
            this.buttonAdvanced.Text = "&Advanced";
            this.buttonAdvanced.UseVisualStyleBackColor = true;
            this.buttonAdvanced.Click += new System.EventHandler(this.buttonAdvanced_Click);
            // 
            // groupBoxSync
            // 
            this.groupBoxSync.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSync.Name = "groupBoxSync";
            this.groupBoxSync.Size = new System.Drawing.Size(200, 100);
            this.groupBoxSync.TabIndex = 1003;
            this.groupBoxSync.TabStop = false;
            this.groupBoxSync.Text = "Sync";
            // 
            // OptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.buttonAdvanced);
            this.Controls.Add(this.groupBoxSync);
            this.Name = "OptionsDialog";
            this.Text = "Options";
            this.Controls.SetChildIndex(this.groupBoxSync, 0);
            this.Controls.SetChildIndex(this.buttonAdvanced, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonAdvanced;
        private System.Windows.Forms.GroupBox groupBoxSync;
    }
}