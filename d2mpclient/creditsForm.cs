// 
// creditsForm.cs
// Created by ilian000 on 2014-06-14
// Licenced under the Apache License, Version 2.0
//
      
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace d2mp
{
    public partial class creditsForm : Form
    {
        public creditsForm()
        {
            InitializeComponent();
        }

        private void creditsForm_Load(object sender, EventArgs e)
        {
            txtVersion.Text += ClientCommon.Version.ClientVersion;
            LinkLabel.Link kidovate_link = new LinkLabel.Link(13, 8);
            LinkLabel.Link ilian000_link = new LinkLabel.Link(26, 8);
            LinkLabel.Link licence_link = new LinkLabel.Link(71, 27);

            kidovate_link.Name = "kidovate";
            ilian000_link.Name = "ilian000";
            licence_link.Name = "licence";
            
            creditsLinkLabel.Links.Add(kidovate_link);
            creditsLinkLabel.Links.Add(ilian000_link);
            creditsLinkLabel.Links.Add(licence_link);

            creditsLinkLabel.Text += Environment.UserName;
        }

        private void creditsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            switch (e.Link.Name)
            {
                case "kidovate":
                    System.Diagnostics.Process.Start("http://github.com/kidovate");
                    break;
                case "ilian000":
                    System.Diagnostics.Process.Start("http://github.com/ilian000");
                    break;
                case "licence":
                    System.Diagnostics.Process.Start("http://www.apache.org/licenses/LICENSE-2.0.txt");
                    break;
                default:
                    break;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
