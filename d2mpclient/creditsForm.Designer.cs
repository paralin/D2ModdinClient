namespace d2mp
{
    partial class creditsForm
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
            this.lblSeperator = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtVersion = new System.Windows.Forms.Label();
            this.creditsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.btnOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSeperator
            // 
            this.lblSeperator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeperator.Location = new System.Drawing.Point(12, 104);
            this.lblSeperator.Name = "lblSeperator";
            this.lblSeperator.Size = new System.Drawing.Size(458, 2);
            this.lblSeperator.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::d2mp.Properties.Resources.D2ModdinLogo;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(443, 89);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // txtVersion
            // 
            this.txtVersion.Location = new System.Drawing.Point(13, 110);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.Size = new System.Drawing.Size(230, 30);
            this.txtVersion.TabIndex = 6;
            this.txtVersion.Text = "D2Modd.in Windows Client\r\nVersion ";
            // 
            // creditsLinkLabel
            // 
            this.creditsLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.creditsLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.creditsLinkLabel.Location = new System.Drawing.Point(13, 149);
            this.creditsLinkLabel.Name = "creditsLinkLabel";
            this.creditsLinkLabel.Size = new System.Drawing.Size(457, 53);
            this.creditsLinkLabel.TabIndex = 8;
            this.creditsLinkLabel.Text = "Developed by kidovate and ilian000\r\nThis product is Licenced under the Apache Lic" +
    "ense, Version 2.0 to: ";
            this.creditsLinkLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.creditsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.creditsLinkLabel_LinkClicked);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(400, 205);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 9;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // creditsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 237);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.creditsLinkLabel);
            this.Controls.Add(this.txtVersion);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblSeperator);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "creditsForm";
            this.ShowInTaskbar = false;
            this.Text = "About D2Modd.in Client";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.creditsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblSeperator;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label txtVersion;
        private System.Windows.Forms.LinkLabel creditsLinkLabel;
        private System.Windows.Forms.Button btnOk;
    }
}