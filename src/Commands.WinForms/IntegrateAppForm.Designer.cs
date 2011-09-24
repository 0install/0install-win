namespace ZeroInstall.Commands.WinForms
{
    partial class IntegrateAppForm
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
            this.tabControlCapabilities = new System.Windows.Forms.TabControl();
            this.tabPageFileTypes = new System.Windows.Forms.TabPage();
            this.dataGridViewFileType = new System.Windows.Forms.DataGridView();
            this.fileTypeDataGridViewUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.fileTypeDataGridViewDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileTypeDataGridViewExtensions = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageUrlProtocol = new System.Windows.Forms.TabPage();
            this.dataGridViewUrlProtocols = new System.Windows.Forms.DataGridView();
            this.dataGridViewUrlProtocolsCheckBoxUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewUrlProtocolsTextBoxDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageDefaultPrograms = new System.Windows.Forms.TabPage();
            this.tabPageStartMenu = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.dataGridViewDefaultPrograms = new System.Windows.Forms.DataGridView();
            this.dataGridViewDefaultProgramsCheckBoxUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewDefaultProgramsTextBoxDescriptions = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewDefaultProgramsTextBoxService = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControlCapabilities.SuspendLayout();
            this.tabPageFileTypes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileType)).BeginInit();
            this.tabPageUrlProtocol.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUrlProtocols)).BeginInit();
            this.tabPageDefaultPrograms.SuspendLayout();
            this.tabPageStartMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDefaultPrograms)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(332, 207);
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(413, 207);
            // 
            // tabControlCapabilities
            // 
            this.tabControlCapabilities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlCapabilities.Controls.Add(this.tabPageFileTypes);
            this.tabControlCapabilities.Controls.Add(this.tabPageUrlProtocol);
            this.tabControlCapabilities.Controls.Add(this.tabPageDefaultPrograms);
            this.tabControlCapabilities.Controls.Add(this.tabPageStartMenu);
            this.tabControlCapabilities.Location = new System.Drawing.Point(12, 12);
            this.tabControlCapabilities.Name = "tabControlCapabilities";
            this.tabControlCapabilities.SelectedIndex = 0;
            this.tabControlCapabilities.Size = new System.Drawing.Size(476, 189);
            this.tabControlCapabilities.TabIndex = 1002;
            // 
            // tabPageFileTypes
            // 
            this.tabPageFileTypes.Controls.Add(this.dataGridViewFileType);
            this.tabPageFileTypes.Location = new System.Drawing.Point(4, 22);
            this.tabPageFileTypes.Name = "tabPageFileTypes";
            this.tabPageFileTypes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFileTypes.Size = new System.Drawing.Size(587, 370);
            this.tabPageFileTypes.TabIndex = 0;
            this.tabPageFileTypes.Text = "file types";
            this.tabPageFileTypes.UseVisualStyleBackColor = true;
            // 
            // dataGridViewFileType
            // 
            this.dataGridViewFileType.AllowUserToAddRows = false;
            this.dataGridViewFileType.AllowUserToDeleteRows = false;
            this.dataGridViewFileType.AllowUserToOrderColumns = true;
            this.dataGridViewFileType.AllowUserToResizeRows = false;
            this.dataGridViewFileType.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFileType.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fileTypeDataGridViewUse,
            this.fileTypeDataGridViewDescription,
            this.fileTypeDataGridViewExtensions});
            this.dataGridViewFileType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewFileType.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewFileType.Name = "dataGridViewFileType";
            this.dataGridViewFileType.RowHeadersVisible = false;
            this.dataGridViewFileType.Size = new System.Drawing.Size(581, 364);
            this.dataGridViewFileType.TabIndex = 0;
            // 
            // fileTypeDataGridViewUse
            // 
            this.fileTypeDataGridViewUse.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.fileTypeDataGridViewUse.DataPropertyName = "Use";
            this.fileTypeDataGridViewUse.HeaderText = "Use";
            this.fileTypeDataGridViewUse.Name = "fileTypeDataGridViewUse";
            this.fileTypeDataGridViewUse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.fileTypeDataGridViewUse.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.fileTypeDataGridViewUse.Width = 51;
            // 
            // fileTypeDataGridViewDescription
            // 
            this.fileTypeDataGridViewDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.fileTypeDataGridViewDescription.DataPropertyName = "BestDescription";
            this.fileTypeDataGridViewDescription.HeaderText = "Description";
            this.fileTypeDataGridViewDescription.Name = "fileTypeDataGridViewDescription";
            this.fileTypeDataGridViewDescription.ReadOnly = true;
            // 
            // fileTypeDataGridViewExtensions
            // 
            this.fileTypeDataGridViewExtensions.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.fileTypeDataGridViewExtensions.DataPropertyName = "Extensions";
            this.fileTypeDataGridViewExtensions.HeaderText = "Extensions";
            this.fileTypeDataGridViewExtensions.Name = "fileTypeDataGridViewExtensions";
            this.fileTypeDataGridViewExtensions.ReadOnly = true;
            this.fileTypeDataGridViewExtensions.Width = 83;
            // 
            // tabPageUrlProtocol
            // 
            this.tabPageUrlProtocol.Controls.Add(this.dataGridViewUrlProtocols);
            this.tabPageUrlProtocol.Location = new System.Drawing.Point(4, 22);
            this.tabPageUrlProtocol.Name = "tabPageUrlProtocol";
            this.tabPageUrlProtocol.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUrlProtocol.Size = new System.Drawing.Size(587, 370);
            this.tabPageUrlProtocol.TabIndex = 1;
            this.tabPageUrlProtocol.Text = "url protocol";
            this.tabPageUrlProtocol.UseVisualStyleBackColor = true;
            // 
            // dataGridViewUrlProtocols
            // 
            this.dataGridViewUrlProtocols.AllowUserToAddRows = false;
            this.dataGridViewUrlProtocols.AllowUserToDeleteRows = false;
            this.dataGridViewUrlProtocols.AllowUserToOrderColumns = true;
            this.dataGridViewUrlProtocols.AllowUserToResizeRows = false;
            this.dataGridViewUrlProtocols.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewUrlProtocols.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewUrlProtocolsCheckBoxUse,
            this.dataGridViewUrlProtocolsTextBoxDescription,
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols});
            this.dataGridViewUrlProtocols.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewUrlProtocols.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewUrlProtocols.Name = "dataGridViewUrlProtocols";
            this.dataGridViewUrlProtocols.RowHeadersVisible = false;
            this.dataGridViewUrlProtocols.Size = new System.Drawing.Size(581, 364);
            this.dataGridViewUrlProtocols.TabIndex = 1;
            // 
            // dataGridViewUrlProtocolsCheckBoxUse
            // 
            this.dataGridViewUrlProtocolsCheckBoxUse.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewUrlProtocolsCheckBoxUse.DataPropertyName = "Use";
            this.dataGridViewUrlProtocolsCheckBoxUse.HeaderText = "Use";
            this.dataGridViewUrlProtocolsCheckBoxUse.Name = "dataGridViewUrlProtocolsCheckBoxUse";
            this.dataGridViewUrlProtocolsCheckBoxUse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewUrlProtocolsCheckBoxUse.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewUrlProtocolsCheckBoxUse.Width = 51;
            // 
            // dataGridViewUrlProtocolsTextBoxDescription
            // 
            this.dataGridViewUrlProtocolsTextBoxDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewUrlProtocolsTextBoxDescription.DataPropertyName = "BestDescription";
            this.dataGridViewUrlProtocolsTextBoxDescription.HeaderText = "Description";
            this.dataGridViewUrlProtocolsTextBoxDescription.Name = "dataGridViewUrlProtocolsTextBoxDescription";
            this.dataGridViewUrlProtocolsTextBoxDescription.ReadOnly = true;
            // 
            // dataGridViewUrlProtocolsTextBoxUrlProtocols
            // 
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols.DataPropertyName = "KnownPrefixes";
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols.HeaderText = "Url protocols";
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols.Name = "dataGridViewUrlProtocolsTextBoxUrlProtocols";
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols.ReadOnly = true;
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols.Width = 91;
            // 
            // tabPageDefaultPrograms
            // 
            this.tabPageDefaultPrograms.Controls.Add(this.dataGridViewDefaultPrograms);
            this.tabPageDefaultPrograms.Location = new System.Drawing.Point(4, 22);
            this.tabPageDefaultPrograms.Name = "tabPageDefaultPrograms";
            this.tabPageDefaultPrograms.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDefaultPrograms.Size = new System.Drawing.Size(468, 163);
            this.tabPageDefaultPrograms.TabIndex = 2;
            this.tabPageDefaultPrograms.Text = "default programs";
            this.tabPageDefaultPrograms.UseVisualStyleBackColor = true;
            // 
            // tabPageStartMenu
            // 
            this.tabPageStartMenu.Controls.Add(this.textBox1);
            this.tabPageStartMenu.Controls.Add(this.button1);
            this.tabPageStartMenu.Location = new System.Drawing.Point(4, 22);
            this.tabPageStartMenu.Name = "tabPageStartMenu";
            this.tabPageStartMenu.Size = new System.Drawing.Size(587, 370);
            this.tabPageStartMenu.TabIndex = 3;
            this.tabPageStartMenu.Text = "start menu";
            this.tabPageStartMenu.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(50, 51);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(160, 20);
            this.textBox1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(232, 51);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.StartMenu;
            // 
            // dataGridViewDefaultPrograms
            // 
            this.dataGridViewDefaultPrograms.AllowUserToAddRows = false;
            this.dataGridViewDefaultPrograms.AllowUserToDeleteRows = false;
            this.dataGridViewDefaultPrograms.AllowUserToOrderColumns = true;
            this.dataGridViewDefaultPrograms.AllowUserToResizeRows = false;
            this.dataGridViewDefaultPrograms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDefaultPrograms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewDefaultProgramsCheckBoxUse,
            this.dataGridViewDefaultProgramsTextBoxDescriptions,
            this.dataGridViewDefaultProgramsTextBoxService});
            this.dataGridViewDefaultPrograms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDefaultPrograms.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewDefaultPrograms.Name = "dataGridViewDefaultPrograms";
            this.dataGridViewDefaultPrograms.RowHeadersVisible = false;
            this.dataGridViewDefaultPrograms.Size = new System.Drawing.Size(462, 157);
            this.dataGridViewDefaultPrograms.TabIndex = 5;
            // 
            // dataGridViewDefaultProgramsCheckBoxUse
            // 
            this.dataGridViewDefaultProgramsCheckBoxUse.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewDefaultProgramsCheckBoxUse.DataPropertyName = "Use";
            this.dataGridViewDefaultProgramsCheckBoxUse.HeaderText = "Use";
            this.dataGridViewDefaultProgramsCheckBoxUse.Name = "dataGridViewDefaultProgramsCheckBoxUse";
            this.dataGridViewDefaultProgramsCheckBoxUse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewDefaultProgramsCheckBoxUse.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewDefaultProgramsCheckBoxUse.Width = 51;
            // 
            // dataGridViewDefaultProgramsTextBoxDescriptions
            // 
            this.dataGridViewDefaultProgramsTextBoxDescriptions.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewDefaultProgramsTextBoxDescriptions.DataPropertyName = "BestDescription";
            this.dataGridViewDefaultProgramsTextBoxDescriptions.HeaderText = "Description";
            this.dataGridViewDefaultProgramsTextBoxDescriptions.Name = "dataGridViewDefaultProgramsTextBoxDescriptions";
            this.dataGridViewDefaultProgramsTextBoxDescriptions.ReadOnly = true;
            // 
            // dataGridViewDefaultProgramsTextBoxService
            // 
            this.dataGridViewDefaultProgramsTextBoxService.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewDefaultProgramsTextBoxService.DataPropertyName = "Service";
            this.dataGridViewDefaultProgramsTextBoxService.HeaderText = "Service";
            this.dataGridViewDefaultProgramsTextBoxService.Name = "dataGridViewDefaultProgramsTextBoxService";
            this.dataGridViewDefaultProgramsTextBoxService.ReadOnly = true;
            this.dataGridViewDefaultProgramsTextBoxService.Width = 68;
            // 
            // IntegrateAppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 242);
            this.Controls.Add(this.tabControlCapabilities);
            this.Name = "IntegrateAppForm";
            this.Text = "Integrate application";
            this.Load += new System.EventHandler(this.IntegrateAppForm_Load);
            this.Controls.SetChildIndex(this.tabControlCapabilities, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControlCapabilities.ResumeLayout(false);
            this.tabPageFileTypes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileType)).EndInit();
            this.tabPageUrlProtocol.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUrlProtocols)).EndInit();
            this.tabPageDefaultPrograms.ResumeLayout(false);
            this.tabPageStartMenu.ResumeLayout(false);
            this.tabPageStartMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDefaultPrograms)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlCapabilities;
        private System.Windows.Forms.TabPage tabPageFileTypes;
        private System.Windows.Forms.TabPage tabPageUrlProtocol;
        private System.Windows.Forms.DataGridView dataGridViewFileType;
        private System.Windows.Forms.DataGridView dataGridViewUrlProtocols;
        private System.Windows.Forms.TabPage tabPageDefaultPrograms;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewUrlProtocolsCheckBoxUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewUrlProtocolsTextBoxDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewUrlProtocolsTextBoxUrlProtocols;
        private System.Windows.Forms.DataGridViewCheckBoxColumn fileTypeDataGridViewUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileTypeDataGridViewDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileTypeDataGridViewExtensions;
        private System.Windows.Forms.TabPage tabPageStartMenu;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.DataGridView dataGridViewDefaultPrograms;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewDefaultProgramsCheckBoxUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewDefaultProgramsTextBoxDescriptions;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewDefaultProgramsTextBoxService;
    }
}