namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class DetailsPage
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
            this.components = new System.ComponentModel.Container();
            this.labelTitle = new System.Windows.Forms.Label();
            this.propertyGridCandidate = new Common.Controls.ResettablePropertyGrid();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 18);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(470, 37);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Fill in missing details";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // propertyGridCandidate
            // 
            this.propertyGridCandidate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyGridCandidate.Location = new System.Drawing.Point(39, 58);
            this.propertyGridCandidate.Name = "propertyGridCandidate";
            this.propertyGridCandidate.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGridCandidate.Size = new System.Drawing.Size(396, 191);
            this.propertyGridCandidate.TabIndex = 1;
            this.propertyGridCandidate.ToolbarVisible = false;
            this.propertyGridCandidate.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridCandidate_PropertyValueChanged);
            // 
            // buttonContinue
            // 
            this.buttonContinue.Location = new System.Drawing.Point(335, 255);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(100, 33);
            this.buttonContinue.TabIndex = 2;
            this.buttonContinue.Text = "&Continue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // DetailsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.propertyGridCandidate);
            this.Controls.Add(this.labelTitle);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DetailsPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private Common.Controls.ResettablePropertyGrid propertyGridCandidate;
        private System.Windows.Forms.Button buttonContinue;
    }
}
