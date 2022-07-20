using AdvancedSharpAdbClient;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Models;
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
using TitleBar = APKInstaller.Controls.TitleBar;

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        internal SettingsViewModel Provider;

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (SettingsViewModel.Caches != null)
            {
                Provider = SettingsViewModel.Caches;
                if (AdbServer.Instance.GetStatus().IsRunning)
                {
                    Provider.DeviceList = new AdvancedAdbClient().GetDevices();
                }
            }
            else
            {
                Provider = new SettingsViewModel(this);
                if (Provider.UpdateDate == DateTime.MinValue) { Provider.CheckUpdate(); }
                if (AdbServer.Instance.GetStatus().IsRunning)
                {
                    ADBHelper.Monitor.DeviceChanged += Provider.OnDeviceChanged;
                    Provider.DeviceList = new AdvancedAdbClient().GetDevices();
                }
            }
            DataContext = Provider;
            //#if DEBUG
            GoToTestPage.Visibility = Visibility.Visible;
            //#endif
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (AdbServer.Instance.GetStatus().IsRunning) { ADBHelper.Monitor.DeviceChanged -= Provider.OnDeviceChanged; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Rate":
                    _ = Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9N3HJLJP8V15"));
                    break;
                case "Group":
                    _ = Launcher.LaunchUriAsync(new Uri("https://t.me/PavingBase"));
                    break;
                case "Reset":
                    if (PackagedAppHelper.IsPackagedApp)
                    {
                        ApplicationData.Current.LocalSettings.Values.Clear();
                    }
                    else
                    {
                        Settings.Default.Reset();
                    }
                    SettingsHelper.SetDefaultSettings();
                    if (FlyoutService.GetFlyout(Reset) is Flyout flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    _ = Frame.Navigate(typeof(SettingsPage));
                    Frame.GoBack();
                    break;
                case "ADBPath":
                    Provider?.ChangeADBPath();
                    break;
                case "Connect":
                    if (!string.IsNullOrEmpty(ConnectIP.Text))
                    {
                        new AdvancedAdbClient().Connect(ConnectIP.Text);
                        Provider?.OnDeviceChanged(null, null);
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

        private void TitleBar_BackRequested(TitleBar sender, object args)
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
                SettingsHelper.Set(SettingsHelper.DefaultDevice, JsonSerializer.Serialize(device));
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "ADBPath":
                    _ = await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Provider.ADBPath[..Provider.ADBPath.LastIndexOf(@"\")]));
                    break;
                case "LogFolder":
                    _ = await Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
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

        private void WebXAML_Loaded(object sender, RoutedEventArgs e) => (sender as WebXAML).ContentInfo = new GitInfo("Paving-Base", "APK-Installer-Classic", "screenshots", "Documents/Announcements", "Announcements.xml");
    }
}
