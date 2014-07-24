namespace d2mp
{
    partial class modManager
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(modManager));
            this.gbxMods = new System.Windows.Forms.GroupBox();
            this.modsGridView = new System.Windows.Forms.DataGridView();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gbxModActions = new System.Windows.Forms.GroupBox();
            this.ckbUpdate = new System.Windows.Forms.CheckBox();
            this.btnUninstallAll = new System.Windows.Forms.Button();
            this.btnInstallAll = new System.Windows.Forms.Button();
            this.btnUpdateAll = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.modMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.installModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.setActiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gbxMods.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modsGridView)).BeginInit();
            this.gbxModActions.SuspendLayout();
            this.modMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxMods
            // 
            this.gbxMods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxMods.Controls.Add(this.modsGridView);
            this.gbxMods.Location = new System.Drawing.Point(12, 12);
            this.gbxMods.Name = "gbxMods";
            this.gbxMods.Size = new System.Drawing.Size(619, 168);
            this.gbxMods.TabIndex = 0;
            this.gbxMods.TabStop = false;
            this.gbxMods.Text = "Available Mods";
            // 
            // modsGridView
            // 
            this.modsGridView.AllowUserToAddRows = false;
            this.modsGridView.AllowUserToDeleteRows = false;
            this.modsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modsGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.modsGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.modsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.modsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.version,
            this.author,
            this.status});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.modsGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this.modsGridView.Location = new System.Drawing.Point(6, 19);
            this.modsGridView.MultiSelect = false;
            this.modsGridView.Name = "modsGridView";
            this.modsGridView.ReadOnly = true;
            this.modsGridView.RowHeadersVisible = false;
            this.modsGridView.RowTemplate.Height = 20;
            this.modsGridView.RowTemplate.ReadOnly = true;
            this.modsGridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.modsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.modsGridView.Size = new System.Drawing.Size(607, 143);
            this.modsGridView.TabIndex = 0;
            this.modsGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.modsGridView_CellMouseDown);
            // 
            // name
            // 
            this.name.HeaderText = "Name";
            this.name.Name = "name";
            this.name.ReadOnly = true;
            this.name.Width = 170;
            // 
            // version
            // 
            this.version.HeaderText = "Version";
            this.version.Name = "version";
            this.version.ReadOnly = true;
            this.version.Width = 80;
            // 
            // author
            // 
            this.author.HeaderText = "Author";
            this.author.Name = "author";
            this.author.ReadOnly = true;
            this.author.Width = 200;
            // 
            // status
            // 
            this.status.HeaderText = "Status";
            this.status.Name = "status";
            this.status.ReadOnly = true;
            this.status.Width = 110;
            // 
            // gbxModActions
            // 
            this.gbxModActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxModActions.Controls.Add(this.ckbUpdate);
            this.gbxModActions.Controls.Add(this.btnUninstallAll);
            this.gbxModActions.Controls.Add(this.btnInstallAll);
            this.gbxModActions.Controls.Add(this.btnUpdateAll);
            this.gbxModActions.Controls.Add(this.btnRefresh);
            this.gbxModActions.Location = new System.Drawing.Point(12, 186);
            this.gbxModActions.Name = "gbxModActions";
            this.gbxModActions.Size = new System.Drawing.Size(619, 59);
            this.gbxModActions.TabIndex = 1;
            this.gbxModActions.TabStop = false;
            this.gbxModActions.Text = "Actions";
            // 
            // ckbUpdate
            // 
            this.ckbUpdate.Location = new System.Drawing.Point(430, 19);
            this.ckbUpdate.Name = "ckbUpdate";
            this.ckbUpdate.Size = new System.Drawing.Size(120, 30);
            this.ckbUpdate.TabIndex = 4;
            this.ckbUpdate.Text = "Update all outdated mods at startup";
            this.ckbUpdate.UseVisualStyleBackColor = true;
            this.ckbUpdate.CheckedChanged += new System.EventHandler(this.ckbUpdate_CheckedChanged);
            // 
            // btnUninstallAll
            // 
            this.btnUninstallAll.Location = new System.Drawing.Point(324, 19);
            this.btnUninstallAll.Name = "btnUninstallAll";
            this.btnUninstallAll.Size = new System.Drawing.Size(100, 30);
            this.btnUninstallAll.TabIndex = 3;
            this.btnUninstallAll.Text = "Remove All";
            this.btnUninstallAll.UseVisualStyleBackColor = true;
            this.btnUninstallAll.Click += new System.EventHandler(this.btnUninstallAll_Click);
            // 
            // btnInstallAll
            // 
            this.btnInstallAll.Location = new System.Drawing.Point(218, 19);
            this.btnInstallAll.Name = "btnInstallAll";
            this.btnInstallAll.Size = new System.Drawing.Size(100, 30);
            this.btnInstallAll.TabIndex = 2;
            this.btnInstallAll.Text = "Install All";
            this.btnInstallAll.UseVisualStyleBackColor = true;
            this.btnInstallAll.Click += new System.EventHandler(this.btnInstallAll_Click);
            // 
            // btnUpdateAll
            // 
            this.btnUpdateAll.Enabled = false;
            this.btnUpdateAll.Location = new System.Drawing.Point(112, 19);
            this.btnUpdateAll.Name = "btnUpdateAll";
            this.btnUpdateAll.Size = new System.Drawing.Size(100, 30);
            this.btnUpdateAll.TabIndex = 1;
            this.btnUpdateAll.Text = "Update All";
            this.btnUpdateAll.UseVisualStyleBackColor = true;
            this.btnUpdateAll.Click += new System.EventHandler(this.btnUpdateAll_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(6, 19);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 30);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh Mods";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // modMenuStrip
            // 
            this.modMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installModToolStripMenuItem,
            this.toolStripSeparator1,
            this.setActiveToolStripMenuItem,
            this.updateModToolStripMenuItem,
            this.removeModToolStripMenuItem});
            this.modMenuStrip.Name = "modMenuStrip";
            this.modMenuStrip.ShowImageMargin = false;
            this.modMenuStrip.Size = new System.Drawing.Size(121, 98);
            // 
            // installModToolStripMenuItem
            // 
            this.installModToolStripMenuItem.Name = "installModToolStripMenuItem";
            this.installModToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.installModToolStripMenuItem.Text = "Install Mod";
            this.installModToolStripMenuItem.Click += new System.EventHandler(this.installModToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(117, 6);
            // 
            // setActiveToolStripMenuItem
            // 
            this.setActiveToolStripMenuItem.Name = "setActiveToolStripMenuItem";
            this.setActiveToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.setActiveToolStripMenuItem.Text = "Set Active";
            this.setActiveToolStripMenuItem.Click += new System.EventHandler(this.setActiveToolStripMenuItem_Click);
            // 
            // updateModToolStripMenuItem
            // 
            this.updateModToolStripMenuItem.Name = "updateModToolStripMenuItem";
            this.updateModToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.updateModToolStripMenuItem.Text = "Update Mod";
            this.updateModToolStripMenuItem.Click += new System.EventHandler(this.updateModToolStripMenuItem_Click);
            // 
            // removeModToolStripMenuItem
            // 
            this.removeModToolStripMenuItem.Name = "removeModToolStripMenuItem";
            this.removeModToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.removeModToolStripMenuItem.Text = "Remove Mod";
            this.removeModToolStripMenuItem.Click += new System.EventHandler(this.removeModToolStripMenuItem_Click);
            // 
            // modManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 257);
            this.Controls.Add(this.gbxModActions);
            this.Controls.Add(this.gbxMods);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "modManager";
            this.Text = "Mod Manager";
            this.Load += new System.EventHandler(this.modManager_Load);
            this.gbxMods.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.modsGridView)).EndInit();
            this.gbxModActions.ResumeLayout(false);
            this.modMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxMods;
        private System.Windows.Forms.DataGridView modsGridView;
        private System.Windows.Forms.GroupBox gbxModActions;
        private System.Windows.Forms.Button btnUpdateAll;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnInstallAll;
        private System.Windows.Forms.ContextMenuStrip modMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem installModToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateModToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeModToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button btnUninstallAll;
        private System.Windows.Forms.ToolStripMenuItem setActiveToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn version;
        private System.Windows.Forms.DataGridViewTextBoxColumn author;
        private System.Windows.Forms.DataGridViewTextBoxColumn status;
        private System.Windows.Forms.CheckBox ckbUpdate;
    }
}