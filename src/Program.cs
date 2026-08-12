using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CryptoMonitor
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            EnableDpiAwareness();

            bool created;
            using (Mutex mutex = new Mutex(true, "Local\\CryptoMonitorTaskbarWindowV2", out created))
            {
                if (!created)
                {
                    MessageBox.Show(
                        "CryptoMonitor is already running. Check the floating window or system tray icon.",
                        "CryptoMonitor",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                AppConfig config = AppConfig.Load(appDir);
                try
                {
                    StartupManager.SetEnabled(config.StartWithWindows);
                }
                catch
                {
                    // Startup sync should not prevent the monitor from opening.
                }

                using (TaskbarPriceForm form = new TaskbarPriceForm(appDir, config))
                {
                    Application.Run(form);
                }
            }
        }

        private static void EnableDpiAwareness()
        {
            try
            {
                if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                {
                    return;
                }
            }
            catch
            {
            }

            try
            {
                SetProcessDPIAware();
            }
            catch
            {
            }
        }

        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
