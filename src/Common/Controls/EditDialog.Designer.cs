namespace Common.Controls
{
    partial class EditDialog<T>
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
            this.propertyGrid = new Common.Controls.ResettablePropertyGrid();
            this.buttonResetValue = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(146, 256);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(227, 256);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(12, 12);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(290, 238);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid_SelectedGridItemChanged);
            // 
            // buttonResetValue
            // 
            this.buttonResetValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResetValue.Location = new System.Drawing.Point(12, 256);
            this.buttonResetValue.Name = "buttonResetValue";
            this.buttonResetValue.Size = new System.Drawing.Size(114, 23);
            this.buttonResetValue.TabIndex = 999;
            this.buttonResetValue.Text = "(Reset value)";
            this.buttonResetValue.UseVisualStyleBackColor = true;
            this.buttonResetValue.Click += new System.EventHandler(this.buttonResetValue_Click);
            // 
            // EditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(314, 291);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.buttonResetValue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "EditDialog";
            this.Controls.SetChildIndex(this.buttonResetValue, 0);
            this.Controls.SetChildIndex(this.propertyGrid, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private ResettablePropertyGrid propertyGrid;
        private System.Windows.Forms.Button buttonResetValue;
    }
}