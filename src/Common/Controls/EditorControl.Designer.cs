namespace Common.Controls
{
    partial class EditorControl<T>
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
            this.propertyGrid = new Common.Controls.ResettablePropertyGrid();
            this.buttonResetValue = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(290, 238);
            this.propertyGrid.TabIndex = 1000;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            this.propertyGrid.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid_SelectedGridItemChanged);
            // 
            // buttonResetValue
            // 
            this.buttonResetValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResetValue.Location = new System.Drawing.Point(0, 244);
            this.buttonResetValue.Name = "buttonResetValue";
            this.buttonResetValue.Size = new System.Drawing.Size(114, 23);
            this.buttonResetValue.TabIndex = 1001;
            this.buttonResetValue.Text = "(Reset value)";
            this.buttonResetValue.UseVisualStyleBackColor = true;
            this.buttonResetValue.Click += new System.EventHandler(this.buttonResetValue_Click);
            // 
            // EditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.buttonResetValue);
            this.Name = "EditorControl";
            this.Size = new System.Drawing.Size(290, 267);
            this.ResumeLayout(false);

        }

        #endregion

        private ResettablePropertyGrid propertyGrid;
        private System.Windows.Forms.Button buttonResetValue;

    }
}
