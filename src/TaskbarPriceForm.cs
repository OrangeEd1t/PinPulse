using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CryptoMonitor
{
    internal sealed class TaskbarPriceForm : Form
    {
        private readonly Label priceLabel;
        private IntPtr taskbarHandle;
        private string taskbarAnchor = "left";
        private int taskbarOffsetX = 280;
        private int taskbarOffsetY;
        private int taskbarFixedWidth;
        private int taskbarMinWidth = 190;
        private int taskbarMaxWidth = 520;
        private bool isPositioning;
        private NativeRect lastWindowRect;
        private bool hasLastWindowRect;

        public TaskbarPriceForm()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.White;
            Height = 80;
            Width = 420;
            Padding = new Padding(8, 0, 8, 0);
            Text = "CryptoMonitor";

            priceLabel = new Label();
            priceLabel.AutoSize = false;
            priceLabel.Dock = DockStyle.Fill;
            priceLabel.TextAlign = ContentAlignment.MiddleCenter;
            priceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            priceLabel.ForeColor = Color.White;
            priceLabel.Text = "BTC --   ETH --";
            Controls.Add(priceLabel);

            Load += delegate { LogState("Load"); };
            Shown += delegate { LogState("Shown"); };
            VisibleChanged += delegate { LogState("VisibleChanged"); };
            FormClosing += delegate(object sender, FormClosingEventArgs e) { LogState("FormClosing " + e.CloseReason.ToString()); };
            FormClosed += delegate { LogState("FormClosed"); };
            Disposed += delegate { LogState("Disposed"); };
        }

        public void SetText(string text, bool isError)
        {
            if (IsDisposed)
            {
                return;
            }

            priceLabel.Text = text;
            priceLabel.ForeColor = isError ? Color.FromArgb(255, 130, 130) : Color.White;
            ResizeToText();
            EnsureShown();
        }

        public void ApplyConfig(AppConfig config)
        {
            if (config == null)
            {
                return;
            }

            taskbarAnchor = String.Equals(config.TaskbarAnchor, "right", StringComparison.OrdinalIgnoreCase)
                ? "right"
                : "left";
            taskbarOffsetX = config.TaskbarOffsetX;
            taskbarOffsetY = config.TaskbarOffsetY;
            taskbarFixedWidth = Clamp(config.TaskbarFixedWidth, 0, 4000);
            taskbarMinWidth = Clamp(config.TaskbarMinWidth, 80, 4000);
            taskbarMaxWidth = Clamp(Math.Max(config.TaskbarMaxWidth, taskbarMinWidth), 80, 4000);
            ResizeToText();
            EnsureShown();
        }

        public void SetMenu(ContextMenuStrip menu)
        {
            ContextMenuStrip = menu;
            priceLabel.ContextMenuStrip = menu;
        }

        public bool AttachToTaskbar()
        {
            IntPtr currentTaskbar = FindWindow("Shell_TrayWnd", null);
            if (currentTaskbar == IntPtr.Zero)
            {
                return false;
            }

            taskbarHandle = currentTaskbar;
            return true;
        }

        public void PositionInTaskbar()
        {
            if (IsDisposed || isPositioning)
            {
                return;
            }

            isPositioning = true;
            try
            {
            EnsureShown();
            if (!AttachToTaskbar())
            {
                PositionFallback();
                return;
            }

            NativeRect taskbarRect;
            if (!GetWindowRect(taskbarHandle, out taskbarRect))
            {
                PositionFallback();
                return;
            }

            NativeRect layoutRect = taskbarRect;
            Rectangle taskbarBounds = new Rectangle(
                taskbarRect.Left,
                taskbarRect.Top,
                Math.Max(1, taskbarRect.Right - taskbarRect.Left),
                Math.Max(1, taskbarRect.Bottom - taskbarRect.Top));
            Rectangle workingArea = Screen.FromRectangle(taskbarBounds).WorkingArea;
            int taskbarWidth = Math.Max(1, layoutRect.Right - layoutRect.Left);
            int taskbarHeight = Math.Max(1, layoutRect.Bottom - layoutRect.Top);
            bool horizontal = taskbarWidth >= taskbarHeight;

            int width = Width;
            int height = Height;
            int rightLimit = taskbarWidth - 8;
            int bottomLimit = taskbarHeight - 8;
            int leftLimit = 2;
            int topLimit = 2;

            IntPtr trayNotify = FindWindowEx(taskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);
            if (trayNotify != IntPtr.Zero)
            {
                NativeRect trayRect;
                if (GetWindowRect(trayNotify, out trayRect))
                {
                    Point trayTopLeft = ScreenToTaskbarPoint(trayRect.Left, trayRect.Top, layoutRect);
                    if (horizontal)
                    {
                        rightLimit = Math.Max(8, trayTopLeft.X - 8);
                    }
                    else
                    {
                        bottomLimit = Math.Max(8, trayTopLeft.Y - 8);
                    }
                }
            }

            int x;
            int y;
            if (horizontal)
            {
                if (taskbarAnchor == "right")
                {
                    x = rightLimit - width + taskbarOffsetX;
                }
                else
                {
                    x = leftLimit + taskbarOffsetX;
                }

                y = Math.Max(topLimit, (taskbarHeight - height) / 2 + taskbarOffsetY);
            }
            else
            {
                x = Math.Max(leftLimit, (taskbarWidth - width) / 2 + taskbarOffsetX);
                y = taskbarAnchor == "right"
                    ? bottomLimit - height + taskbarOffsetY
                    : topLimit + taskbarOffsetY;
            }

            NativeRect trafficRect;
            if (TryFindTrafficMonitorRect(layoutRect, out trafficRect))
            {
                NativeRect desired = new NativeRect();
                desired.Left = x;
                desired.Top = y;
                desired.Right = x + width;
                desired.Bottom = y + height;
                if (Intersects(desired, trafficRect))
                {
                    if (horizontal)
                    {
                        if (taskbarAnchor == "right")
                        {
                            x = trafficRect.Left - width - 8;
                        }
                        else
                        {
                            x = trafficRect.Right + 8;
                        }
                    }
                    else
                    {
                        if (taskbarAnchor == "right")
                        {
                            y = trafficRect.Top - height - 8;
                        }
                        else
                        {
                            y = trafficRect.Bottom + 8;
                        }
                    }
                }
            }

            x = Clamp(x, leftLimit, Math.Max(leftLimit, taskbarWidth - width - 2));
            y = Clamp(y, topLimit, Math.Max(topLimit, taskbarHeight - height - 2));

            int screenX;
            if (taskbarAnchor == "right")
            {
                screenX = workingArea.Right - width - 12 + taskbarOffsetX;
            }
            else
            {
                screenX = workingArea.Left + taskbarOffsetX;
            }

            int screenY = workingArea.Bottom - height - 12 + taskbarOffsetY;
            screenX = Clamp(screenX, workingArea.Left, Math.Max(workingArea.Left, workingArea.Right - width));
            screenY = Clamp(screenY, workingArea.Top, Math.Max(workingArea.Top, workingArea.Bottom - height));

            NativeRect nextRect = new NativeRect();
            nextRect.Left = screenX;
            nextRect.Top = screenY;
            nextRect.Right = nextRect.Left + width;
            nextRect.Bottom = nextRect.Top + height;
            if (hasLastWindowRect &&
                lastWindowRect.Left == nextRect.Left &&
                lastWindowRect.Top == nextRect.Top &&
                lastWindowRect.Right == nextRect.Right &&
                lastWindowRect.Bottom == nextRect.Bottom)
            {
                return;
            }

            Bounds = new Rectangle(nextRect.Left, nextRect.Top, width, height);
            Show();
            BringToFront();
            lastWindowRect = nextRect;
            hasLastWindowRect = true;
            }
            finally
            {
                isPositioning = false;
            }
        }

        public void EnsureShown()
        {
            if (IsDisposed)
            {
                return;
            }

            if (!Visible)
            {
                Show();
            }

            if (WindowState != FormWindowState.Normal)
            {
                WindowState = FormWindowState.Normal;
            }

            TopMost = true;
            LogState("EnsureShown");
        }

        private void PositionFallback()
        {
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int width = Width;
            int height = Height;
            int x = workingArea.Left + Math.Max(16, taskbarOffsetX);
            int y = workingArea.Bottom - height - 16;
            Bounds = new Rectangle(x, y, width, height);
            Show();
            BringToFront();
        }

        public void ReattachIfNeeded()
        {
            if (!IsDisposed)
            {
                AttachToTaskbar();
                PositionInTaskbar();
            }
        }

        private void ResizeToText()
        {
            if (taskbarFixedWidth > 0)
            {
                Width = taskbarFixedWidth;
                return;
            }

            Size preferred = TextRenderer.MeasureText(priceLabel.Text, priceLabel.Font);
            Width = Math.Max(260, Math.Max(taskbarMinWidth, Math.Min(taskbarMaxWidth, preferred.Width + 48)));
        }

        private bool TryFindTrafficMonitorRect(NativeRect originRect, out NativeRect rect)
        {
            rect = new NativeRect();
            IntPtr hwnd = FindChildByTitle(taskbarHandle, "TrafficMonitorTaskbarWindow");
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }

            NativeRect screenRect;
            if (!GetWindowRect(hwnd, out screenRect))
            {
                return false;
            }

            Point topLeft = ScreenToTaskbarPoint(screenRect.Left, screenRect.Top, originRect);
            Point bottomRight = ScreenToTaskbarPoint(screenRect.Right, screenRect.Bottom, originRect);

            rect.Left = topLeft.X;
            rect.Top = topLeft.Y;
            rect.Right = bottomRight.X;
            rect.Bottom = bottomRight.Y;
            return true;
        }

        private static Point ScreenToTaskbarPoint(int x, int y, NativeRect taskbarRect)
        {
            return new Point(x - taskbarRect.Left, y - taskbarRect.Top);
        }

        private static bool Intersects(NativeRect a, NativeRect b)
        {
            return a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static IntPtr FindChildByTitle(IntPtr parent, string title)
        {
            FindState state = new FindState();
            state.Title = title;
            EnumChildWindows(parent, delegate(IntPtr hwnd, IntPtr lParam)
            {
                string text = GetWindowTextValue(hwnd);
                if (String.Equals(text, state.Title, StringComparison.Ordinal))
                {
                    state.Handle = hwnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);
            return state.Handle;
        }

        private static string GetWindowTextValue(IntPtr hwnd)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            GetWindowText(hwnd, builder, builder.Capacity);
            return builder.ToString();
        }

        private void LogState(string action)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
                string line = DateTime.Now.ToString("HH:mm:ss.fff") +
                    " " + action +
                    " Visible=" + Visible.ToString() +
                    " IsHandleCreated=" + IsHandleCreated.ToString() +
                    " IsDisposed=" + IsDisposed.ToString() +
                    " WindowState=" + WindowState.ToString() +
                    " Bounds=" + Bounds.ToString() +
                    Environment.NewLine;
                File.AppendAllText(path, line);
            }
            catch
            {
            }
        }

        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private sealed class FindState
        {
            public string Title;
            public IntPtr Handle;
        }

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetParent(IntPtr childHandle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetClientRect(IntPtr hWnd, out NativeRect rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ScreenToClient(IntPtr hWnd, ref Point point);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumChildWindows(IntPtr parentHandle, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int maxCount);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    }
}
