using APKInstaller.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace APKInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            SetCulture();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnStartup(StartupEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.TrackWindow();
            ThemeHelper.Initialize();
            MainWindow.Show();
            MainWindow.Activate();
        }

        private void SetCulture()
        {
            string lang = SettingsHelper.Get<string>(SettingsHelper.CurrentLanguage);
            if (!string.IsNullOrWhiteSpace(lang)) { SetCulture(new(lang)); }
        }

        public void SetCulture(CultureInfo culture)
        {
            if (culture.Name != LanguageHelper.GetCurrentLanguage())
            {
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
        }
    }
}
