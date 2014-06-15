namespace d2mp
{
    partial class settingsForm
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
        public void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(settingsForm));
            this.gbxSteamDir = new System.Windows.Forms.GroupBox();
            this.btnChangeSteamDir = new System.Windows.Forms.Button();
            this.txtSteamDir = new System.Windows.Forms.TextBox();
            this.gbxDotaDir = new System.Windows.Forms.GroupBox();
            this.btnChangeDotaDir = new System.Windows.Forms.Button();
            this.txtDotaDir = new System.Windows.Forms.TextBox();
            this.gbxExtra = new System.Windows.Forms.GroupBox();
            this.btnResetSettings = new System.Windows.Forms.Button();
            this.btnViewLog = new System.Windows.Forms.Button();
            this.lblSeperator = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.gbxLog = new System.Windows.Forms.GroupBox();
            this.gbxSteamDir.SuspendLayout();
            this.gbxDotaDir.SuspendLayout();
            this.gbxExtra.SuspendLayout();
            this.gbxLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxSteamDir
            // 
            this.gbxSteamDir.Controls.Add(this.btnChangeSteamDir);
            this.gbxSteamDir.Controls.Add(this.txtSteamDir);
            this.gbxSteamDir.Location = new System.Drawing.Point(12, 12);
            this.gbxSteamDir.Name = "gbxSteamDir";
            this.gbxSteamDir.Size = new System.Drawing.Size(458, 62);
            this.gbxSteamDir.TabIndex = 0;
            this.gbxSteamDir.TabStop = false;
            this.gbxSteamDir.Text = "Steam Location";
            // 
            // btnChangeSteamDir
            // 
            this.btnChangeSteamDir.Location = new System.Drawing.Point(368, 19);
            this.btnChangeSteamDir.Name = "btnChangeSteamDir";
            this.btnChangeSteamDir.Size = new System.Drawing.Size(84, 29);
            this.btnChangeSteamDir.TabIndex = 0;
            this.btnChangeSteamDir.Text = "Change...";
            this.btnChangeSteamDir.UseVisualStyleBackColor = true;
            this.btnChangeSteamDir.Click += new System.EventHandler(this.btnChangeSteamDir_Click);
            // 
            // txtSteamDir
            // 
            this.txtSteamDir.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSteamDir.Location = new System.Drawing.Point(6, 19);
            this.txtSteamDir.Name = "txtSteamDir";
            this.txtSteamDir.ReadOnly = true;
            this.txtSteamDir.Size = new System.Drawing.Size(356, 29);
            this.txtSteamDir.TabIndex = 0;
            this.txtSteamDir.TabStop = false;
            // 
            // gbxDotaDir
            // 
            this.gbxDotaDir.Controls.Add(this.btnChangeDotaDir);
            this.gbxDotaDir.Controls.Add(this.txtDotaDir);
            this.gbxDotaDir.Location = new System.Drawing.Point(12, 80);
            this.gbxDotaDir.Name = "gbxDotaDir";
            this.gbxDotaDir.Size = new System.Drawing.Size(458, 62);
            this.gbxDotaDir.TabIndex = 1;
            this.gbxDotaDir.TabStop = false;
            this.gbxDotaDir.Text = "Dota 2 Location";
            // 
            // btnChangeDotaDir
            // 
            this.btnChangeDotaDir.Location = new System.Drawing.Point(368, 19);
            this.btnChangeDotaDir.Name = "btnChangeDotaDir";
            this.btnChangeDotaDir.Size = new System.Drawing.Size(84, 29);
            this.btnChangeDotaDir.TabIndex = 1;
            this.btnChangeDotaDir.Text = "Change...";
            this.btnChangeDotaDir.UseVisualStyleBackColor = true;
            this.btnChangeDotaDir.Click += new System.EventHandler(this.btnChangeDotaDir_Click);
            // 
            // txtDotaDir
            // 
            this.txtDotaDir.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDotaDir.Location = new System.Drawing.Point(6, 19);
            this.txtDotaDir.Name = "txtDotaDir";
            this.txtDotaDir.ReadOnly = true;
            this.txtDotaDir.Size = new System.Drawing.Size(356, 29);
            this.txtDotaDir.TabIndex = 2;
            this.txtDotaDir.TabStop = false;
            // 
            // gbxExtra
            // 
            this.gbxExtra.Controls.Add(this.btnResetSettings);
            this.gbxExtra.Controls.Add(this.btnViewLog);
            this.gbxExtra.Location = new System.Drawing.Point(12, 149);
            this.gbxExtra.Name = "gbxExtra";
            this.gbxExtra.Size = new System.Drawing.Size(214, 65);
            this.gbxExtra.TabIndex = 2;
            this.gbxExtra.TabStop = false;
            this.gbxExtra.Text = "Additional Preferences";
            // 
            // btnResetSettings
            // 
            this.btnResetSettings.Location = new System.Drawing.Point(106, 20);
            this.btnResetSettings.Name = "btnResetSettings";
            this.btnResetSettings.Size = new System.Drawing.Size(93, 34);
            this.btnResetSettings.TabIndex = 1;
            this.btnResetSettings.Text = "Reset Settings";
            this.btnResetSettings.UseVisualStyleBackColor = true;
            this.btnResetSettings.Click += new System.EventHandler(this.btnResetSettings_Click);
            // 
            // btnViewLog
            // 
            this.btnViewLog.Location = new System.Drawing.Point(7, 19);
            this.btnViewLog.Name = "btnViewLog";
            this.btnViewLog.Size = new System.Drawing.Size(93, 34);
            this.btnViewLog.TabIndex = 0;
            this.btnViewLog.Text = "View Log";
            this.btnViewLog.UseVisualStyleBackColor = true;
            this.btnViewLog.Click += new System.EventHandler(this.btnViewLog_Click);
            // 
            // lblSeperator
            // 
            this.lblSeperator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeperator.Location = new System.Drawing.Point(12, 222);
            this.lblSeperator.Name = "lblSeperator";
            this.lblSeperator.Size = new System.Drawing.Size(458, 2);
            this.lblSeperator.TabIndex = 3;
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtLog.Location = new System.Drawing.Point(6, 19);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(446, 156);
            this.txtLog.TabIndex = 4;
            // 
            // gbxLog
            // 
            this.gbxLog.Controls.Add(this.txtLog);
            this.gbxLog.Location = new System.Drawing.Point(12, 227);
            this.gbxLog.Name = "gbxLog";
            this.gbxLog.Size = new System.Drawing.Size(458, 181);
            this.gbxLog.TabIndex = 5;
            this.gbxLog.TabStop = false;
            this.gbxLog.Text = "Application Log";
            // 
            // settingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 420);
            this.Controls.Add(this.gbxLog);
            this.Controls.Add(this.lblSeperator);
            this.Controls.Add(this.gbxExtra);
            this.Controls.Add(this.gbxDotaDir);
            this.Controls.Add(this.gbxSteamDir);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "settingsForm";
            this.Text = "D2Modd.in Preferences";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.settingsForm_FormClosing);
            this.Load += new System.EventHandler(this.settingsForm_Load);
            this.Shown += new System.EventHandler(this.settingsForm_Shown);
            this.gbxSteamDir.ResumeLayout(false);
            this.gbxSteamDir.PerformLayout();
            this.gbxDotaDir.ResumeLayout(false);
            this.gbxDotaDir.PerformLayout();
            this.gbxExtra.ResumeLayout(false);
            this.gbxLog.ResumeLayout(false);
            this.gbxLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxSteamDir;
        private System.Windows.Forms.TextBox txtSteamDir;
        private System.Windows.Forms.Button btnChangeSteamDir;
        private System.Windows.Forms.GroupBox gbxDotaDir;
        private System.Windows.Forms.Button btnChangeDotaDir;
        private System.Windows.Forms.TextBox txtDotaDir;
        private System.Windows.Forms.GroupBox gbxExtra;
        private System.Windows.Forms.Button btnViewLog;
        private System.Windows.Forms.Button btnResetSettings;
        private System.Windows.Forms.Label lblSeperator;
        private System.Windows.Forms.GroupBox gbxLog;
        private System.Windows.Forms.TextBox txtLog;
    }
}