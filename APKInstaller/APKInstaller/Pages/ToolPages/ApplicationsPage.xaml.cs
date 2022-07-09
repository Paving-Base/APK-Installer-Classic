using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.ViewModel.ToolPages;
using ModernWpf;
using ModernWpf.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using ListView = ModernWpf.Controls.ListView;
using Page = ModernWpf.Controls.Page;
using TitleBar = APKInstaller.Controls.TitleBar;

namespace APKInstaller.Pages.ToolPages
{
    /// <summary>
    /// ApplicationsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ApplicationsPage : Page
    {
        private ApplicationsViewModel Provider;

        public ApplicationsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = new ApplicationsViewModel(this);
            DataContext = Provider;
            Provider.TitleBar = TitleBar;
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e) => this.RunOnUIThread(() => _ = Provider.GetDevices());

        private void TitleBar_BackRequested(TitleBar sender, object e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ = Provider.GetApps();
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e)
        {
            _ = Provider.GetDevices().ContinueWith((Task) => _ = Provider.GetApps());
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement Button = sender as FrameworkElement;
            switch (Button.Name)
            {
                case "More":
                    Setting Setting = Button.FindAscendant<Setting>();
                    Setting.ContextMenu.Placement = PlacementMode.Top;
                    Setting.ContextMenu.PlacementTarget = Button;
                    Setting.ContextMenu.IsOpen = (Button.Tag is bool IsOpen && IsOpen) ? false : true;
                    Button.Tag = Setting.ContextMenu.IsOpen;
                    break;
                case "Stop":
                    new AdvancedAdbClient().StopApp(Provider.devices[DeviceComboBox.SelectedIndex], Button.Tag.ToString());
                    break;
                case "Start":
                    new AdvancedAdbClient().StartApp(Provider.devices[DeviceComboBox.SelectedIndex], Button.Tag.ToString());
                    break;
                case "Uninstall":
                    break;
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.DeviceComboBox = sender as ComboBox;
            _ = Provider.GetDevices();
        }

        private void Setting_Loaded(object sender, RoutedEventArgs e)
        {
            Setting Setting = sender as Setting;
            ContentPresenter ContentPresenter = Setting.FindAscendant<ContentPresenter>();
            if (ContentPresenter != null)
            {
                ContentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }
    }
}
