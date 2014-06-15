// 
// settinfsForm.cs
// Created by ilian000 on 2014-06-08
// Licenced under the Apache License, Version 2.0
//

using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace d2mp
{
    public partial class settingsForm : Form
    {
        public settingsForm()
        {
            InitializeComponent();
        }

        private void settingsForm_Load(object sender, EventArgs e)
        {
            refreshSettings();
        }

        private void settingsForm_Shown(object sender, EventArgs e)
        {
            txtLog.AppendText(logKeeper.log);
        }

        private void refreshSettings()
        {
            txtSteamDir.Text = Settings.steamDir;
            txtDotaDir.Text = Settings.dotaDir;
        }

        private void btnChangeSteamDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fDialog = new FolderBrowserDialog();
            fDialog.Description = "Please select your Steam directory.";
            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(Path.Combine(fDialog.SelectedPath, @"config\config.vdf")))
                {
                    Settings.steamDir = fDialog.SelectedPath;
                    refreshSettings();
                }
                else
                {
                    MessageBox.Show(this, "Selected directory is not a valid Steam directory.", "Incorrect folder specified." ,MessageBoxButtons.OK,  MessageBoxIcon.Error);
                }
            }
        }

        private void btnChangeDotaDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fDialog = new FolderBrowserDialog();
            fDialog.Description = "Please select your Dota 2 directory.";
            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(Path.Combine(fDialog.SelectedPath, @"dota/gameinfo.txt")))
                {
                    Settings.dotaDir = fDialog.SelectedPath;
                    refreshSettings();
                }
                else
                {
                    MessageBox.Show("Selected directory is not a valid Dota 2 directory.", "Incorrect folder specified.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "d2mp.log"));
        }

        private void btnResetSettings_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show("Are you sure you want to reset all settings of the client and restart it?", "Reset confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.Yes)
            {
                Settings.Reset();
                D2MP.Restart();
            }
        }


        private void settingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!D2MP.shutDown) {
                Hide();
                e.Cancel = true;
            }
        }

    }
    public class TextBoxAppender : AppenderSkeleton
    {
        private Form form;
        private TextBox _textBox;
        public string FormName { get; set; }
        public string TextBoxName { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_textBox == null)
            {
                if (String.IsNullOrEmpty(FormName) ||
                    String.IsNullOrEmpty(TextBoxName))
                    return;

                form = Application.OpenForms[FormName];
                if (form == null)
                {
                    logKeeper.log += loggingEvent.RenderedMessage + Environment.NewLine;
                    return;
                }

                _textBox = form.Controls["gbxLog"].Controls[TextBoxName] as TextBox;
                if (_textBox == null)
                    return;

                form.FormClosing += (s, e) => _textBox = null;
            }
            _textBox.Invoke((MethodInvoker)delegate {
                _textBox.AppendText(loggingEvent.RenderedMessage + Environment.NewLine);
                _textBox.SelectionStart = _textBox.Text.Length;
                _textBox.ScrollToCaret();
            });
        }
    }
}
