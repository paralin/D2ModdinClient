// <copyright file="ProcessIcon.cs">
// Copyright (c) 2014 All Right Reserved
//
// This source is subject to the License.
// Please see the License.txt file for more information.
// All other rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Christian Stewart</author>
// <email>kidovate@gmail.com</email>
// <date>2014-05-10</date>
// <summary>D2Moddin Tray Icon Functions</summary>
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using d2mp.Properties;

namespace d2mp
{
    /// <summary>
    /// 
    /// </summary>
    class ProcessIcon : IDisposable
    {
        /// <summary>
        /// The NotifyIcon object.
        /// </summary>
        NotifyIcon ni;
        public Action showNotification { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIcon"/> class.
        /// </summary>
        public ProcessIcon()
        {
            // Instantiate the NotifyIcon object.
            ni = new NotifyIcon();
        }

        /// <summary>
        /// Displays the icon in the system tray.
        /// </summary>
        public void Display()
        {
            // Put the icon in the system tray and allow it react to mouse clicks.			
            ni.MouseClick += new MouseEventHandler(ni_MouseClick);
            ni.Icon = Resources.D2MPIconSmall;
            ni.Text = "D2Moddin Manager\nClick to view last received notification.";
            ni.Visible = true;

            // Attach a context menu.
            ni.ContextMenuStrip = new ContextMenus().Create();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            // When the application closes, this will remove the icon from the system tray immediately.
            ni.Dispose();
        }

        /// <summary>
        /// Handles the MouseClick event of the ni control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        void ni_MouseClick(object sender, MouseEventArgs e)
        {
            // Handle mouse button clicks.
            if (e.Button == MouseButtons.Left)
            {
                showNotification();
            }
        }

        public void DisplayBubble(string msg)
        {
            ni.ShowBalloonTip(2000, "D2Moddin", msg, ToolTipIcon.None);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class ContextMenus
    {
        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ContextMenuStrip</returns>
        public ContextMenuStrip Create()
        {
            // Add the default menu options.
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item;
            ToolStripSeparator sep;

            menu.ShowImageMargin = false;

            // Restart.
            item = new ToolStripMenuItem();
            item.Text = "Restart";
            item.Click += new EventHandler(Restart_Click);
            menu.Items.Add(item);

            // Uninstall.
            item = new ToolStripMenuItem();
            item.Text = "Uninstall";
            item.Click += new EventHandler(Uninstall_Click);
            menu.Items.Add(item);

            // Mod Manager.
            item = new ToolStripMenuItem();
            item.Text = "Mod Manager";
            item.Click += new EventHandler(ModManager_Click);
            menu.Items.Add(item);

            // Preferences
            item = new ToolStripMenuItem();
            item.Text = "Preferences";
            item.Click += new EventHandler(Preferences_Click);
            menu.Items.Add(item);

            // Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            // About
            item = new ToolStripMenuItem();
            item.Text = "About";
            item.Click += new EventHandler(About_Click);
            menu.Items.Add(item);

            // Exit.
            item = new ToolStripMenuItem();
            item.Text = "Exit";
            item.Click += new System.EventHandler(Exit_Click);
            //item.Image = Resources.Exit;
            menu.Items.Add(item);

            return menu;
        }

        private void Preferences_Click(object sender, EventArgs e)
        {
            D2MP.showPreferences();
        }

        private void Uninstall_Click(object sender, EventArgs e)
        {
            D2MP.Uninstall();
        }

        private void ModManager_Click(object sender, EventArgs e)
        {
            D2MP.showModManager();
        }

        private void Restart_Click(object sender, EventArgs e)
        {
            D2MP.Restart();
        }

        private void About_Click(object sender, EventArgs e)
        {
            D2MP.showCredits();
        }

        /// <summary>
        /// Processes a menu item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            D2MP.shutDown = true;
        }
    }
}
