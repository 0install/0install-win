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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.dataGridViewDefaultPrograms = new System.Windows.Forms.DataGridView();
            this.dataGridViewDefaultProgramsCheckBoxUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewDefaultProgramsTextBoxDescriptions = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewDefaultProgramsTextBoxService = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageContextMenu = new System.Windows.Forms.TabPage();
            this.dataGridViewContextMenu = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxContextMenuUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxContextMenuName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageStartMenu = new System.Windows.Forms.TabPage();
            this.buttonStartMenuAdd = new System.Windows.Forms.Button();
            this.comboBoxEntryPoints = new System.Windows.Forms.ComboBox();
            this.dataGridViewMenuEntry = new System.Windows.Forms.DataGridView();
            this.DataGridViewStartMenuButtonColumnRemove = new System.Windows.Forms.DataGridViewButtonColumn();
            this.DataGridViewStartMenuTextBoxColumnEntryPoint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewStartMenuTextBoxColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewStartMenuTextBoxColumnCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.checkBoxCapabilities = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoUpdate = new System.Windows.Forms.CheckBox();
            this.tabControlCapabilities.SuspendLayout();
            this.tabPageFileTypes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileType)).BeginInit();
            this.tabPageUrlProtocol.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUrlProtocols)).BeginInit();
            this.tabPageDefaultPrograms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDefaultPrograms)).BeginInit();
            this.tabPageContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContextMenu)).BeginInit();
            this.tabPageStartMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMenuEntry)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(306, 258);
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(387, 258);
            // 
            // tabControlCapabilities
            // 
            this.tabControlCapabilities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlCapabilities.Controls.Add(this.tabPageFileTypes);
            this.tabControlCapabilities.Controls.Add(this.tabPageUrlProtocol);
            this.tabControlCapabilities.Controls.Add(this.tabPageDefaultPrograms);
            this.tabControlCapabilities.Controls.Add(this.tabPageContextMenu);
            this.tabControlCapabilities.Controls.Add(this.tabPageStartMenu);
            this.tabControlCapabilities.Location = new System.Drawing.Point(12, 35);
            this.tabControlCapabilities.Name = "tabControlCapabilities";
            this.tabControlCapabilities.SelectedIndex = 0;
            this.tabControlCapabilities.Size = new System.Drawing.Size(450, 217);
            this.tabControlCapabilities.TabIndex = 2;
            // 
            // tabPageFileTypes
            // 
            this.tabPageFileTypes.Controls.Add(this.dataGridViewFileType);
            this.tabPageFileTypes.Location = new System.Drawing.Point(4, 22);
            this.tabPageFileTypes.Name = "tabPageFileTypes";
            this.tabPageFileTypes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFileTypes.Size = new System.Drawing.Size(442, 191);
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewFileType.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewFileType.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFileType.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fileTypeDataGridViewUse,
            this.fileTypeDataGridViewDescription,
            this.fileTypeDataGridViewExtensions});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewFileType.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewFileType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewFileType.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewFileType.Name = "dataGridViewFileType";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewFileType.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewFileType.RowHeadersVisible = false;
            this.dataGridViewFileType.Size = new System.Drawing.Size(436, 185);
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
            this.tabPageUrlProtocol.Size = new System.Drawing.Size(442, 191);
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
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewUrlProtocols.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewUrlProtocols.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewUrlProtocols.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewUrlProtocolsCheckBoxUse,
            this.dataGridViewUrlProtocolsTextBoxDescription,
            this.dataGridViewUrlProtocolsTextBoxUrlProtocols});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewUrlProtocols.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewUrlProtocols.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewUrlProtocols.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewUrlProtocols.Name = "dataGridViewUrlProtocols";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewUrlProtocols.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewUrlProtocols.RowHeadersVisible = false;
            this.dataGridViewUrlProtocols.Size = new System.Drawing.Size(462, 292);
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
            this.tabPageDefaultPrograms.Size = new System.Drawing.Size(442, 191);
            this.tabPageDefaultPrograms.TabIndex = 2;
            this.tabPageDefaultPrograms.Text = "default programs";
            this.tabPageDefaultPrograms.UseVisualStyleBackColor = true;
            // 
            // dataGridViewDefaultPrograms
            // 
            this.dataGridViewDefaultPrograms.AllowUserToAddRows = false;
            this.dataGridViewDefaultPrograms.AllowUserToDeleteRows = false;
            this.dataGridViewDefaultPrograms.AllowUserToOrderColumns = true;
            this.dataGridViewDefaultPrograms.AllowUserToResizeRows = false;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDefaultPrograms.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewDefaultPrograms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDefaultPrograms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewDefaultProgramsCheckBoxUse,
            this.dataGridViewDefaultProgramsTextBoxDescriptions,
            this.dataGridViewDefaultProgramsTextBoxService});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewDefaultPrograms.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridViewDefaultPrograms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDefaultPrograms.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewDefaultPrograms.Name = "dataGridViewDefaultPrograms";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDefaultPrograms.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridViewDefaultPrograms.RowHeadersVisible = false;
            this.dataGridViewDefaultPrograms.Size = new System.Drawing.Size(462, 292);
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
            // tabPageContextMenu
            // 
            this.tabPageContextMenu.Controls.Add(this.dataGridViewContextMenu);
            this.tabPageContextMenu.Location = new System.Drawing.Point(4, 22);
            this.tabPageContextMenu.Name = "tabPageContextMenu";
            this.tabPageContextMenu.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageContextMenu.Size = new System.Drawing.Size(442, 191);
            this.tabPageContextMenu.TabIndex = 4;
            this.tabPageContextMenu.Text = "context menu";
            this.tabPageContextMenu.UseVisualStyleBackColor = true;
            // 
            // dataGridViewContextMenu
            // 
            this.dataGridViewContextMenu.AllowUserToAddRows = false;
            this.dataGridViewContextMenu.AllowUserToDeleteRows = false;
            this.dataGridViewContextMenu.AllowUserToOrderColumns = true;
            this.dataGridViewContextMenu.AllowUserToResizeRows = false;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewContextMenu.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            this.dataGridViewContextMenu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewContextMenu.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxContextMenuUse,
            this.dataGridViewTextBoxContextMenuName});
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewContextMenu.DefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridViewContextMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewContextMenu.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewContextMenu.Name = "dataGridViewContextMenu";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewContextMenu.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridViewContextMenu.RowHeadersVisible = false;
            this.dataGridViewContextMenu.Size = new System.Drawing.Size(436, 185);
            this.dataGridViewContextMenu.TabIndex = 6;
            // 
            // dataGridViewCheckBoxContextMenuUse
            // 
            this.dataGridViewCheckBoxContextMenuUse.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridViewCheckBoxContextMenuUse.DataPropertyName = "Use";
            this.dataGridViewCheckBoxContextMenuUse.Frozen = true;
            this.dataGridViewCheckBoxContextMenuUse.HeaderText = "Use";
            this.dataGridViewCheckBoxContextMenuUse.Name = "dataGridViewCheckBoxContextMenuUse";
            this.dataGridViewCheckBoxContextMenuUse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxContextMenuUse.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxContextMenuUse.Width = 51;
            // 
            // dataGridViewTextBoxContextMenuName
            // 
            this.dataGridViewTextBoxContextMenuName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxContextMenuName.DataPropertyName = "Name";
            this.dataGridViewTextBoxContextMenuName.HeaderText = "Entry name";
            this.dataGridViewTextBoxContextMenuName.Name = "dataGridViewTextBoxContextMenuName";
            this.dataGridViewTextBoxContextMenuName.ReadOnly = true;
            // 
            // tabPageStartMenu
            // 
            this.tabPageStartMenu.Controls.Add(this.buttonStartMenuAdd);
            this.tabPageStartMenu.Controls.Add(this.comboBoxEntryPoints);
            this.tabPageStartMenu.Controls.Add(this.dataGridViewMenuEntry);
            this.tabPageStartMenu.Location = new System.Drawing.Point(4, 22);
            this.tabPageStartMenu.Name = "tabPageStartMenu";
            this.tabPageStartMenu.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStartMenu.Size = new System.Drawing.Size(442, 191);
            this.tabPageStartMenu.TabIndex = 3;
            this.tabPageStartMenu.Text = "start menu";
            this.tabPageStartMenu.UseVisualStyleBackColor = true;
            // 
            // buttonStartMenuAdd
            // 
            this.buttonStartMenuAdd.Location = new System.Drawing.Point(182, 162);
            this.buttonStartMenuAdd.Name = "buttonStartMenuAdd";
            this.buttonStartMenuAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonStartMenuAdd.TabIndex = 14;
            this.buttonStartMenuAdd.Text = "Add";
            this.buttonStartMenuAdd.UseVisualStyleBackColor = true;
            // 
            // comboBoxEntryPoints
            // 
            this.comboBoxEntryPoints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEntryPoints.FormattingEnabled = true;
            this.comboBoxEntryPoints.Location = new System.Drawing.Point(6, 164);
            this.comboBoxEntryPoints.Name = "comboBoxEntryPoints";
            this.comboBoxEntryPoints.Size = new System.Drawing.Size(170, 21);
            this.comboBoxEntryPoints.TabIndex = 13;
            // 
            // dataGridViewMenuEntry
            // 
            this.dataGridViewMenuEntry.AllowUserToAddRows = false;
            this.dataGridViewMenuEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewMenuEntry.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle13;
            this.dataGridViewMenuEntry.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMenuEntry.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DataGridViewStartMenuButtonColumnRemove,
            this.DataGridViewStartMenuTextBoxColumnEntryPoint,
            this.DataGridViewStartMenuTextBoxColumnName,
            this.DataGridViewStartMenuTextBoxColumnCategory});
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewMenuEntry.DefaultCellStyle = dataGridViewCellStyle14;
            this.dataGridViewMenuEntry.Location = new System.Drawing.Point(6, 6);
            this.dataGridViewMenuEntry.Name = "dataGridViewMenuEntry";
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewMenuEntry.RowHeadersDefaultCellStyle = dataGridViewCellStyle15;
            this.dataGridViewMenuEntry.RowHeadersVisible = false;
            this.dataGridViewMenuEntry.Size = new System.Drawing.Size(430, 152);
            this.dataGridViewMenuEntry.TabIndex = 10;
            // 
            // DataGridViewStartMenuButtonColumnRemove
            // 
            this.DataGridViewStartMenuButtonColumnRemove.HeaderText = "Remove";
            this.DataGridViewStartMenuButtonColumnRemove.Name = "DataGridViewStartMenuButtonColumnRemove";
            // 
            // DataGridViewStartMenuTextBoxColumnEntryPoint
            // 
            this.DataGridViewStartMenuTextBoxColumnEntryPoint.HeaderText = "Entry Point";
            this.DataGridViewStartMenuTextBoxColumnEntryPoint.Name = "DataGridViewStartMenuTextBoxColumnEntryPoint";
            this.DataGridViewStartMenuTextBoxColumnEntryPoint.ReadOnly = true;
            // 
            // DataGridViewStartMenuTextBoxColumnName
            // 
            this.DataGridViewStartMenuTextBoxColumnName.HeaderText = "Name";
            this.DataGridViewStartMenuTextBoxColumnName.Name = "DataGridViewStartMenuTextBoxColumnName";
            // 
            // DataGridViewStartMenuTextBoxColumnCategory
            // 
            this.DataGridViewStartMenuTextBoxColumnCategory.HeaderText = "Category";
            this.DataGridViewStartMenuTextBoxColumnCategory.Name = "DataGridViewStartMenuTextBoxColumnCategory";
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.StartMenu;
            // 
            // checkBoxCapabilities
            // 
            this.checkBoxCapabilities.AutoSize = true;
            this.checkBoxCapabilities.Location = new System.Drawing.Point(104, 12);
            this.checkBoxCapabilities.Name = "checkBoxCapabilities";
            this.checkBoxCapabilities.Size = new System.Drawing.Size(120, 17);
            this.checkBoxCapabilities.TabIndex = 1;
            this.checkBoxCapabilities.Text = "&Register capabilities";
            this.checkBoxCapabilities.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoUpdate
            // 
            this.checkBoxAutoUpdate.AutoSize = true;
            this.checkBoxAutoUpdate.Location = new System.Drawing.Point(12, 12);
            this.checkBoxAutoUpdate.Name = "checkBoxAutoUpdate";
            this.checkBoxAutoUpdate.Size = new System.Drawing.Size(86, 17);
            this.checkBoxAutoUpdate.TabIndex = 0;
            this.checkBoxAutoUpdate.Tag = "";
            this.checkBoxAutoUpdate.Text = "&Auto Update";
            this.checkBoxAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // IntegrateAppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 293);
            this.Controls.Add(this.tabControlCapabilities);
            this.Controls.Add(this.checkBoxAutoUpdate);
            this.Controls.Add(this.checkBoxCapabilities);
            this.Name = "IntegrateAppForm";
            this.Text = "Integrate application";
            this.Load += new System.EventHandler(this.IntegrateAppForm_Load);
            this.Controls.SetChildIndex(this.checkBoxCapabilities, 0);
            this.Controls.SetChildIndex(this.checkBoxAutoUpdate, 0);
            this.Controls.SetChildIndex(this.tabControlCapabilities, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControlCapabilities.ResumeLayout(false);
            this.tabPageFileTypes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileType)).EndInit();
            this.tabPageUrlProtocol.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUrlProtocols)).EndInit();
            this.tabPageDefaultPrograms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDefaultPrograms)).EndInit();
            this.tabPageContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContextMenu)).EndInit();
            this.tabPageStartMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMenuEntry)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.DataGridView dataGridViewDefaultPrograms;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewDefaultProgramsCheckBoxUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewDefaultProgramsTextBoxDescriptions;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewDefaultProgramsTextBoxService;
        private System.Windows.Forms.TabPage tabPageContextMenu;
        private System.Windows.Forms.DataGridView dataGridViewContextMenu;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxContextMenuUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxContextMenuName;
        private System.Windows.Forms.CheckBox checkBoxCapabilities;
        private System.Windows.Forms.CheckBox checkBoxAutoUpdate;
        private System.Windows.Forms.Button buttonStartMenuAdd;
        private System.Windows.Forms.ComboBox comboBoxEntryPoints;
        private System.Windows.Forms.DataGridView dataGridViewMenuEntry;
        private System.Windows.Forms.DataGridViewButtonColumn DataGridViewStartMenuButtonColumnRemove;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewStartMenuTextBoxColumnEntryPoint;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewStartMenuTextBoxColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewStartMenuTextBoxColumnCategory;
    }
}