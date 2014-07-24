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
using System.Threading;
using System.Windows.Forms;

namespace d2mp
{
    public enum NotificationType : int
    {
        None = 0,
        Success = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Progress = 5,
        TryAgain = 6
    }

    public partial class notificationForm : Form
    {
        private static readonly Dictionary<NotificationType, Bitmap> NotificationIcons = new Dictionary<NotificationType, Bitmap>()
        {
            {NotificationType.None, null},
            {NotificationType.Success, new Bitmap(Properties.Resources.icon_success)},
            {NotificationType.Info, new Bitmap(Properties.Resources.icon_info)},
            {NotificationType.Warning, new Bitmap(Properties.Resources.icon_warning)},
            {NotificationType.Error, new Bitmap(Properties.Resources.icon_error)},
            {NotificationType.Progress, new Bitmap(Properties.Resources.icon_info)},
            {NotificationType.TryAgain, new Bitmap(Properties.Resources.icon_warning)}
        };

        private static readonly Dictionary<NotificationType, Color> NotificationColors = new Dictionary<NotificationType, Color>()
        {
            {NotificationType.None, Color.FromArgb(91, 192, 222)},
            {NotificationType.Success, Color.FromArgb(67, 172, 106)},
            {NotificationType.Info, Color.FromArgb(91, 192, 222)},
            {NotificationType.Warning, Color.FromArgb(233, 144, 2)},
            {NotificationType.Error, Color.FromArgb(240, 65, 36)},
            {NotificationType.Progress, Color.FromArgb(91, 192, 222)},
            {NotificationType.TryAgain, Color.FromArgb(233, 144, 2)}
        };

        private Action mTryAgain = null;
        private Action mCancel = null;
        private Action mDownload = null;

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
            this.Size = this.MinimumSize;
        }

        /// <summary>
        /// Shows notification for a limited time.
        /// </summary>
        /// <param name="type">Type of notification.</param>
        /// <param name="title">Title displayed on notification window</param>
        /// <param name="message">Message displayed on notification window</param>
        public void Notify(NotificationType type, string title, string message)
        {
            this.InvokeIfRequired(() =>
            {
                hideTimer.Stop();

                notifierProgress.Hide();
                pnlTryAgain.Hide();

                BackColor = NotificationColors[type];
                icon.Image = NotificationIcons[type];

                lblTitle.Text = title;
                lblMsg.Text = message;
                this.Opacity = 1;
                this.Size = this.MinimumSize;

                switch (type)
                {
                    case NotificationType.Progress:
                        notifierProgress.Value = 0;
                        notifierProgress.Show();
                        break;
                    case NotificationType.TryAgain:
                        pnlTryAgain.Show();
                        this.Size = this.MaximumSize;
                        break;
                    default:
                        hideTimer.Start();
                        break;
                }

                SetupLocation();
            });
        }

        /// <summary>
        /// Open a Try Again notification
        /// </summary>
        /// <param name="title">Title displayed on notification window</param>
        /// <param name="message">Message displayed on notification window</param>
        /// <param name="TryAgain">Action when user clicks Try Again</param>
        /// <param name="Cancel">Action when user clicks Cancel</param>
        /// <param name="Download">Action when user clicks Download</param>
        public void NotifyTryAgain(string title, string message, Action TryAgain, Action Cancel, Action Download)
        {
            this.InvokeIfRequired(() =>
            {
                mTryAgain = TryAgain;
                mCancel = Cancel;
                mDownload = Download;

                Notify(NotificationType.TryAgain, title, message);
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
            SetupLocation();
            if (Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor <= 1))
            {
                FormBorderStyle = FormBorderStyle.None;
            }
            hideTimer.Start();
        }

        private void SetupLocation()
        {
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width - 10, Screen.PrimaryScreen.WorkingArea.Height - Height - 10);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3; // Prevent double click on border
            const int WM_NCHITTEST = 0x0084; // Prevent resize cursors

            switch (m.Msg)
            {
                case WM_NCLBUTTONDBLCLK:
                    m.Result = IntPtr.Zero;
                    return;
                case WM_NCHITTEST:
                    m.Result = IntPtr.Zero;
                    return;
            }
            base.WndProc(ref m);
        }

        private void btnTryAgain_Click(object sender, EventArgs e)
        {
            Fade(0);
            if (mTryAgain != null) mTryAgain();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Fade(0);
            if (mCancel != null) mCancel();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            Fade(0);
            if (mDownload != null) mDownload();
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
        catch (ObjectDisposedException)
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