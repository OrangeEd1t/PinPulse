using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CryptoMonitor
{
    internal static class StartupManager
    {
        private const string ShortcutName = "CryptoMonitor.lnk";

        public static void SetEnabled(bool enabled)
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (String.IsNullOrEmpty(startupFolder))
            {
                throw new InvalidOperationException("Windows startup folder is unavailable.");
            }

            string shortcutPath = Path.Combine(startupFolder, ShortcutName);
            if (!enabled)
            {
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                return;
            }

            CreateShortcut(shortcutPath, GetExecutablePath());
        }

        private static string GetExecutablePath()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string path = assembly == null ? null : assembly.Location;
            if (String.IsNullOrEmpty(path))
            {
                path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            }

            return path;
        }

        private static void CreateShortcut(string shortcutPath, string targetPath)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
            {
                throw new InvalidOperationException("Windows Script Host is unavailable.");
            }

            object shell = null;
            object shortcut = null;
            try
            {
                shell = Activator.CreateInstance(shellType);
                shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath) });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
            finally
            {
                if (shortcut != null && Marshal.IsComObject(shortcut))
                {
                    Marshal.FinalReleaseComObject(shortcut);
                }

                if (shell != null && Marshal.IsComObject(shell))
                {
                    Marshal.FinalReleaseComObject(shell);
                }
            }
        }
    }
}
