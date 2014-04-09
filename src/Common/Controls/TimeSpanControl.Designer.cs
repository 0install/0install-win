namespace NanoByte.Common.Controls
{
    partial class TimeSpanControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.upDownDays = new System.Windows.Forms.NumericUpDown();
            this.labelDays = new System.Windows.Forms.Label();
            this.labelColon1 = new System.Windows.Forms.Label();
            this.upDownHours = new System.Windows.Forms.NumericUpDown();
            this.labelColon2 = new System.Windows.Forms.Label();
            this.upDownMinutes = new System.Windows.Forms.NumericUpDown();
            this.upDownSeconds = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.upDownDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownSeconds)).BeginInit();
            this.SuspendLayout();
            // 
            // upDownDays
            // 
            this.upDownDays.Location = new System.Drawing.Point(3, 3);
            this.upDownDays.Name = "upDownDays";
            this.upDownDays.Size = new System.Drawing.Size(40, 20);
            this.upDownDays.TabIndex = 0;
            // 
            // labelDays
            // 
            this.labelDays.AutoSize = true;
            this.labelDays.Location = new System.Drawing.Point(49, 5);
            this.labelDays.Name = "labelDays";
            this.labelDays.Size = new System.Drawing.Size(29, 13);
            this.labelDays.TabIndex = 1;
            this.labelDays.Text = "days";
            // 
            // labelColon1
            // 
            this.labelColon1.AutoSize = true;
            this.labelColon1.Location = new System.Drawing.Point(121, 5);
            this.labelColon1.Name = "labelColon1";
            this.labelColon1.Size = new System.Drawing.Size(10, 13);
            this.labelColon1.TabIndex = 3;
            this.labelColon1.Text = ":";
            // 
            // upDownHours
            // 
            this.upDownHours.Location = new System.Drawing.Point(84, 3);
            this.upDownHours.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.upDownHours.Name = "upDownHours";
            this.upDownHours.Size = new System.Drawing.Size(35, 20);
            this.upDownHours.TabIndex = 2;
            // 
            // labelColon2
            // 
            this.labelColon2.AutoSize = true;
            this.labelColon2.Location = new System.Drawing.Point(170, 5);
            this.labelColon2.Name = "labelColon2";
            this.labelColon2.Size = new System.Drawing.Size(10, 13);
            this.labelColon2.TabIndex = 5;
            this.labelColon2.Text = ":";
            // 
            // upDownMinutes
            // 
            this.upDownMinutes.Location = new System.Drawing.Point(133, 3);
            this.upDownMinutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.upDownMinutes.Name = "upDownMinutes";
            this.upDownMinutes.Size = new System.Drawing.Size(35, 20);
            this.upDownMinutes.TabIndex = 4;
            // 
            // upDownSeconds
            // 
            this.upDownSeconds.Location = new System.Drawing.Point(182, 3);
            this.upDownSeconds.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.upDownSeconds.Name = "upDownSeconds";
            this.upDownSeconds.Size = new System.Drawing.Size(35, 20);
            this.upDownSeconds.TabIndex = 6;
            // 
            // TimeSpanControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.upDownSeconds);
            this.Controls.Add(this.labelColon2);
            this.Controls.Add(this.upDownMinutes);
            this.Controls.Add(this.labelColon1);
            this.Controls.Add(this.upDownHours);
            this.Controls.Add(this.labelDays);
            this.Controls.Add(this.upDownDays);
            this.Name = "TimeSpanControl";
            this.Size = new System.Drawing.Size(223, 28);
            ((System.ComponentModel.ISupportInitialize)(this.upDownDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownSeconds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown upDownDays;
        private System.Windows.Forms.Label labelDays;
        private System.Windows.Forms.Label labelColon1;
        private System.Windows.Forms.NumericUpDown upDownHours;
        private System.Windows.Forms.Label labelColon2;
        private System.Windows.Forms.NumericUpDown upDownMinutes;
        private System.Windows.Forms.NumericUpDown upDownSeconds;
    }
}
