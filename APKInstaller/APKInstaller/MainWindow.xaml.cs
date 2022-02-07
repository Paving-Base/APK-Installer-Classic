using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using APKInstaller.Properties;
using MicaWPF.Controls;
using System;
using System.IO;

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
            if (Settings.Default.IsCloseADB)
            {
                new AdvancedAdbClient().KillAdb();
            }
            string TempPath = Path.Combine(Path.GetTempPath(), @$"APKInstaller\Caches\{Environment.ProcessId}");
            if (Directory.Exists(TempPath))
            {
                Directory.Delete(TempPath, true);
            }
        }
    }
}
