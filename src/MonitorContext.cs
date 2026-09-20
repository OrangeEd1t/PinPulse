using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PinPulse
{
    internal sealed class MonitorContext : ApplicationContext
    {
        private readonly string appDir;
        private readonly MonitorService monitorService;
        private readonly Timer refreshTimer;
        private readonly Timer positionTimer;
        private readonly NotifyIcon notifyIcon;
        private MonitorForm monitorForm;
        private AppConfig config;
        private bool isRefreshing;
        private bool isDisposed;

        public MonitorContext(string appDir, AppConfig config)
        {
            this.appDir = appDir;
            this.config = config;
            monitorService = new MonitorService();
            monitorForm = new MonitorForm();
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = LoadAppIcon();
            notifyIcon.Text = "PinPulse";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += delegate { ShowSettings(); };
            SetMenus();

            refreshTimer = new Timer();
            refreshTimer.Tick += delegate { RefreshData(); };

            positionTimer = new Timer();
            positionTimer.Interval = 5000;
            positionTimer.Tick += delegate { monitorForm.EnsureShown(); };

            Application.Idle += FirstApplicationIdle;
        }

        public Form MainWindow
        {
            get { return monitorForm; }
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
                monitorForm.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SetMenus()
        {
            ContextMenuStrip menu = BuildMenu();
            monitorForm.SetMenu(menu);
            notifyIcon.ContextMenuStrip = menu;
        }

        private ContextMenuStrip BuildMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem showHideItem = new ToolStripMenuItem(Localization.Text(config, "MenuShowHide"), null, delegate { ToggleTaskbarWindow(); });
            ToolStripMenuItem refreshItem = new ToolStripMenuItem(Localization.Text(config, "MenuRefreshNow"), null, delegate { RefreshData(); });
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
            RefreshData();
        }

        private void ApplyConfig()
        {
            refreshTimer.Stop();
            refreshTimer.Interval = Math.Max(AppConfig.MinRefreshSeconds, config.GetPollIntervalSeconds()) * 1000;
            refreshTimer.Start();

            monitorForm.ApplyConfig(config);
            config.ShowTaskbarWindow = true;
            if (!monitorForm.Visible)
            {
                monitorForm.Show();
            }
        }

        private void RefreshData()
        {
            if (isDisposed || isRefreshing)
            {
                return;
            }

            isRefreshing = true;
            monitorService.FetchAsync(config).ContinueWith(delegate(Task<string> task)
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
            monitorForm.SetText(text, false);
            notifyIcon.Text = TruncateNotifyText("PinPulse - " + text);
        }

        private void ShowError(string message)
        {
            if (isDisposed)
            {
                return;
            }

            isRefreshing = false;
            monitorForm.SetText(Localization.Text(config, "ApiError"), true);
            notifyIcon.Text = TruncateNotifyText("PinPulse - " + message);
        }

        private void PostToUi(MethodInvoker action)
        {
            try
            {
                if (isDisposed || monitorForm == null || monitorForm.IsDisposed || !monitorForm.IsHandleCreated)
                {
                    isRefreshing = false;
                    return;
                }

                monitorForm.BeginInvoke((MethodInvoker)delegate
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
            RefreshData();
        }

        private void ToggleTaskbarWindow()
        {
            if (monitorForm.Visible)
            {
                monitorForm.Hide();
                return;
            }

            monitorForm.Show();
            monitorForm.EnsureShown();
        }

        private void OpenConfigFolder()
        {
            Process.Start("explorer.exe", appDir);
        }

        private void Exit()
        {
            if (monitorForm != null && !monitorForm.IsDisposed)
            {
                monitorForm.Close();
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
