using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using APKInstaller.Properties;
using ModernWpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace APKInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
                CachesHelper.CleanAllCaches(true);

                if (SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB))
                {
                    try { new AdvancedAdbClient().KillAdb(); } catch { }
                }
            }
            else
            {
                CachesHelper.CleanAllCaches(false);
            }
        }
    }
}
