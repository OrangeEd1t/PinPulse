using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoMonitor
{
    internal sealed class MonitorContext : ApplicationContext
    {
        private readonly string appDir;
        private readonly CryptoPriceService priceService;
        private readonly Timer refreshTimer;
        private readonly Timer positionTimer;
        private readonly NotifyIcon notifyIcon;
        private TaskbarPriceForm priceForm;
        private AppConfig config;
        private bool isRefreshing;
        private bool isDisposed;

        public MonitorContext(string appDir, AppConfig config)
        {
            this.appDir = appDir;
            this.config = config;
            priceService = new CryptoPriceService();
            priceForm = new TaskbarPriceForm();
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = LoadAppIcon();
            notifyIcon.Text = "CryptoMonitor";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += delegate { ShowSettings(); };
            SetMenus();

            refreshTimer = new Timer();
            refreshTimer.Tick += delegate { RefreshPrices(); };

            positionTimer = new Timer();
            positionTimer.Interval = 5000;
            positionTimer.Tick += delegate { priceForm.EnsureShown(); };

            Application.Idle += FirstApplicationIdle;
        }

        public Form MainWindow
        {
            get { return priceForm; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                isDisposed = true;
                refreshTimer.Dispose();
                positionTimer.Dispose();
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                priceForm.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SetMenus()
        {
            ContextMenuStrip menu = BuildMenu();
            priceForm.SetMenu(menu);
            notifyIcon.ContextMenuStrip = menu;
        }

        private ContextMenuStrip BuildMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem showHideItem = new ToolStripMenuItem(Localization.Text(config, "MenuShowHide"), null, delegate { ToggleTaskbarWindow(); });
            ToolStripMenuItem refreshItem = new ToolStripMenuItem(Localization.Text(config, "MenuRefreshNow"), null, delegate { RefreshPrices(); });
            ToolStripMenuItem settingsItem = new ToolStripMenuItem(Localization.Text(config, "MenuSettings"), null, delegate { ShowSettings(); });
            ToolStripMenuItem configItem = new ToolStripMenuItem(Localization.Text(config, "MenuOpenConfigFolder"), null, delegate { OpenConfigFolder(); });
            ToolStripMenuItem exitItem = new ToolStripMenuItem(Localization.Text(config, "MenuExit"), null, delegate { Exit(); });

            menu.Items.Add(showHideItem);
            menu.Items.Add(refreshItem);
            menu.Items.Add(settingsItem);
            menu.Items.Add(configItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);
            return menu;
        }

        private void FirstApplicationIdle(object sender, EventArgs e)
        {
            Application.Idle -= FirstApplicationIdle;
            ApplyConfig();
            RefreshPrices();
        }

        private void ApplyConfig()
        {
            refreshTimer.Stop();
            refreshTimer.Interval = Math.Max(AppConfig.MinRefreshSeconds, config.GetPollIntervalSeconds()) * 1000;
            refreshTimer.Start();

            priceForm.ApplyConfig(config);
            config.ShowTaskbarWindow = true;
            if (!priceForm.Visible)
            {
                priceForm.Show();
            }
        }

        private void RefreshPrices()
        {
            if (isDisposed || isRefreshing)
            {
                return;
            }

            isRefreshing = true;
            priceService.FetchAsync(config).ContinueWith(delegate(Task<string> task)
            {
                if (isDisposed)
                {
                    isRefreshing = false;
                    return;
                }

                string text = null;
                string error = null;
                if (task.IsFaulted)
                {
                    Exception ex = task.Exception == null ? null : task.Exception.GetBaseException();
                    error = ex == null ? Localization.Text(config, "UpdateFailed") : ex.Message;
                }
                else
                {
                    text = task.Result;
                }

                PostToUi(delegate
                {
                    if (error != null)
                    {
                        ShowError(error);
                    }
                    else
                    {
                        ShowText(text);
                    }
                });
            }, TaskScheduler.Default);
        }

        private void ShowText(string text)
        {
            if (isDisposed)
            {
                return;
            }

            isRefreshing = false;
            priceForm.SetText(text, false);
            notifyIcon.Text = TruncateNotifyText("CryptoMonitor - " + text);
        }

        private void ShowError(string message)
        {
            if (isDisposed)
            {
                return;
            }

            isRefreshing = false;
            priceForm.SetText(Localization.Text(config, "ApiError"), true);
            notifyIcon.Text = TruncateNotifyText("CryptoMonitor - " + message);
        }

        private void PostToUi(MethodInvoker action)
        {
            try
            {
                if (isDisposed || priceForm == null || priceForm.IsDisposed || !priceForm.IsHandleCreated)
                {
                    isRefreshing = false;
                    return;
                }

                priceForm.BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        if (!isDisposed)
                        {
                            action();
                        }
                    }
                    catch
                    {
                        isRefreshing = false;
                    }
                });
            }
            catch
            {
                isRefreshing = false;
            }
        }

        private void ShowSettings()
        {
            using (SettingsForm settings = new SettingsForm(config))
            {
                settings.Saved += SettingsSaved;
                settings.ShowDialog();
                settings.Saved -= SettingsSaved;
            }
        }

        private void SettingsSaved(object sender, EventArgs e)
        {
            config.Save(appDir);
            SetMenus();
            ApplyConfig();
            RefreshPrices();
        }

        private void ToggleTaskbarWindow()
        {
            if (priceForm.Visible)
            {
                priceForm.Hide();
                return;
            }

            priceForm.Show();
            priceForm.EnsureShown();
        }

        private void OpenConfigFolder()
        {
            Process.Start("explorer.exe", appDir);
        }

        private void Exit()
        {
            if (priceForm != null && !priceForm.IsDisposed)
            {
                priceForm.Close();
            }
            else
            {
                Application.Exit();
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
    }
}
