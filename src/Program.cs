using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PinPulse
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            EnableDpiAwareness();

            bool created;
            using (Mutex mutex = new Mutex(true, "Local\\PinPulseTaskbarWindowV2", out created))
            {
                if (!created)
                {
                    MessageBox.Show(
                        "PinPulse is already running. Check the floating window or system tray icon.",
                        "PinPulse",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
                {
                    LogException(e.Exception, "UI thread exception");
                    MessageBox.Show(
                        "PinPulse recovered from an unexpected error. Details were written to crash.log.",
                        "PinPulse",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                };
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    LogException(e.ExceptionObject as Exception, "Unhandled exception");
                };

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

                using (MonitorForm form = new MonitorForm(appDir, config))
                {
                    Application.Run(form);
                }
            }
        }

        internal static void LogException(Exception ex, string context)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
                File.AppendAllText(
                    path,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + context + Environment.NewLine +
                    (ex == null ? "Unknown exception" : ex.ToString()) +
                    Environment.NewLine + Environment.NewLine);
            }
            catch
            {
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
