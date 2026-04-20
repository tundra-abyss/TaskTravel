using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TaskTravel
{
    public class MainForm : Form
    {
        Timer updateTimer;
        FlowLayoutPanel panel;
        Dictionary<IntPtr, TaskButton> buttons = new();

        public MainForm()
        {
            InitializeWindow();
            BuildPanel();
            StartUpdater();
        }

        void InitializeWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Height = 48;
            Width = 800;
            BackColor = Color.Magenta;
            TransparencyKey = Color.Magenta;
            SetRoundedCorners(14);
            Win32.EnableBlur(this.Handle);
            PositionAtBottomCenter();
        }

        void BuildPanel()
        {
            panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                Padding = new Padding(8, 6, 8, 6),
            };
            Controls.Add(panel);
        }

        void StartUpdater()
        {
            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += (s, e) => RefreshWindowList();
            updateTimer.Start();
            RefreshWindowList();
        }

        void RefreshWindowList()
        {
            var topWindows = Win32.EnumTopLevelWindows()
                .Where(Win32.IsVisibleAppWindow)
                .ToList();

            foreach (var h in topWindows)
            {
                if (!buttons.ContainsKey(h))
                {
                    var btn = new TaskButton(h);
                    btn.Click += (s, e) => ActivateWindow(h);
                    buttons[h] = btn;
                    panel.Controls.Add(btn.ButtonControl);
                }
            }

            var toRemove = buttons.Keys.Except(topWindows).ToList();
            foreach (var h in toRemove)
            {
                var tb = buttons[h];
                panel.Controls.Remove(tb.ButtonControl);
                tb.Dispose();
                buttons.Remove(h);
            }

            panel.PerformLayout();
            Width = Math.Min(Screen.PrimaryScreen.WorkingArea.Width - 40, panel.PreferredSize.Width + 40);
            PositionAtBottomCenter();
        }

        void ActivateWindow(IntPtr h)
        {
            Win32.RestoreAndActivateWindow(h);
        }

        void PositionAtBottomCenter()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            Left = screen.Left + (screen.Width - Width) / 2;
            Top = screen.Bottom - Height - 8;
        }

        void SetRoundedCorners(int radius)
        {
            var r = Win32.CreateRoundRectRgn(0, 0, Width + 1, Height + 1, radius, radius);
            Win32.SetWindowRgn(this.Handle, r, true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRoundedCorners(14);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            foreach (var kv in buttons) kv.Value.Dispose();
            base.OnFormClosing(e);
        }
    }

    class TaskButton : IDisposable
    {
        public Control ButtonControl { get; }
        IntPtr windowHandle;

        public event EventHandler Click;

        public TaskButton(IntPtr hWnd)
        {
            windowHandle = hWnd;
            var pic = new PictureBox
            {
                Size = new Size(36, 36),
                Margin = new Padding(6, 0, 6, 0),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
            };

            var ico = Win32.GetSmallWindowIcon(hWnd);
            if (ico != null) pic.Image = ico.ToBitmap();
            else pic.Image = SystemIcons.Application.ToBitmap();

            pic.Click += (s, e) => Click?.Invoke(this, EventArgs.Empty);

            ButtonControl = pic;
        }

        public void Dispose()
        {
            if (ButtonControl != null) ButtonControl.Dispose();
        }
    }
}
