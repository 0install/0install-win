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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntegrateAppForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageStartMenu = new System.Windows.Forms.TabPage();
            this.dataGridStartMenu = new System.Windows.Forms.DataGridView();
            this.dataGridStartMenuColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridStartMenuColumnCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridStartMenuColumnCommand = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tabPageDesktop = new System.Windows.Forms.TabPage();
            this.dataGridDesktop = new System.Windows.Forms.DataGridView();
            this.dataGridDesktopColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridDesktopColumnCommand = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tabPageAliases = new System.Windows.Forms.TabPage();
            this.dataGridAliases = new System.Windows.Forms.DataGridView();
            this.dataGridAliasesColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridAliasesColumnCommand = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tabPageFileTypes = new System.Windows.Forms.TabPage();
            this.checkBoxFileTypesAll = new System.Windows.Forms.CheckBox();
            this.dataGridFileTypes = new System.Windows.Forms.DataGridView();
            this.dataGridFileTypesColumnDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridFileTypesColumnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridFileTypesColumnExtensions = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageUrlProtocols = new System.Windows.Forms.TabPage();
            this.checkBoxUrlProtocolsAll = new System.Windows.Forms.CheckBox();
            this.dataGridUrlProtocols = new System.Windows.Forms.DataGridView();
            this.dataGridUrlProtocolsColumnDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridUrlProtocolsColumnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridUrlProtocolsColumnProtocols = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageAutoPlay = new System.Windows.Forms.TabPage();
            this.checkBoxAutoPlayAll = new System.Windows.Forms.CheckBox();
            this.dataGridAutoPlay = new System.Windows.Forms.DataGridView();
            this.dataGridAutoPlayColumnDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridAutoPlayColumnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridAutoPlayColumnEvents = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageContextMenu = new System.Windows.Forms.TabPage();
            this.checkBoxContextMenuAll = new System.Windows.Forms.CheckBox();
            this.dataGridContextMenu = new System.Windows.Forms.DataGridView();
            this.dataGridContextMenuColumnDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridContextMenuColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageDefaultPrograms = new System.Windows.Forms.TabPage();
            this.checkBoxDefaultProgramsAll = new System.Windows.Forms.CheckBox();
            this.dataGridDefaultPrograms = new System.Windows.Forms.DataGridView();
            this.dataGridDefaultProgramsColumnDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridDefaultProgramsColumnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridDefaultProgramsColumnService = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checkBoxCapabilities = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoUpdate = new System.Windows.Forms.CheckBox();
            this.tabControl.SuspendLayout();
            this.tabPageStartMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridStartMenu)).BeginInit();
            this.tabPageDesktop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridDesktop)).BeginInit();
            this.tabPageAliases.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAliases)).BeginInit();
            this.tabPageFileTypes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFileTypes)).BeginInit();
            this.tabPageUrlProtocols.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridUrlProtocols)).BeginInit();
            this.tabPageAutoPlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAutoPlay)).BeginInit();
            this.tabPageContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridContextMenu)).BeginInit();
            this.tabPageDefaultPrograms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridDefaultPrograms)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.None;
            this.buttonOK.Location = new System.Drawing.Point(306, 258);
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(387, 258);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageStartMenu);
            this.tabControl.Controls.Add(this.tabPageDesktop);
            this.tabControl.Controls.Add(this.tabPageAliases);
            this.tabControl.Controls.Add(this.tabPageFileTypes);
            this.tabControl.Controls.Add(this.tabPageUrlProtocols);
            this.tabControl.Controls.Add(this.tabPageAutoPlay);
            this.tabControl.Controls.Add(this.tabPageContextMenu);
            this.tabControl.Controls.Add(this.tabPageDefaultPrograms);
            this.tabControl.Location = new System.Drawing.Point(12, 35);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(450, 217);
            this.tabControl.TabIndex = 2;
            // 
            // tabPageStartMenu
            // 
            this.tabPageStartMenu.Controls.Add(this.dataGridStartMenu);
            this.tabPageStartMenu.Location = new System.Drawing.Point(4, 22);
            this.tabPageStartMenu.Name = "tabPageStartMenu";
            this.tabPageStartMenu.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStartMenu.Size = new System.Drawing.Size(442, 191);
            this.tabPageStartMenu.TabIndex = 3;
            this.tabPageStartMenu.Text = "Start menu";
            this.tabPageStartMenu.UseVisualStyleBackColor = true;
            // 
            // dataGridStartMenu
            // 
            this.dataGridStartMenu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridStartMenu.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridStartMenuColumnName,
            this.dataGridStartMenuColumnCategory,
            this.dataGridStartMenuColumnCommand});
            this.dataGridStartMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridStartMenu.Location = new System.Drawing.Point(3, 3);
            this.dataGridStartMenu.Name = "dataGridStartMenu";
            this.dataGridStartMenu.Size = new System.Drawing.Size(436, 185);
            this.dataGridStartMenu.TabIndex = 0;
            // 
            // dataGridStartMenuColumnName
            // 
            this.dataGridStartMenuColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridStartMenuColumnName.DataPropertyName = "Name";
            this.dataGridStartMenuColumnName.FillWeight = 40F;
            this.dataGridStartMenuColumnName.HeaderText = "Name";
            this.dataGridStartMenuColumnName.Name = "dataGridStartMenuColumnName";
            // 
            // dataGridStartMenuColumnCategory
            // 
            this.dataGridStartMenuColumnCategory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridStartMenuColumnCategory.DataPropertyName = "Category";
            this.dataGridStartMenuColumnCategory.FillWeight = 40F;
            this.dataGridStartMenuColumnCategory.HeaderText = "Category";
            this.dataGridStartMenuColumnCategory.Name = "dataGridStartMenuColumnCategory";
            // 
            // dataGridStartMenuColumnCommand
            // 
            this.dataGridStartMenuColumnCommand.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridStartMenuColumnCommand.DataPropertyName = "Command";
            this.dataGridStartMenuColumnCommand.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.dataGridStartMenuColumnCommand.FillWeight = 20F;
            this.dataGridStartMenuColumnCommand.HeaderText = "Command";
            this.dataGridStartMenuColumnCommand.Name = "dataGridStartMenuColumnCommand";
            this.dataGridStartMenuColumnCommand.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // tabPageDesktop
            // 
            this.tabPageDesktop.Controls.Add(this.dataGridDesktop);
            this.tabPageDesktop.Location = new System.Drawing.Point(4, 22);
            this.tabPageDesktop.Name = "tabPageDesktop";
            this.tabPageDesktop.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDesktop.Size = new System.Drawing.Size(442, 191);
            this.tabPageDesktop.TabIndex = 5;
            this.tabPageDesktop.Text = "Desktop";
            this.tabPageDesktop.UseVisualStyleBackColor = true;
            // 
            // dataGridDesktop
            // 
            this.dataGridDesktop.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridDesktop.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridDesktopColumnName,
            this.dataGridDesktopColumnCommand});
            this.dataGridDesktop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridDesktop.Location = new System.Drawing.Point(3, 3);
            this.dataGridDesktop.Name = "dataGridDesktop";
            this.dataGridDesktop.Size = new System.Drawing.Size(436, 185);
            this.dataGridDesktop.TabIndex = 0;
            // 
            // dataGridDesktopColumnName
            // 
            this.dataGridDesktopColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridDesktopColumnName.DataPropertyName = "Name";
            this.dataGridDesktopColumnName.FillWeight = 70F;
            this.dataGridDesktopColumnName.HeaderText = "Name";
            this.dataGridDesktopColumnName.Name = "dataGridDesktopColumnName";
            // 
            // dataGridDesktopColumnCommand
            // 
            this.dataGridDesktopColumnCommand.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridDesktopColumnCommand.DataPropertyName = "Command";
            this.dataGridDesktopColumnCommand.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.dataGridDesktopColumnCommand.FillWeight = 30F;
            this.dataGridDesktopColumnCommand.HeaderText = "Command";
            this.dataGridDesktopColumnCommand.Name = "dataGridDesktopColumnCommand";
            this.dataGridDesktopColumnCommand.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // tabPageAliases
            // 
            this.tabPageAliases.Controls.Add(this.dataGridAliases);
            this.tabPageAliases.Location = new System.Drawing.Point(4, 22);
            this.tabPageAliases.Name = "tabPageAliases";
            this.tabPageAliases.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAliases.Size = new System.Drawing.Size(442, 191);
            this.tabPageAliases.TabIndex = 7;
            this.tabPageAliases.Text = "Aliases";
            this.tabPageAliases.UseVisualStyleBackColor = true;
            // 
            // dataGridAliases
            // 
            this.dataGridAliases.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridAliases.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridAliasesColumnName,
            this.dataGridAliasesColumnCommand});
            this.dataGridAliases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridAliases.Location = new System.Drawing.Point(3, 3);
            this.dataGridAliases.Name = "dataGridAliases";
            this.dataGridAliases.Size = new System.Drawing.Size(436, 185);
            this.dataGridAliases.TabIndex = 1;
            // 
            // dataGridAliasesColumnName
            // 
            this.dataGridAliasesColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridAliasesColumnName.DataPropertyName = "Name";
            this.dataGridAliasesColumnName.FillWeight = 70F;
            this.dataGridAliasesColumnName.HeaderText = "Name";
            this.dataGridAliasesColumnName.Name = "dataGridAliasesColumnName";
            // 
            // dataGridAliasesColumnCommand
            // 
            this.dataGridAliasesColumnCommand.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridAliasesColumnCommand.DataPropertyName = "Command";
            this.dataGridAliasesColumnCommand.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.dataGridAliasesColumnCommand.FillWeight = 30F;
            this.dataGridAliasesColumnCommand.HeaderText = "Command";
            this.dataGridAliasesColumnCommand.Name = "dataGridAliasesColumnCommand";
            this.dataGridAliasesColumnCommand.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // tabPageFileTypes
            // 
            this.tabPageFileTypes.Controls.Add(this.checkBoxFileTypesAll);
            this.tabPageFileTypes.Controls.Add(this.dataGridFileTypes);
            this.tabPageFileTypes.Location = new System.Drawing.Point(4, 22);
            this.tabPageFileTypes.Name = "tabPageFileTypes";
            this.tabPageFileTypes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFileTypes.Size = new System.Drawing.Size(442, 191);
            this.tabPageFileTypes.TabIndex = 0;
            this.tabPageFileTypes.Text = "File types";
            this.tabPageFileTypes.UseVisualStyleBackColor = true;
            // 
            // checkBoxFileTypesAll
            // 
            this.checkBoxFileTypesAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxFileTypesAll.AutoSize = true;
            this.checkBoxFileTypesAll.Location = new System.Drawing.Point(3, 174);
            this.checkBoxFileTypesAll.Name = "checkBoxFileTypesAll";
            this.checkBoxFileTypesAll.Size = new System.Drawing.Size(69, 17);
            this.checkBoxFileTypesAll.TabIndex = 1;
            this.checkBoxFileTypesAll.Text = "Select &all";
            this.checkBoxFileTypesAll.UseVisualStyleBackColor = true;
            this.checkBoxFileTypesAll.CheckedChanged += new System.EventHandler(this.checkBoxFileTypesAll_CheckedChanged);
            // 
            // dataGridFileTypes
            // 
            this.dataGridFileTypes.AllowUserToAddRows = false;
            this.dataGridFileTypes.AllowUserToDeleteRows = false;
            this.dataGridFileTypes.AllowUserToResizeRows = false;
            this.dataGridFileTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridFileTypes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridFileTypes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridFileTypesColumnDefault,
            this.dataGridFileTypesColumnDescription,
            this.dataGridFileTypesColumnExtensions});
            this.dataGridFileTypes.Location = new System.Drawing.Point(3, 3);
            this.dataGridFileTypes.Name = "dataGridFileTypes";
            this.dataGridFileTypes.RowHeadersVisible = false;
            this.dataGridFileTypes.Size = new System.Drawing.Size(436, 169);
            this.dataGridFileTypes.TabIndex = 0;
            // 
            // dataGridFileTypesColumnDefault
            // 
            this.dataGridFileTypesColumnDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridFileTypesColumnDefault.DataPropertyName = "Use";
            this.dataGridFileTypesColumnDefault.HeaderText = "Default";
            this.dataGridFileTypesColumnDefault.Name = "dataGridFileTypesColumnDefault";
            this.dataGridFileTypesColumnDefault.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridFileTypesColumnDefault.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridFileTypesColumnDefault.Width = 66;
            // 
            // dataGridFileTypesColumnDescription
            // 
            this.dataGridFileTypesColumnDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridFileTypesColumnDescription.DataPropertyName = "Description";
            this.dataGridFileTypesColumnDescription.HeaderText = "Description";
            this.dataGridFileTypesColumnDescription.Name = "dataGridFileTypesColumnDescription";
            this.dataGridFileTypesColumnDescription.ReadOnly = true;
            this.dataGridFileTypesColumnDescription.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridFileTypesColumnExtensions
            // 
            this.dataGridFileTypesColumnExtensions.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridFileTypesColumnExtensions.DataPropertyName = "Extensions";
            this.dataGridFileTypesColumnExtensions.HeaderText = "Extensions";
            this.dataGridFileTypesColumnExtensions.Name = "dataGridFileTypesColumnExtensions";
            this.dataGridFileTypesColumnExtensions.ReadOnly = true;
            this.dataGridFileTypesColumnExtensions.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridFileTypesColumnExtensions.Width = 83;
            // 
            // tabPageUrlProtocols
            // 
            this.tabPageUrlProtocols.Controls.Add(this.checkBoxUrlProtocolsAll);
            this.tabPageUrlProtocols.Controls.Add(this.dataGridUrlProtocols);
            this.tabPageUrlProtocols.Location = new System.Drawing.Point(4, 22);
            this.tabPageUrlProtocols.Name = "tabPageUrlProtocols";
            this.tabPageUrlProtocols.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUrlProtocols.Size = new System.Drawing.Size(442, 191);
            this.tabPageUrlProtocols.TabIndex = 1;
            this.tabPageUrlProtocols.Text = "URL protocols";
            this.tabPageUrlProtocols.UseVisualStyleBackColor = true;
            // 
            // checkBoxUrlProtocolsAll
            // 
            this.checkBoxUrlProtocolsAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxUrlProtocolsAll.AutoSize = true;
            this.checkBoxUrlProtocolsAll.Location = new System.Drawing.Point(3, 174);
            this.checkBoxUrlProtocolsAll.Name = "checkBoxUrlProtocolsAll";
            this.checkBoxUrlProtocolsAll.Size = new System.Drawing.Size(69, 17);
            this.checkBoxUrlProtocolsAll.TabIndex = 2;
            this.checkBoxUrlProtocolsAll.Text = "Select &all";
            this.checkBoxUrlProtocolsAll.UseVisualStyleBackColor = true;
            this.checkBoxUrlProtocolsAll.CheckedChanged += new System.EventHandler(this.checkBoxUrlProtocolsAll_CheckedChanged);
            // 
            // dataGridUrlProtocols
            // 
            this.dataGridUrlProtocols.AllowUserToAddRows = false;
            this.dataGridUrlProtocols.AllowUserToDeleteRows = false;
            this.dataGridUrlProtocols.AllowUserToResizeRows = false;
            this.dataGridUrlProtocols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridUrlProtocols.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridUrlProtocols.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridUrlProtocolsColumnDefault,
            this.dataGridUrlProtocolsColumnDescription,
            this.dataGridUrlProtocolsColumnProtocols});
            this.dataGridUrlProtocols.Location = new System.Drawing.Point(3, 3);
            this.dataGridUrlProtocols.Name = "dataGridUrlProtocols";
            this.dataGridUrlProtocols.RowHeadersVisible = false;
            this.dataGridUrlProtocols.Size = new System.Drawing.Size(436, 169);
            this.dataGridUrlProtocols.TabIndex = 0;
            // 
            // dataGridUrlProtocolsColumnDefault
            // 
            this.dataGridUrlProtocolsColumnDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridUrlProtocolsColumnDefault.DataPropertyName = "Use";
            this.dataGridUrlProtocolsColumnDefault.HeaderText = "Default";
            this.dataGridUrlProtocolsColumnDefault.Name = "dataGridUrlProtocolsColumnDefault";
            this.dataGridUrlProtocolsColumnDefault.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridUrlProtocolsColumnDefault.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridUrlProtocolsColumnDefault.Width = 66;
            // 
            // dataGridUrlProtocolsColumnDescription
            // 
            this.dataGridUrlProtocolsColumnDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridUrlProtocolsColumnDescription.DataPropertyName = "Description";
            this.dataGridUrlProtocolsColumnDescription.HeaderText = "Description";
            this.dataGridUrlProtocolsColumnDescription.Name = "dataGridUrlProtocolsColumnDescription";
            this.dataGridUrlProtocolsColumnDescription.ReadOnly = true;
            this.dataGridUrlProtocolsColumnDescription.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridUrlProtocolsColumnProtocols
            // 
            this.dataGridUrlProtocolsColumnProtocols.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridUrlProtocolsColumnProtocols.DataPropertyName = "KnownPrefixes";
            this.dataGridUrlProtocolsColumnProtocols.HeaderText = "Protocols";
            this.dataGridUrlProtocolsColumnProtocols.Name = "dataGridUrlProtocolsColumnProtocols";
            this.dataGridUrlProtocolsColumnProtocols.ReadOnly = true;
            this.dataGridUrlProtocolsColumnProtocols.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridUrlProtocolsColumnProtocols.Width = 76;
            // 
            // tabPageAutoPlay
            // 
            this.tabPageAutoPlay.Controls.Add(this.checkBoxAutoPlayAll);
            this.tabPageAutoPlay.Controls.Add(this.dataGridAutoPlay);
            this.tabPageAutoPlay.Location = new System.Drawing.Point(4, 22);
            this.tabPageAutoPlay.Name = "tabPageAutoPlay";
            this.tabPageAutoPlay.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAutoPlay.Size = new System.Drawing.Size(442, 191);
            this.tabPageAutoPlay.TabIndex = 6;
            this.tabPageAutoPlay.Text = "AutoPlay";
            this.tabPageAutoPlay.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoPlayAll
            // 
            this.checkBoxAutoPlayAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxAutoPlayAll.AutoSize = true;
            this.checkBoxAutoPlayAll.Location = new System.Drawing.Point(3, 174);
            this.checkBoxAutoPlayAll.Name = "checkBoxAutoPlayAll";
            this.checkBoxAutoPlayAll.Size = new System.Drawing.Size(69, 17);
            this.checkBoxAutoPlayAll.TabIndex = 2;
            this.checkBoxAutoPlayAll.Text = "Select &all";
            this.checkBoxAutoPlayAll.UseVisualStyleBackColor = true;
            this.checkBoxAutoPlayAll.CheckedChanged += new System.EventHandler(this.checkBoxAutoPlayAll_CheckedChanged);
            // 
            // dataGridAutoPlay
            // 
            this.dataGridAutoPlay.AllowUserToAddRows = false;
            this.dataGridAutoPlay.AllowUserToDeleteRows = false;
            this.dataGridAutoPlay.AllowUserToResizeRows = false;
            this.dataGridAutoPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridAutoPlay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridAutoPlay.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridAutoPlayColumnDefault,
            this.dataGridAutoPlayColumnDescription,
            this.dataGridAutoPlayColumnEvents});
            this.dataGridAutoPlay.Location = new System.Drawing.Point(3, 3);
            this.dataGridAutoPlay.Name = "dataGridAutoPlay";
            this.dataGridAutoPlay.RowHeadersVisible = false;
            this.dataGridAutoPlay.Size = new System.Drawing.Size(436, 169);
            this.dataGridAutoPlay.TabIndex = 1;
            // 
            // dataGridAutoPlayColumnDefault
            // 
            this.dataGridAutoPlayColumnDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridAutoPlayColumnDefault.DataPropertyName = "Use";
            this.dataGridAutoPlayColumnDefault.HeaderText = "Default";
            this.dataGridAutoPlayColumnDefault.Name = "dataGridAutoPlayColumnDefault";
            this.dataGridAutoPlayColumnDefault.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridAutoPlayColumnDefault.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridAutoPlayColumnDefault.Width = 66;
            // 
            // dataGridAutoPlayColumnDescription
            // 
            this.dataGridAutoPlayColumnDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridAutoPlayColumnDescription.DataPropertyName = "Description";
            this.dataGridAutoPlayColumnDescription.HeaderText = "Description";
            this.dataGridAutoPlayColumnDescription.Name = "dataGridAutoPlayColumnDescription";
            this.dataGridAutoPlayColumnDescription.ReadOnly = true;
            this.dataGridAutoPlayColumnDescription.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridAutoPlayColumnEvents
            // 
            this.dataGridAutoPlayColumnEvents.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridAutoPlayColumnEvents.DataPropertyName = "Events";
            this.dataGridAutoPlayColumnEvents.HeaderText = "Events";
            this.dataGridAutoPlayColumnEvents.Name = "dataGridAutoPlayColumnEvents";
            this.dataGridAutoPlayColumnEvents.ReadOnly = true;
            this.dataGridAutoPlayColumnEvents.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridAutoPlayColumnEvents.Width = 65;
            // 
            // tabPageContextMenu
            // 
            this.tabPageContextMenu.Controls.Add(this.checkBoxContextMenuAll);
            this.tabPageContextMenu.Controls.Add(this.dataGridContextMenu);
            this.tabPageContextMenu.Location = new System.Drawing.Point(4, 22);
            this.tabPageContextMenu.Name = "tabPageContextMenu";
            this.tabPageContextMenu.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageContextMenu.Size = new System.Drawing.Size(442, 191);
            this.tabPageContextMenu.TabIndex = 4;
            this.tabPageContextMenu.Text = "Context menu";
            this.tabPageContextMenu.UseVisualStyleBackColor = true;
            // 
            // checkBoxContextMenuAll
            // 
            this.checkBoxContextMenuAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxContextMenuAll.AutoSize = true;
            this.checkBoxContextMenuAll.Location = new System.Drawing.Point(3, 174);
            this.checkBoxContextMenuAll.Name = "checkBoxContextMenuAll";
            this.checkBoxContextMenuAll.Size = new System.Drawing.Size(69, 17);
            this.checkBoxContextMenuAll.TabIndex = 2;
            this.checkBoxContextMenuAll.Text = "Select &all";
            this.checkBoxContextMenuAll.UseVisualStyleBackColor = true;
            this.checkBoxContextMenuAll.CheckedChanged += new System.EventHandler(this.checkBoxContextMenuAll_CheckedChanged);
            // 
            // dataGridContextMenu
            // 
            this.dataGridContextMenu.AllowUserToAddRows = false;
            this.dataGridContextMenu.AllowUserToDeleteRows = false;
            this.dataGridContextMenu.AllowUserToResizeRows = false;
            this.dataGridContextMenu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridContextMenu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridContextMenu.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridContextMenuColumnDefault,
            this.dataGridContextMenuColumnName});
            this.dataGridContextMenu.Location = new System.Drawing.Point(3, 3);
            this.dataGridContextMenu.Name = "dataGridContextMenu";
            this.dataGridContextMenu.RowHeadersVisible = false;
            this.dataGridContextMenu.Size = new System.Drawing.Size(436, 169);
            this.dataGridContextMenu.TabIndex = 0;
            // 
            // dataGridContextMenuColumnDefault
            // 
            this.dataGridContextMenuColumnDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridContextMenuColumnDefault.DataPropertyName = "Use";
            this.dataGridContextMenuColumnDefault.Frozen = true;
            this.dataGridContextMenuColumnDefault.HeaderText = "Default";
            this.dataGridContextMenuColumnDefault.Name = "dataGridContextMenuColumnDefault";
            this.dataGridContextMenuColumnDefault.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridContextMenuColumnDefault.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridContextMenuColumnDefault.Width = 66;
            // 
            // dataGridContextMenuColumnName
            // 
            this.dataGridContextMenuColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridContextMenuColumnName.DataPropertyName = "Name";
            this.dataGridContextMenuColumnName.HeaderText = "Entry name";
            this.dataGridContextMenuColumnName.Name = "dataGridContextMenuColumnName";
            this.dataGridContextMenuColumnName.ReadOnly = true;
            this.dataGridContextMenuColumnName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // tabPageDefaultPrograms
            // 
            this.tabPageDefaultPrograms.Controls.Add(this.checkBoxDefaultProgramsAll);
            this.tabPageDefaultPrograms.Controls.Add(this.dataGridDefaultPrograms);
            this.tabPageDefaultPrograms.Location = new System.Drawing.Point(4, 22);
            this.tabPageDefaultPrograms.Name = "tabPageDefaultPrograms";
            this.tabPageDefaultPrograms.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDefaultPrograms.Size = new System.Drawing.Size(442, 191);
            this.tabPageDefaultPrograms.TabIndex = 2;
            this.tabPageDefaultPrograms.Text = "Default programs";
            this.tabPageDefaultPrograms.UseVisualStyleBackColor = true;
            // 
            // checkBoxDefaultProgramsAll
            // 
            this.checkBoxDefaultProgramsAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxDefaultProgramsAll.AutoSize = true;
            this.checkBoxDefaultProgramsAll.Location = new System.Drawing.Point(3, 174);
            this.checkBoxDefaultProgramsAll.Name = "checkBoxDefaultProgramsAll";
            this.checkBoxDefaultProgramsAll.Size = new System.Drawing.Size(69, 17);
            this.checkBoxDefaultProgramsAll.TabIndex = 2;
            this.checkBoxDefaultProgramsAll.Text = "Select &all";
            this.checkBoxDefaultProgramsAll.UseVisualStyleBackColor = true;
            this.checkBoxDefaultProgramsAll.CheckedChanged += new System.EventHandler(this.checkBoxDefaultProgramsAll_CheckedChanged);
            // 
            // dataGridDefaultPrograms
            // 
            this.dataGridDefaultPrograms.AllowUserToAddRows = false;
            this.dataGridDefaultPrograms.AllowUserToDeleteRows = false;
            this.dataGridDefaultPrograms.AllowUserToResizeRows = false;
            this.dataGridDefaultPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridDefaultPrograms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridDefaultPrograms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridDefaultProgramsColumnDefault,
            this.dataGridDefaultProgramsColumnDescription,
            this.dataGridDefaultProgramsColumnService});
            this.dataGridDefaultPrograms.Location = new System.Drawing.Point(3, 3);
            this.dataGridDefaultPrograms.Name = "dataGridDefaultPrograms";
            this.dataGridDefaultPrograms.RowHeadersVisible = false;
            this.dataGridDefaultPrograms.Size = new System.Drawing.Size(436, 169);
            this.dataGridDefaultPrograms.TabIndex = 0;
            // 
            // dataGridDefaultProgramsColumnDefault
            // 
            this.dataGridDefaultProgramsColumnDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridDefaultProgramsColumnDefault.DataPropertyName = "Use";
            this.dataGridDefaultProgramsColumnDefault.HeaderText = "Default";
            this.dataGridDefaultProgramsColumnDefault.Name = "dataGridDefaultProgramsColumnDefault";
            this.dataGridDefaultProgramsColumnDefault.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridDefaultProgramsColumnDefault.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridDefaultProgramsColumnDefault.Width = 66;
            // 
            // dataGridDefaultProgramsColumnDescription
            // 
            this.dataGridDefaultProgramsColumnDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridDefaultProgramsColumnDescription.DataPropertyName = "Description";
            this.dataGridDefaultProgramsColumnDescription.HeaderText = "Description";
            this.dataGridDefaultProgramsColumnDescription.Name = "dataGridDefaultProgramsColumnDescription";
            this.dataGridDefaultProgramsColumnDescription.ReadOnly = true;
            this.dataGridDefaultProgramsColumnDescription.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridDefaultProgramsColumnService
            // 
            this.dataGridDefaultProgramsColumnService.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridDefaultProgramsColumnService.DataPropertyName = "Service";
            this.dataGridDefaultProgramsColumnService.HeaderText = "Service";
            this.dataGridDefaultProgramsColumnService.Name = "dataGridDefaultProgramsColumnService";
            this.dataGridDefaultProgramsColumnService.ReadOnly = true;
            this.dataGridDefaultProgramsColumnService.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridDefaultProgramsColumnService.Width = 68;
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
            this.checkBoxAutoUpdate.Text = "Auto &Update";
            this.checkBoxAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // IntegrateAppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 293);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.checkBoxAutoUpdate);
            this.Controls.Add(this.checkBoxCapabilities);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new System.Drawing.Size(300, 230);
            this.Name = "IntegrateAppForm";
            this.ShowIcon = true;
            this.ShowInTaskbar = true;
            this.Text = "Integrate application";
            this.Load += new System.EventHandler(this.IntegrateAppForm_Load);
            this.Controls.SetChildIndex(this.checkBoxCapabilities, 0);
            this.Controls.SetChildIndex(this.checkBoxAutoUpdate, 0);
            this.Controls.SetChildIndex(this.tabControl, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControl.ResumeLayout(false);
            this.tabPageStartMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridStartMenu)).EndInit();
            this.tabPageDesktop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridDesktop)).EndInit();
            this.tabPageAliases.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAliases)).EndInit();
            this.tabPageFileTypes.ResumeLayout(false);
            this.tabPageFileTypes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFileTypes)).EndInit();
            this.tabPageUrlProtocols.ResumeLayout(false);
            this.tabPageUrlProtocols.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridUrlProtocols)).EndInit();
            this.tabPageAutoPlay.ResumeLayout(false);
            this.tabPageAutoPlay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAutoPlay)).EndInit();
            this.tabPageContextMenu.ResumeLayout(false);
            this.tabPageContextMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridContextMenu)).EndInit();
            this.tabPageDefaultPrograms.ResumeLayout(false);
            this.tabPageDefaultPrograms.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridDefaultPrograms)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageFileTypes;
        private System.Windows.Forms.TabPage tabPageUrlProtocols;
        private System.Windows.Forms.DataGridView dataGridFileTypes;
        private System.Windows.Forms.DataGridView dataGridUrlProtocols;
        private System.Windows.Forms.TabPage tabPageDefaultPrograms;
        private System.Windows.Forms.TabPage tabPageStartMenu;
        private System.Windows.Forms.DataGridView dataGridDefaultPrograms;
        private System.Windows.Forms.TabPage tabPageContextMenu;
        private System.Windows.Forms.DataGridView dataGridContextMenu;
        private System.Windows.Forms.CheckBox checkBoxCapabilities;
        private System.Windows.Forms.CheckBox checkBoxAutoUpdate;
        private System.Windows.Forms.DataGridView dataGridStartMenu;
        private System.Windows.Forms.TabPage tabPageDesktop;
        private System.Windows.Forms.DataGridView dataGridDesktop;
        private System.Windows.Forms.TabPage tabPageAutoPlay;
        private System.Windows.Forms.DataGridView dataGridAutoPlay;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridUrlProtocolsColumnDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridUrlProtocolsColumnDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridUrlProtocolsColumnProtocols;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridAutoPlayColumnDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridAutoPlayColumnDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridAutoPlayColumnEvents;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridContextMenuColumnDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridContextMenuColumnName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridDefaultProgramsColumnDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridDefaultProgramsColumnDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridDefaultProgramsColumnService;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridStartMenuColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridStartMenuColumnCategory;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridStartMenuColumnCommand;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridDesktopColumnName;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridDesktopColumnCommand;
        private System.Windows.Forms.TabPage tabPageAliases;
        private System.Windows.Forms.DataGridView dataGridAliases;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridAliasesColumnName;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridAliasesColumnCommand;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridFileTypesColumnDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridFileTypesColumnDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridFileTypesColumnExtensions;
        private System.Windows.Forms.CheckBox checkBoxFileTypesAll;
        private System.Windows.Forms.CheckBox checkBoxUrlProtocolsAll;
        private System.Windows.Forms.CheckBox checkBoxAutoPlayAll;
        private System.Windows.Forms.CheckBox checkBoxContextMenuAll;
        private System.Windows.Forms.CheckBox checkBoxDefaultProgramsAll;
    }
}