using System;
using System.Threading;
using System.Windows.Forms;

namespace CryptoMonitor
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool created;
            using (Mutex mutex = new Mutex(true, "Local\\CryptoMonitorTaskbarWindowV2", out created))
            {
                if (!created)
                {
                    MessageBox.Show(
                        "CryptoMonitor is already running. Check the taskbar or system tray icon.",
                        "CryptoMonitor",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            AppConfig config = AppConfig.Load(appDir);
            using (MonitorContext context = new MonitorContext(appDir, config))
            {
                Application.Run(context.MainWindow);
            }
            }
        }
    }
}
