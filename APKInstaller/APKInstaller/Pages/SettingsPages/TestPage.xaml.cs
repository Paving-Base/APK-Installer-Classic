using APKInstaller.Helpers;
using APKInstaller.Pages.ToolPages;
using System;
using System.Collections.Generic;
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

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// TestPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestPage : Page
    {
        public TestPage() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "OutPIP":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanResizeWithGrip;
                    break;
                case "EnterPIP":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.NoResize;
                    break;
                case "Processes":
                    _ = NavigationService.Navigate(new ProcessesPage());
                    break;
                case "Applications":
                    _ = NavigationService.Navigate(new ApplicationsPage());
                    break;
                default:
                    break;
            }
        }

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void OverlayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ComboBox).SelectedItem as string)
            {
                case "No Resize":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.NoResize;
                    break;
                case "Can Resize":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanResize;
                    break;
                case "Can Minimize":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanMinimize;
                    break;
                case "Can Resize With Grip":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanResizeWithGrip;
                    break;
            }
        }
    }
}
