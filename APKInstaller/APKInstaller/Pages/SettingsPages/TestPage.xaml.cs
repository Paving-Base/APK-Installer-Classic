using APKInstaller.Helpers;
using APKInstaller.Pages.ToolPages;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Page = ModernWpf.Controls.Page;
using TitleBar = APKInstaller.Controls.TitleBar;

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// TestPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

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
                    _ = Frame.Navigate(typeof(ProcessesPage));
                    break;
                case "Applications":
                    _ = Frame.Navigate(typeof(ApplicationsPage));
                    break;
                default:
                    break;
            }
        }

        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
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

        private void BackdorpComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ComboBox).SelectedItem as string)
            {
                case "None":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.None);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, false);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, false);
                    break;
                case "Mica":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.Mica);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, false);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, false);
                    break;
                case "Tabbed":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.Tabbed);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, false);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, false);
                    break;
                case "Acrylic":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.Acrylic);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, true);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, true);
                    break;
            }
        }
    }
}
