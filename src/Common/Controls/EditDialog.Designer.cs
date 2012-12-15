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
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(12, 12);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(260, 209);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid_SelectedGridItemChanged);
            // 
            // buttonResetValue
            // 
            this.buttonResetValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResetValue.Location = new System.Drawing.Point(12, 227);
            this.buttonResetValue.Name = "buttonResetValue";
            this.buttonResetValue.Size = new System.Drawing.Size(98, 23);
            this.buttonResetValue.TabIndex = 999;
            this.buttonResetValue.Text = "(Reset value)";
            this.buttonResetValue.UseVisualStyleBackColor = true;
            this.buttonResetValue.Click += new System.EventHandler(this.buttonResetValue_Click);
            // 
            // EditDialog
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.buttonResetValue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "EditDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private ResettablePropertyGrid propertyGrid;
        private System.Windows.Forms.Button buttonResetValue;
    }
}