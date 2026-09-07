using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoMonitor
{
    internal sealed class TaskbarPriceForm : Form
    {
        private readonly string appDir;
        private readonly CryptoPriceService priceService;
        private readonly Timer refreshTimer;
        private readonly Timer visibilityTimer;
        private readonly Timer positionSaveTimer;
        private readonly NotifyIcon notifyIcon;
        private ContextMenuStrip activeMenu;
        private AppConfig config;
        private bool isRefreshing;
        private bool isSettingsOpen;
        private bool isMenuOpen;
        private bool isClosing;
        private bool dragging;
        private bool initialPositionApplied;
        private Point dragOffset;
        private string displayText = "BTC --   ETH --";
        private Color displayColor = Color.Black;
        private Color windowBackgroundColor = Color.White;
        private bool windowBackgroundTransparent = true;
        private bool windowTextWrap;
        private int fixedWidth;
        private int minWidth = 260;
        private int maxWidth = 520;

        public TaskbarPriceForm()
            : this(null, null)
        {
        }

        public TaskbarPriceForm(string appDir, AppConfig config)
        {
            this.appDir = appDir;
            this.config = config;
            priceService = appDir == null ? null : new CryptoPriceService();

            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Color.Black;
            ForeColor = Color.Black;
            Font = CreateDisplayFont(config);
            Height = 34;
            Width = 420;
            Padding = new Padding(10, 4, 10, 4);
            Text = "CryptoMonitor";

            MouseDown += DragMouseDown;
            MouseMove += DragMouseMove;
            MouseUp += DragMouseUp;

            refreshTimer = new Timer();
            refreshTimer.Tick += delegate { RefreshPrices(); };

            visibilityTimer = new Timer();
            visibilityTimer.Interval = 2000;
            visibilityTimer.Tick += delegate { EnsureShown(); };
            visibilityTimer.Start();

            positionSaveTimer = new Timer();
            positionSaveTimer.Interval = 500;
            positionSaveTimer.Tick += delegate
            {
                positionSaveTimer.Stop();
                SaveWindowPosition();
            };

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = LoadAppIcon();
            notifyIcon.Text = "CryptoMonitor";
            notifyIcon.Visible = appDir != null;
            notifyIcon.DoubleClick += delegate { ShowSettings(); };

            if (config != null)
            {
                ApplyConfig(config);
                SetMenu(BuildMenu());
                Shown += delegate { RefreshPrices(); };
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                cp.ExStyle &= ~WS_EX_APPWINDOW;
                return cp;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            EnsureTopMost();
            RenderLayeredWindow();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isClosing = true;
            base.OnFormClosing(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (IsHandleCreated && Visible)
            {
                RenderLayeredWindow();
                ScheduleWindowPositionSave();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            isClosing = true;
            refreshTimer.Stop();
            visibilityTimer.Stop();
            positionSaveTimer.Stop();
            refreshTimer.Dispose();
            visibilityTimer.Dispose();
            positionSaveTimer.Dispose();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            base.OnFormClosed(e);
        }

        public void SetText(string text, bool isError)
        {
            if (IsDisposed || isClosing)
            {
                return;
            }

            displayText = String.IsNullOrEmpty(text) ? "" : text;
            displayColor = isError ? Color.FromArgb(190, 30, 30) : Color.Black;
            if (isSettingsOpen)
            {
                return;
            }

            ResizeToText();
            EnsureShown();
            RenderLayeredWindow();
        }

        public void ApplyConfig(AppConfig config)
        {
            if (config == null)
            {
                return;
            }

            this.config = config;
            fixedWidth = Clamp(config.TaskbarFixedWidth, 0, 4000);
            minWidth = Clamp(config.TaskbarMinWidth, 260, 4000);
            maxWidth = Clamp(Math.Max(config.TaskbarMaxWidth, minWidth), 260, 4000);
            Font = CreateDisplayFont(config);
            windowBackgroundColor = ParseColor(config.WindowBackgroundColor, Color.White);
            windowBackgroundTransparent = config.WindowBackgroundTransparent;
            windowTextWrap = config.WindowTextWrap;
            ResizeToText();
            ApplyInitialWindowPosition();

            refreshTimer.Stop();
            refreshTimer.Interval = Math.Max(AppConfig.MinRefreshSeconds, config.GetPollIntervalSeconds()) * 1000;
            refreshTimer.Start();
            EnsureShown();
        }

        public void SetMenu(ContextMenuStrip menu)
        {
            if (activeMenu != null)
            {
                activeMenu.Opened -= MenuOpened;
                activeMenu.Closed -= MenuClosed;
            }

            activeMenu = menu;
            if (activeMenu != null)
            {
                activeMenu.Opened += MenuOpened;
                activeMenu.Closed += MenuClosed;
            }

            ContextMenuStrip = null;
            notifyIcon.ContextMenuStrip = menu;
        }

        public void EnsureShown()
        {
            if (IsDisposed || isClosing || isSettingsOpen || isMenuOpen)
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
            EnsureTopMost();
            RenderLayeredWindow();
        }

        private void MenuOpened(object sender, EventArgs e)
        {
            isMenuOpen = true;
            visibilityTimer.Stop();
        }

        private void MenuClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            isMenuOpen = false;
            if (!isSettingsOpen && !visibilityTimer.Enabled)
            {
                visibilityTimer.Start();
            }
        }

        public bool AttachToTaskbar()
        {
            return false;
        }

        public void PositionInTaskbar()
        {
            EnsureShown();
        }

        public void ReattachIfNeeded()
        {
            EnsureShown();
        }

        private ContextMenuStrip BuildMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripMenuItem(Localization.Text(config, "MenuShowHide"), null, delegate { ToggleWindow(); }));
            menu.Items.Add(new ToolStripMenuItem(Localization.Text(config, "MenuRefreshNow"), null, delegate { RefreshPrices(); }));
            menu.Items.Add(new ToolStripMenuItem(Localization.Text(config, "MenuSettings"), null, delegate { ShowSettings(); }));
            menu.Items.Add(new ToolStripMenuItem(Localization.Text(config, "MenuOpenConfigFolder"), null, delegate { OpenConfigFolder(); }));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem(Localization.Text(config, "MenuExit"), null, delegate { Close(); }));
            return menu;
        }

        private void RefreshPrices()
        {
            if (priceService == null || config == null || isRefreshing || IsDisposed || isClosing)
            {
                return;
            }

            isRefreshing = true;
            priceService.FetchAsync(config).ContinueWith(delegate(Task<string> task)
            {
                if (IsDisposed || isClosing || !IsHandleCreated)
                {
                    isRefreshing = false;
                    return;
                }

                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        try
                        {
                            if (IsDisposed || isClosing)
                            {
                                return;
                            }

                            if (task.IsFaulted)
                            {
                                Exception ex = task.Exception == null ? null : task.Exception.GetBaseException();
                                string message = ex == null ? Localization.Text(config, "UpdateFailed") : ex.Message;
                                ShowError(message);
                            }
                            else
                            {
                                ShowText(task.Result);
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.LogException(ex, "Refresh UI update failed");
                        }
                        finally
                        {
                            isRefreshing = false;
                        }
                    });
                }
                catch (Exception ex)
                {
                    isRefreshing = false;
                    Program.LogException(ex, "Refresh callback could not reach UI");
                }
            }, TaskScheduler.Default);
        }

        private void ShowText(string text)
        {
            SetText(text, false);
            notifyIcon.Text = TruncateNotifyText("CryptoMonitor - " + text);
        }

        private void ShowError(string message)
        {
            SetText(Localization.Text(config, "ApiError"), true);
            notifyIcon.Text = TruncateNotifyText("CryptoMonitor - " + message);
        }

        private void ShowSettings()
        {
            if (config == null || isSettingsOpen)
            {
                return;
            }

            isSettingsOpen = true;
            visibilityTimer.Stop();
            refreshTimer.Stop();
            TopMost = false;

            try
            {
                using (SettingsForm settings = new SettingsForm(config))
                {
                    settings.Saved += SettingsSaved;
                    settings.ShowDialog();
                    settings.Saved -= SettingsSaved;
                }
            }
            finally
            {
                isSettingsOpen = false;
                TopMost = true;
                visibilityTimer.Start();
                if (!refreshTimer.Enabled)
                {
                    refreshTimer.Start();
                }

                EnsureShown();
            }
        }

        private void SettingsSaved(object sender, EventArgs e)
        {
            config.Save(appDir);
            SetMenu(BuildMenu());
            ApplyConfig(config);
            RefreshPrices();
            RenderLayeredWindow();
        }

        private void EnsureTopMost()
        {
            if (IsDisposed || !IsHandleCreated || isSettingsOpen)
            {
                return;
            }

            TopMost = true;
            SetWindowPos(
                Handle,
                HWND_TOPMOST,
                0,
                0,
                0,
                0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        private void ToggleWindow()
        {
            if (Visible)
            {
                Hide();
                return;
            }

            Show();
            EnsureShown();
        }

        private void OpenConfigFolder()
        {
            if (!String.IsNullOrEmpty(appDir))
            {
                Process.Start("explorer.exe", appDir);
            }
        }

        private void DragMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            dragging = true;
            Capture = true;
            dragOffset = e.Location;
        }

        private void DragMouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging)
            {
                return;
            }

            Point screen = Cursor.Position;
            Location = new Point(screen.X - dragOffset.X, screen.Y - dragOffset.Y);
        }

        private void DragMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = false;
                Capture = false;
                SaveWindowPosition();
            }
        }

        private void ScheduleWindowPositionSave()
        {
            if (config == null || String.IsNullOrEmpty(appDir) || !initialPositionApplied || isSettingsOpen)
            {
                return;
            }

            positionSaveTimer.Stop();
            positionSaveTimer.Start();
        }

        private void ApplyInitialWindowPosition()
        {
            if (initialPositionApplied || IsHandleCreated || Visible)
            {
                return;
            }

            initialPositionApplied = true;
            RestoreWindowPosition();
        }

        private void RestoreWindowPosition()
        {
            if (config != null && config.HasSavedWindowPosition())
            {
                Point saved = new Point(config.WindowLeft, config.WindowTop);
                if (IsPointNearAnyScreen(saved))
                {
                    Location = saved;
                    return;
                }
            }

            CenterToScreen();
        }

        private void SaveWindowPosition()
        {
            if (config == null || String.IsNullOrEmpty(appDir))
            {
                return;
            }

            config.WindowLeft = Left;
            config.WindowTop = Top;
            try
            {
                config.Save(appDir);
            }
            catch
            {
                // The app should keep running even if config cannot be written.
            }
        }

        private bool IsPointNearAnyScreen(Point point)
        {
            Rectangle windowBounds = new Rectangle(point.X, point.Y, Math.Max(1, Width), Math.Max(1, Height));
            foreach (Screen screen in Screen.AllScreens)
            {
                Rectangle area = screen.WorkingArea;
                area.Inflate(80, 80);
                if (area.IntersectsWith(windowBounds))
                {
                    return true;
                }
            }

            return false;
        }

        private void ResizeToText()
        {
            int targetWidth;
            int targetHeight;
            using (Bitmap bitmap = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using (StringFormat singleLineFormat = CreateDisplayStringFormat(false))
                {
                    SizeF singleLineSize = graphics.MeasureString(displayText, Font, Int32.MaxValue, singleLineFormat);
                    int singleLineWidth = (int)Math.Ceiling(singleLineSize.Width);

                    targetWidth = fixedWidth > 0
                        ? fixedWidth
                        : Math.Max(minWidth, Math.Min(maxWidth, singleLineWidth + Padding.Horizontal + 8));

                    if (windowTextWrap)
                    {
                        int textAreaWidth = Math.Max(1, targetWidth - Padding.Horizontal);
                        using (StringFormat wrapFormat = CreateDisplayStringFormat(true))
                        {
                            SizeF wrappedSize = graphics.MeasureString(displayText, Font, new SizeF(textAreaWidth, 10000F), wrapFormat);
                            targetHeight = (int)Math.Ceiling(wrappedSize.Height);
                        }
                    }
                    else
                    {
                        targetHeight = (int)Math.Ceiling(singleLineSize.Height);
                    }
                }
            }

            Width = targetWidth;
            Height = Math.Max(28, targetHeight + Padding.Vertical + 4);
        }

        private void RenderLayeredWindow()
        {
            try
            {
                RenderLayeredWindowCore();
            }
            catch (Exception ex)
            {
                Program.LogException(ex, "Layered window render failed");
            }
        }

        private void RenderLayeredWindowCore()
        {
            if (IsDisposed || !IsHandleCreated || Width <= 0 || Height <= 0)
            {
                return;
            }

            const int renderScale = 3;
            using (Bitmap highResBitmap = new Bitmap(Width * renderScale, Height * renderScale, PixelFormat.Format32bppPArgb))
            using (Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb))
            {
                using (Graphics graphics = Graphics.FromImage(highResBitmap))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.CompositingMode = CompositingMode.SourceOver;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.ScaleTransform(renderScale, renderScale);

                    if (!windowBackgroundTransparent)
                    {
                        using (Brush background = new SolidBrush(Color.FromArgb(255, windowBackgroundColor)))
                        {
                            graphics.FillRectangle(background, 0, 0, Width, Height);
                        }
                    }

                    using (StringFormat format = CreateDisplayStringFormat(windowTextWrap))
                    using (Brush brush = new SolidBrush(displayColor))
                    {
                        RectangleF rect = new RectangleF(
                            Padding.Left,
                            Padding.Top,
                            Math.Max(1, Width - Padding.Horizontal),
                            Math.Max(1, Height - Padding.Vertical));
                        graphics.DrawString(displayText, Font, brush, rect, format);
                    }
                }

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.DrawImage(
                        highResBitmap,
                        new Rectangle(0, 0, Width, Height),
                        new Rectangle(0, 0, highResBitmap.Width, highResBitmap.Height),
                        GraphicsUnit.Pixel);
                }

                IntPtr screenDc = GetDC(IntPtr.Zero);
                if (screenDc == IntPtr.Zero)
                {
                    return;
                }

                IntPtr memoryDc = IntPtr.Zero;
                IntPtr bitmapHandle = IntPtr.Zero;
                IntPtr oldBitmap = IntPtr.Zero;
                try
                {
                    memoryDc = CreateCompatibleDC(screenDc);
                    if (memoryDc == IntPtr.Zero)
                    {
                        return;
                    }

                    bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
                    if (bitmapHandle == IntPtr.Zero)
                    {
                        return;
                    }

                    oldBitmap = SelectObject(memoryDc, bitmapHandle);
                    NativePoint topPos = new NativePoint(Left, Top);
                    NativeSize size = new NativeSize(Width, Height);
                    NativePoint source = new NativePoint(0, 0);
                    BlendFunction blend = new BlendFunction();
                    blend.BlendOp = AC_SRC_OVER;
                    blend.BlendFlags = 0;
                    blend.SourceConstantAlpha = 255;
                    blend.AlphaFormat = AC_SRC_ALPHA;
                    UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memoryDc, ref source, 0, ref blend, ULW_ALPHA);
                }
                finally
                {
                    if (memoryDc != IntPtr.Zero && oldBitmap != IntPtr.Zero)
                    {
                        SelectObject(memoryDc, oldBitmap);
                    }

                    if (bitmapHandle != IntPtr.Zero)
                    {
                        DeleteObject(bitmapHandle);
                    }

                    if (memoryDc != IntPtr.Zero)
                    {
                        DeleteDC(memoryDc);
                    }

                    ReleaseDC(IntPtr.Zero, screenDc);
                }
            }
        }

        private static string TruncateNotifyText(string text)
        {
            if (String.IsNullOrEmpty(text) || text.Length <= 63)
            {
                return text;
            }

            return text.Substring(0, 60) + "...";
        }

        private static StringFormat CreateDisplayStringFormat(bool wrap)
        {
            StringFormat format = new StringFormat(StringFormat.GenericTypographic);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.Trimming = wrap ? StringTrimming.None : StringTrimming.EllipsisCharacter;
            if (!wrap)
            {
                format.FormatFlags |= StringFormatFlags.NoWrap;
            }

            return format;
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

        private static Font CreateDisplayFont(AppConfig config)
        {
            string family = config == null || String.IsNullOrWhiteSpace(config.TaskbarFontFamily)
                ? "Microsoft YaHei UI"
                : config.TaskbarFontFamily.Trim();
            float size = config == null ? 10F : Clamp(config.TaskbarFontSize, 6, 36);
            FontStyle style = config != null && config.TaskbarFontBold ? FontStyle.Bold : FontStyle.Regular;

            try
            {
                return new Font(family, size, style, GraphicsUnit.Point);
            }
            catch
            {
                return new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            }
        }

        private static Color ParseColor(string value, Color fallback)
        {
            string normalized = AppConfig.NormalizeColorHex(value, "");
            if (normalized.Length == 0)
            {
                return fallback;
            }

            try
            {
                return ColorTranslator.FromHtml(normalized);
            }
            catch
            {
                return fallback;
            }
        }

        private static Icon LoadAppIcon()
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (icon != null)
                {
                    return icon;
                }
            }
            catch
            {
            }

            return SystemIcons.Application;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CONTEXTMENU)
            {
                return;
            }

            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_EXITSIZEMOVE)
            {
                positionSaveTimer.Stop();
                SaveWindowPosition();
                return;
            }

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
            {
                m.Result = new IntPtr(HTCAPTION);
            }
        }

        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WM_CONTEXTMENU = 0x007B;
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDBLCLK = 0x00A3;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int ULW_ALPHA = 0x00000002;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;

            public NativePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSize
        {
            public int Width;
            public int Height;

            public NativeSize(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hwnd,
            IntPtr hdcDst,
            ref NativePoint pptDst,
            ref NativeSize psize,
            IntPtr hdcSrc,
            ref NativePoint pptSrc,
            int crKey,
            ref BlendFunction pblend,
            int dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hwnd,
            IntPtr hwndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
