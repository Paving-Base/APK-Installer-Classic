using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using APKInstaller.Properties;
using MicaWPF.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace APKInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MicaWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            UIHelper.MainWindow = this;
            MainPage MainPage = new();
            Content = MainPage;
        }

        private void MicaWindow_Closed(object sender, EventArgs e)
        {
            string TempPath = Path.Combine(Path.GetTempPath(), @"APKInstaller\Caches", $"{Environment.ProcessId}");
            if (Directory.Exists(TempPath))
            {
                try { Directory.Delete(TempPath, true); } catch { }
            }

            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length <= 1)
            {
                if (Directory.Exists(TempPath[..TempPath.LastIndexOf(@"\")]))
                {
                    try { Directory.Delete(TempPath[..TempPath.LastIndexOf(@"\")], true); } catch { }
                }

                if (PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB) : Settings.Default.IsCloseADB)
                {
                    try { new AdvancedAdbClient().KillAdb(); } catch { }
                }
            }
        }
    }
}
