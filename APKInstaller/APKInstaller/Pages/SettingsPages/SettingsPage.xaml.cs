using AdvancedSharpAdbClient;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Properties;
using APKInstaller.ViewModel.SettingsPages;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.Storage;
using Windows.System;
using ListView = ModernWpf.Controls.ListView;
using Page = ModernWpf.Controls.Page;

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        internal SettingsViewModel? Provider;

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = new SettingsViewModel(this);
            DataContext = Provider;
            //#if DEBUG
            GoToTestPage.Visibility = Visibility.Visible;
            //#endif
            if (SettingsViewModel.UpdateDate == DateTime.MinValue) { Provider.CheckUpdate(); }
            ADBHelper.Monitor.DeviceChanged += Provider.OnDeviceChanged;
            Provider.DeviceList = new AdvancedAdbClient().GetDevices();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= Provider.OnDeviceChanged;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Connect":
                    if (!string.IsNullOrEmpty(ConnectIP.Text))
                    {
                        new AdvancedAdbClient().Connect(ConnectIP.Text);
                        Provider.OnDeviceChanged(null, null);
                    }
                    break;
                case "TestPage":
                    _ = Frame.Navigate(typeof(TestPage));
                    break;
                case "CheckUpdate":
                    Provider?.CheckUpdate();
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

        private void SelectDeviceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object vs = (sender as ListView).SelectedItem;
            if (vs != null && vs is DeviceData device)
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    SettingsHelper.Set(SettingsHelper.DefaultDevice, JsonSerializer.Serialize(device));
                }
                else
                {
                    Settings.Default.DefaultDevice = JsonSerializer.Serialize(device);
                    Settings.Default.Save();
                }
            }
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

        private void GotoUpdate_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri((sender as FrameworkElement).Tag.ToString()));
    }
}
