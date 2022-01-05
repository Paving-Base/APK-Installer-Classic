using APKInstaller.Helpers;
using MicaWPF;
using MicaWPF.Helpers;
using ModernWpf.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Page = ModernWpf.Controls.Page;

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// TestPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestPage : Page, INotifyPropertyChanged
    {
        internal bool IsExtendsTitleBar
        {
            get => UIHelper.HasTitleBar /*? UIHelper.MainWindow.ExtendsContentIntoTitleBar : UIHelper.GetAppWindowForCurrentWindow().TitleBar.ExtendsContentIntoTitleBar*/;
            set
            {
                if (UIHelper.HasTitleBar)
                {
                    //UIHelper.MainWindow.ExtendsContentIntoTitleBar = value;
                }
                else
                {
                    //UIHelper.GetAppWindowForCurrentWindow().TitleBar.ExtendsContentIntoTitleBar = value;
                }
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
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
                    //_ = Frame.Navigate(typeof(ProcessesPage));
                    break;
                case "Applications":
                    //_ = Frame.Navigate(typeof(ApplicationsPage));
                    break;
                default:
                    break;
            }
        }

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void OverlayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch((sender as ComboBox).SelectedItem as string)
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
                    UIHelper.MainWindow.SystemBackdropType = BackdropType.None;
                    UIHelper.MainWindow.EnableMica(WindowsTheme.Auto, true, BackdropType.None, 20);
                    break;
                case "Mica":
                    UIHelper.MainWindow.SystemBackdropType = BackdropType.Mica;
                    UIHelper.MainWindow.EnableMica(WindowsTheme.Auto, true, BackdropType.Mica, 20);
                    break;
                case "Tabbed":
                    UIHelper.MainWindow.SystemBackdropType = BackdropType.Tabbed;
                    UIHelper.MainWindow.EnableMica(WindowsTheme.Auto, true, BackdropType.Tabbed, 20);
                    break;
                case "Acrylic":
                    UIHelper.MainWindow.SystemBackdropType = BackdropType.Acrylic;
                    UIHelper.MainWindow.EnableMica(WindowsTheme.Auto, true, BackdropType.Acrylic, 20);
                    break;
            }
        }
    }
}
