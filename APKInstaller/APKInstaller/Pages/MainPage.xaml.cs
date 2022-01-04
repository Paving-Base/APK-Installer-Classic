using APKInstaller.Helpers;
using APKInstaller.Pages.SettingsPages;
using ModernWpf.Controls;
using System.Windows;

namespace APKInstaller.Pages
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            UIHelper.MainPage = this;
            _ = CoreAppFrame.Navigate(typeof(InstallPage));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "AboutButton":
                    _ = CoreAppFrame.Navigate(typeof(SettingsPage));
                    break;
                default:
                    break;
            }
        }
    }
}
