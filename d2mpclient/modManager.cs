// 
// modManager.cs
// Created by ilian000 on 2014-06-13
// Licenced under the Apache License, Version 2.0
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace d2mp
{
    public partial class modManager : Form
    {
        public modManager()
        {
            InitializeComponent();
        }

        private void modManager_Load(object sender, EventArgs e)
        {
            refreshTable();
            ckbUpdate.Checked = Settings.autoUpdateMods;
        }
        public void refreshTable()
        {
            modsGridView.Rows.Clear();
            modController.getLocalMods();
            List<RemoteMod> remoteMods;
            try
            {
                remoteMods = modController.getRemoteMods();

            }
            catch (Exception)
            {
                MessageBox.Show("Could not connect to the update server.");
                Close();
                throw;
            }
            List<RemoteMod> needsUpdate = modController.checkUpdates();
            var activeMod = D2MP.GetActiveMod();
            foreach (var mod in remoteMods)
            {
                int rowIndex = modsGridView.Rows.Add(new Object[] { mod.fullname, mod.version, mod.author, "Up to date" });
                DataGridViewRow row = modsGridView.Rows[rowIndex];
                row.Tag = mod;
                if (activeMod != null && mod.name == activeMod.name)
                {
                        row.Cells[3].Value = "Active mod";
                }
                if (mod.needsUpdate)
                {
                    DataGridViewCellStyle boldStyle = new DataGridViewCellStyle();
                    boldStyle.Font = new Font(modsGridView.Font, FontStyle.Bold);
                    row.DefaultCellStyle = boldStyle;
                    row.Cells[3].Value = "Needs update";
                }
                else{
                    DataGridViewCellStyle normalStyle = new DataGridViewCellStyle();
                    //normalStyle.Font = new Font(modsGridView.Font, FontStyle.Regular);
                    modsGridView.Rows[rowIndex].DefaultCellStyle = normalStyle;
                }
                if (mod.needsInstall)
                {
                    modsGridView.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Gray;
                    row.Cells[3].Value = "Not installed";
                }
                else
                {
                    modsGridView.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Black;
                }
                //modsGridView.Rows[rowIndex].DefaultCellStyle = boldStyle;
            }
            modsGridView.CurrentRow.Selected = false;
            if (needsUpdate.Count > 0)
            {
                btnUpdateAll.Text = String.Format("Update All ({0})", needsUpdate.Count);
                btnUpdateAll.Enabled = true;
            }
            else
            {
                btnUpdateAll.Text = "Update All";
                btnUpdateAll.Enabled = false;
            }
        }

        private void btnUpdateAll_Click(object sender, EventArgs e)
        {
            List<RemoteMod> needsUpdate = modController.checkUpdates();
            foreach (var mod in needsUpdate)
            {
                var parameterMod = new ClientCommon.Data.ClientMod { name=mod.name };
                D2MP.DeleteMod(new ClientCommon.Methods.DeleteMod { Mod = parameterMod });

                if (!modController.installQueue.Contains(mod))
                    modController.installQueue.Enqueue(mod);
            }
            btnUpdateAll.Text = "Update All";
            btnUpdateAll.Enabled = false;
            modController.InstallQueued();
            refreshTable();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshTable();
        }

        private void btnInstallAll_Click(object sender, EventArgs e)
        {
            var remoteMods = modController.getRemoteMods();
            foreach (var mod in remoteMods.Where(rMod=>rMod.needsInstall))
            {
                if (!modController.installQueue.Contains(mod))
                    modController.installQueue.Enqueue(mod);
            }
            modController.InstallQueued();
        }

        private void modsGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.Button == MouseButtons.Right)
            {
                foreach (DataGridViewRow item in this.modsGridView.Rows)
                {
                    item.Selected = false;
                }
                this.modsGridView.Rows[e.RowIndex].Selected = true;
                modsGridView.CurrentCell = modsGridView.Rows[e.RowIndex].Cells[0];
                Rectangle r = modsGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                modMenuStrip.Show((Control)sender, r.Left + e.X, r.Top + e.Y);
                installModToolStripMenuItem.Enabled = (((RemoteMod)modsGridView.SelectedRows[0].Tag).needsInstall);
                updateModToolStripMenuItem.Enabled = (((RemoteMod)modsGridView.SelectedRows[0].Tag).needsUpdate);
                removeModToolStripMenuItem.Enabled = !(((RemoteMod)modsGridView.SelectedRows[0].Tag).needsInstall);
                setActiveToolStripMenuItem.Enabled = !(((RemoteMod)modsGridView.SelectedRows[0].Tag).needsInstall);
            }
        }

        private void installModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mod = (RemoteMod) modsGridView.SelectedRows[0].Tag;

            if (!modController.installQueue.Contains(mod))
                modController.installQueue.Enqueue(mod);
            
            modController.InstallQueued();
        }

        private void updateModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mod = (RemoteMod)modsGridView.SelectedRows[0].Tag;
            var parameterMod = new ClientCommon.Data.ClientMod { name = mod.name };
            D2MP.DeleteMod(new ClientCommon.Methods.DeleteMod { Mod = parameterMod });
            if (!modController.installQueue.Contains(mod))
                modController.installQueue.Enqueue(mod);

            modController.InstallQueued();
        }

        private void removeModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mod = (RemoteMod)modsGridView.SelectedRows[0].Tag;
            var parameterMod = new ClientCommon.Data.ClientMod { name = mod.name };
            D2MP.DeleteMod(new ClientCommon.Methods.DeleteMod { Mod = parameterMod });
        }

        private void btnUninstallAll_Click(object sender, EventArgs e)
        {
            var clientMods = new List<ClientCommon.Data.ClientMod>(modController.getLocalMods());
            foreach (var mod in clientMods)
            {
                D2MP.DeleteMod(new ClientCommon.Methods.DeleteMod { Mod = mod });
            }
        }

        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mod = (RemoteMod)modsGridView.SelectedRows[0].Tag;
            D2MP.SetMod(new ClientCommon.Methods.SetMod { Mod = new ClientCommon.Data.ClientMod { name = mod.name, version = mod.version } });
        }

        private void ckbUpdate_CheckedChanged(object sender, EventArgs e)
        {
            Settings.autoUpdateMods = ckbUpdate.Checked;
        }
    }
}
