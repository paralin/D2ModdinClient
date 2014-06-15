// 
// notificationForm.cs
// Created by ilian000 on 2014-06-06
// Licenced under the Apache License, Version 2.0
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace d2mp
{
    public partial class notificationForm : Form
    {
        private Color successBg = Color.FromArgb(67, 172, 106);
        private Color infoBg = Color.FromArgb(91, 192, 222);
        private Color warningBg = Color.FromArgb(233, 144, 2);
        private Color errorBg = Color.FromArgb(240, 65, 36);
        private Bitmap successIcon = new Bitmap(Properties.Resources.icon_success);
        private Bitmap infoIcon = new Bitmap(Properties.Resources.icon_info);
        private Bitmap warningIcon = new Bitmap(Properties.Resources.icon_warning);
        private Bitmap errorIcon = new Bitmap(Properties.Resources.icon_error);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public notificationForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows notification for a limited time.
        /// </summary>
        /// <param name="type">Type of notification. 1: success, 2: info, 3: warning, 4: error, 5: progress</param>
        /// <param name="title">Title displayed on notification window</param>
        /// <param name="message">Message displayed on notification window</param>
        public void Notify(int type, string title, string message)
        {
            this.InvokeIfRequired(() =>
            {
                notifierProgress.Hide();
                switch (type)
                {
                    case 1:
                        BackColor = successBg;
                        icon.Image = successIcon;
                        break;
                    case 2:
                        BackColor = infoBg;
                        icon.Image = infoIcon;
                        break;
                    case 3:
                        BackColor = warningBg;
                        icon.Image = warningIcon;
                        break;
                    case 4:
                        BackColor = errorBg;
                        icon.Image = errorIcon;
                        break;
                    case 5:
                        BackColor = infoBg;
                        icon.Image = infoIcon;
                        notifierProgress.Value = 0;
                        notifierProgress.Show();
                        break;
                    default:
                        BackColor = successBg;
                        break;
                }
                lblTitle.Text = title;
                lblMsg.Text = message;
                this.Opacity = 1;
                // Resets the timer -- Looks damn stupid
                hideTimer.Stop();
                if(type != 5)
                    hideTimer.Start();
            });
        }

        public void reportProgress(int value)
        {
            this.InvokeIfRequired(() =>
            {
                notifierProgress.Value = value;
            });
        }

        public void Fade(double opacity)
        {
            double toFade = this.Opacity - opacity;
            while (Math.Abs(this.Opacity - opacity) > 0.01)
            {
                this.Opacity -= toFade / 50;
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
        }

        private void hideTimer_Tick(object sender, EventArgs e)
        {
            this.InvokeIfRequired(() =>
            {
                Fade(0);
            });
            hideTimer.Stop();
        }

        private void Notification_Form_Load(object sender, EventArgs e)
        {
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width - 10, Screen.PrimaryScreen.WorkingArea.Height - Height - 10);
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            hideTimer.Start();
        }
    }
}

public static class ControlExtensions
{
    public static void InvokeIfRequired(this Control control, Action action)
    {
        try
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
                return;
            }
            action();
        }
        catch ( ObjectDisposedException )
        {
            // Control is disposed.
        }
    }
    public static TResult InvokeIfRequired<TResult>(this Control control, Func<TResult> func)
    {
        if (control.InvokeRequired)
        {
            return (TResult)control.Invoke(func);
        }
        return func();
    }
}