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
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Count() <= 1)
            {
                if (PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB) : Settings.Default.IsCloseADB)
                {
                    new AdvancedAdbClient().KillAdb();
                }
            }
            string TempPath = Path.Combine(Path.GetTempPath(), @$"APKInstaller\Caches\{Environment.ProcessId}");
            if (Directory.Exists(TempPath))
            {
                try { Directory.Delete(TempPath, true); } catch { }
            }
        }
    }
}
