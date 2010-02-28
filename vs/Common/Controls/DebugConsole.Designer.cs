namespace Common.Controls
{
    partial class DebugConsole
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
            if (disposing)
            {
                if (components != null) components.Dispose();
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
            this.outputPanel = new System.Windows.Forms.Panel();
            this.outputBox = new System.Windows.Forms.TextBox();
            this.inputPanel = new System.Windows.Forms.Panel();
            this.runButton = new System.Windows.Forms.Button();
            this.inputBox = new System.Windows.Forms.TextBox();
            this.outputPanel.SuspendLayout();
            this.inputPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // outputPanel
            // 
            this.outputPanel.Controls.Add(this.outputBox);
            this.outputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputPanel.Location = new System.Drawing.Point(0, 0);
            this.outputPanel.Name = "outputPanel";
            this.outputPanel.Size = new System.Drawing.Size(492, 345);
            this.outputPanel.TabIndex = 0;
            // 
            // outputBox
            // 
            this.outputBox.BackColor = System.Drawing.SystemColors.Control;
            this.outputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputBox.Location = new System.Drawing.Point(0, 0);
            this.outputBox.Multiline = true;
            this.outputBox.Name = "outputBox";
            this.outputBox.ReadOnly = true;
            this.outputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputBox.Size = new System.Drawing.Size(492, 345);
            this.outputBox.TabIndex = 0;
            this.outputBox.TabStop = false;
            // 
            // inputPanel
            // 
            this.inputPanel.Controls.Add(this.runButton);
            this.inputPanel.Controls.Add(this.inputBox);
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputPanel.Location = new System.Drawing.Point(0, 345);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Size = new System.Drawing.Size(492, 27);
            this.inputPanel.TabIndex = 1;
            // 
            // runButton
            // 
            this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runButton.Location = new System.Drawing.Point(454, 3);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(35, 20);
            this.runButton.TabIndex = 1;
            this.runButton.TabStop = false;
            this.runButton.Text = "&Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // inputBox
            // 
            this.inputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                         | System.Windows.Forms.AnchorStyles.Right)));
            this.inputBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.inputBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.inputBox.Location = new System.Drawing.Point(3, 3);
            this.inputBox.Name = "inputBox";
            this.inputBox.Size = new System.Drawing.Size(445, 20);
            this.inputBox.TabIndex = 0;
            this.inputBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputBox_KeyDown);
            // 
            // DebugForm
            // 
            this.AcceptButton = this.runButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 372);
            this.Controls.Add(this.outputPanel);
            this.Controls.Add(this.inputPanel);
            this.Name = "DebugForm";
            this.ShowIcon = false;
            this.Text = "Debug Console";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.DebugForm_Load);
            this.Shown += new System.EventHandler(this.DebugForm_Shown);
            this.outputPanel.ResumeLayout(false);
            this.outputPanel.PerformLayout();
            this.inputPanel.ResumeLayout(false);
            this.inputPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel outputPanel;
        private System.Windows.Forms.TextBox outputBox;
        private System.Windows.Forms.Panel inputPanel;
        private System.Windows.Forms.TextBox inputBox;
        private System.Windows.Forms.Button runButton;
    }
}