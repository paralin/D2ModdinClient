namespace d2mp
{
    partial class notificationForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.icon = new System.Windows.Forms.PictureBox();
            this.lblMsg = new System.Windows.Forms.Label();
            this.hideTimer = new System.Windows.Forms.Timer(this.components);
            this.notifierProgress = new System.Windows.Forms.ProgressBar();
            this.btnTryAgain = new System.Windows.Forms.Button();
            this.pnlTryAgain = new System.Windows.Forms.Panel();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.icon)).BeginInit();
            this.pnlTryAgain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Light", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(44, 7);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(108, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Starting up";
            // 
            // icon
            // 
            this.icon.Image = global::d2mp.Properties.Resources.icon_info;
            this.icon.Location = new System.Drawing.Point(12, 11);
            this.icon.Name = "icon";
            this.icon.Size = new System.Drawing.Size(26, 26);
            this.icon.TabIndex = 1;
            this.icon.TabStop = false;
            // 
            // lblMsg
            // 
            this.lblMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMsg.Font = new System.Drawing.Font("Segoe UI Light", 12F);
            this.lblMsg.ForeColor = System.Drawing.Color.White;
            this.lblMsg.Location = new System.Drawing.Point(8, 43);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(314, 55);
            this.lblMsg.TabIndex = 1;
            this.lblMsg.Text = "Please wait while the client is connecting...";
            // 
            // hideTimer
            // 
            this.hideTimer.Interval = 5000;
            this.hideTimer.Tick += new System.EventHandler(this.hideTimer_Tick);
            // 
            // notifierProgress
            // 
            this.notifierProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.notifierProgress.Location = new System.Drawing.Point(11, 82);
            this.notifierProgress.Name = "notifierProgress";
            this.notifierProgress.Size = new System.Drawing.Size(310, 16);
            this.notifierProgress.TabIndex = 2;
            this.notifierProgress.Visible = false;
            // 
            // btnTryAgain
            // 
            this.btnTryAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTryAgain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTryAgain.ForeColor = System.Drawing.Color.White;
            this.btnTryAgain.Location = new System.Drawing.Point(8, 1);
            this.btnTryAgain.Name = "btnTryAgain";
            this.btnTryAgain.Size = new System.Drawing.Size(75, 23);
            this.btnTryAgain.TabIndex = 0;
            this.btnTryAgain.Text = "Try Again";
            this.btnTryAgain.UseVisualStyleBackColor = true;
            this.btnTryAgain.Click += new System.EventHandler(this.btnTryAgain_Click);
            // 
            // pnlTryAgain
            // 
            this.pnlTryAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTryAgain.Controls.Add(this.btnDownload);
            this.pnlTryAgain.Controls.Add(this.btnCancel);
            this.pnlTryAgain.Controls.Add(this.btnTryAgain);
            this.pnlTryAgain.Location = new System.Drawing.Point(15, 75);
            this.pnlTryAgain.Name = "pnlTryAgain";
            this.pnlTryAgain.Size = new System.Drawing.Size(306, 27);
            this.pnlTryAgain.TabIndex = 3;
            this.pnlTryAgain.Visible = false;
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(170, 1);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(133, 23);
            this.btnDownload.TabIndex = 2;
            this.btnDownload.Text = "Download Manually";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(89, 1);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // notificationForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(192)))), ((int)(((byte)(222)))));
            this.ClientSize = new System.Drawing.Size(334, 104);
            this.ControlBox = false;
            this.Controls.Add(this.pnlTryAgain);
            this.Controls.Add(this.notifierProgress);
            this.Controls.Add(this.lblMsg);
            this.Controls.Add(this.icon);
            this.Controls.Add(this.lblTitle);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(350, 120);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 100);
            this.Name = "notificationForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Notification_Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.icon)).EndInit();
            this.pnlTryAgain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.PictureBox icon;
        private System.Windows.Forms.Label lblMsg;
        public System.Windows.Forms.Timer hideTimer;
        private System.Windows.Forms.ProgressBar notifierProgress;
        private System.Windows.Forms.Button btnTryAgain;
        private System.Windows.Forms.Panel pnlTryAgain;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnCancel;
    }
}