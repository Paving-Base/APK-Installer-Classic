using APKInstaller.Pages.SettingsPages;
using APKInstaller.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace APKInstaller.Pages
{
    /// <summary>
    /// InstallPage.xaml 的交互逻辑
    /// </summary>
    public partial class InstallPage : Page
    {
        internal InstallViewModel? Provider;

        public InstallPage()
        {
            InitializeComponent();

            string _path = string.Empty;
            string[] arguments = Environment.GetCommandLineArgs();

            if (arguments.GetLength(0) > 1)
            {
                if (arguments[1].EndsWith(".apk"))
                {
                    string filePathFormMainArgs = arguments[1];
                    if (File.Exists(filePathFormMainArgs))
                    {
                        _path = filePathFormMainArgs;
                        Provider = new InstallViewModel(_path, this);
                    }
                }
                else if (arguments[1] == "-f" && arguments[2].EndsWith(".apk"))
                {
                    string filePathFormMainArgs = arguments[2];
                    if (File.Exists(filePathFormMainArgs))
                    {
                        _path = filePathFormMainArgs;
                        Provider = new InstallViewModel(_path, this);
                    }
                }
                else if (arguments[1].Contains(":?source=") || arguments[1].Contains("://"))
                {
                    Provider = new InstallViewModel(new Uri(arguments[1]), this);
                }
            }

            Provider ??= new InstallViewModel(_path, this);
            DataContext = Provider;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "ActionButton":
                    Provider?.InstallAPP();
                    break;
                case "DownloadButton":
                    Provider?.LoadNetAPK();
                    break;
                case "FileSelectButton":
                    Provider?.OpenAPK();
                    break;
                case "DeviceSelectButton":
                    NavigationService.Navigate(new SettingsPage());
                    break;
                case "SecondaryActionButton":
                    Provider?.OpenAPP();
                    break;
                case "CancelOperationButton":
                    Application.Current.Shutdown();
                    break;
            }
        }

        private async void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e) => await Provider?.Refresh();
    }
}
