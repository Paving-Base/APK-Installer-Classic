using AdvancedSharpAdbClient;
using APKInstaller.Helpers;
using APKInstaller.Properties;
using APKInstaller.Strings.SettingsPage;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.ApplicationModel;
using Windows.System;
using ListView = ModernWpf.Controls.ListView;

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : ModernWpf.Controls.Page, INotifyPropertyChanged
    {

        private readonly ResourceManager _loader = new ResourceManager(typeof(SettingsStrings));

        private IEnumerable<DeviceData> _deviceList;
        internal IEnumerable<DeviceData> DeviceList
        {
            get => _deviceList;
            set
            {
                _deviceList = value;
                RaisePropertyChangedEvent();
                if (!IsOnlyWSA) { ChooseDevice(); }
            }
        }

        private bool _isOnlyWSA = Settings.Default.IsOnlyWSA;
        internal bool IsOnlyWSA
        {
            get => _isOnlyWSA;
            set
            {
                Settings.Default.IsOnlyWSA = value;
                Settings.Default.Save();
                _isOnlyWSA = Settings.Default.IsOnlyWSA;
                RaisePropertyChangedEvent();
                if (!value) { ChooseDevice(); }
            }
        }

        private bool _isCloseADB = Settings.Default.IsCloseADB;
        internal bool IsCloseADB
        {
            get => _isCloseADB;
            set
            {
                Settings.Default.IsCloseADB = value;
                Settings.Default.Save();
                _isCloseADB = Settings.Default.IsCloseADB;
                RaisePropertyChangedEvent();
            }
        }

        private DateTime _updateDate = Settings.Default.UpdateDate;
        internal DateTime UpdateDate
        {
            get => _updateDate;
            set
            {
                Settings.Default.UpdateDate = value;
                Settings.Default.Save();
                _updateDate = Settings.Default.UpdateDate;
                RaisePropertyChangedEvent();
            }
        }

        private bool _checkingUpdate;
        internal bool CheckingUpdate
        {
            get => _checkingUpdate;
            set
            {
                _checkingUpdate = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        internal string IssuePath = "https://github.com/Paving-Base/APK-Installer/issues";

        internal static string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                string name = Properties.Resources.AppName ?? "APK Installer";
                return $"{name} v{ver}";
            }
        }

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //#if DEBUG
            GoToTestPage.Visibility = Visibility.Visible;
            //#endif
            //SelectDeviceBox.SelectionMode = IsOnlyWSA ? ListViewSelectionMode.None : ListViewSelectionMode.Single;
            if (UpdateDate == DateTime.MinValue) { CheckUpdate(); }
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            DeviceList = new AdvancedAdbClient().GetDevices();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            this.RunOnUIThread(() =>
            {
                DeviceList = new AdvancedAdbClient().GetDevices();
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Connect":
                    new AdvancedAdbClient().Connect(ConnectIP.Text);
                    break;
                case "TestPage":
                    //_ = Frame.Navigate(typeof(TestPage));
                    break;
                case "FeedBack":
                    _ = Launcher.LaunchUriAsync(new Uri(IssuePath));
                    break;
                case "LogFolder":
                    //_ = await Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "CheckUpdate":
                    CheckUpdate();
                    break;
                default:
                    break;
            }
        }

        private async void CheckUpdate()
        {
            //CheckingUpdate = true;
            //UpdateInfo info = null;
            //try
            //{
            //    info = await UpdateHelper.CheckUpdateAsync("Paving-Base", "APK-Installer");
            //}
            //catch (Exception ex)
            //{
            //    UpdateState.IsOpen = true;
            //    UpdateState.Message = ex.Message;
            //    UpdateState.Severity = InfoBarSeverity.Error;
            //    GotoUpdate.Visibility = Visibility.Collapsed;
            //    UpdateState.Title = _loader.GetString("CheckFailed");
            //}
            //if (info != null)
            //{
            //    if (info.IsExistNewVersion)
            //    {
            //        UpdateState.IsOpen = true;
            //        GotoUpdate.Tag = info.ReleaseUrl;
            //        GotoUpdate.Visibility = Visibility.Visible;
            //        UpdateState.Severity = InfoBarSeverity.Warning;
            //        UpdateState.Title = _loader.GetString("FindUpdate");
            //        UpdateState.Message = $"{VersionTextBlockText} -> {info.TagName}";
            //    }
            //    else
            //    {
            //        UpdateState.IsOpen = true;
            //        GotoUpdate.Visibility = Visibility.Collapsed;
            //        UpdateState.Severity = InfoBarSeverity.Success;
            //        UpdateState.Title = _loader.GetString("UpToDate");
            //    }
            //}
            //UpdateDate = DateTime.Now;
            //CheckingUpdate = false;
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
                //SettingsHelper.Set(SettingsHelper.DefaultDevice, device);
            }
        }

        private void ChooseDevice()
        {
            DeviceData device = JsonSerializer.Deserialize<DeviceData>(Settings.Default.DefaultDevice);
            if (device == null) { return; }
            foreach (DeviceData data in DeviceList)
            {
                if (data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                {
                    SelectDeviceBox.SelectedItem = data;
                    break;
                }
            }
        }
    }
}
