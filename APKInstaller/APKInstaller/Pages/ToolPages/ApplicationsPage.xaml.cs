using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using ListView = ModernWpf.Controls.ListView;
using Page = ModernWpf.Controls.Page;

namespace APKInstaller.Pages.ToolPages
{
    /// <summary>
    /// ApplicationsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ApplicationsPage : Page, INotifyPropertyChanged
    {
        private ComboBox DeviceComboBox;
        private List<DeviceData> devices;

        private List<string> deviceList = new List<string>();
        internal List<string> DeviceList
        {
            get => deviceList;
            set
            {
                deviceList = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<APKInfo> applications;
        internal List<APKInfo> Applications
        {
            get => applications;
            set
            {
                applications = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ApplicationsPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
        }

        private void GetDevices()
        {
            devices = new AdvancedAdbClient().GetDevices();
            DeviceList.Clear();
            if (devices.Count > 0)
            {
                foreach (DeviceData device in devices)
                {
                    if (!string.IsNullOrEmpty(device.Name))
                    {
                        DeviceList.Add(device.Name);
                    }
                    else if (!string.IsNullOrEmpty(device.Model))
                    {
                        DeviceList.Add(device.Model);
                    }
                    else if (!string.IsNullOrEmpty(device.Product))
                    {
                        DeviceList.Add(device.Product);
                    }
                    else if (!string.IsNullOrEmpty(device.Serial))
                    {
                        DeviceList.Add(device.Serial);
                    }
                    else
                    {
                        DeviceList.Add("Device");
                    }
                }
                DeviceComboBox.ItemsSource = DeviceList;
                if (DeviceComboBox.SelectedIndex == -1)
                {
                    DeviceComboBox.SelectedIndex = 0;
                }
            }
            else if (Applications != null)
            {
                Applications.Clear();
            }
        }

        private List<APKInfo> CheckAPP(Dictionary<string, string> apps, int index)
        {
            List<APKInfo> Applications = new List<APKInfo>();
            AdvancedAdbClient client = new AdvancedAdbClient();
            PackageManager manager = new PackageManager(client, devices[index]);
            this.RunOnUIThread(() => TitleBar.ShowProgressRing());
            foreach (KeyValuePair<string, string> app in apps)
            {
                if (!string.IsNullOrEmpty(app.Key))
                {
                    ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                    client.ExecuteRemoteCommand($"pidof {app.Key}", devices[index], receiver);
                    bool isactive = !string.IsNullOrEmpty(receiver.ToString());
                    Applications.Add(new APKInfo()
                    {
                        Name = app.Key,
                        IsActive = isactive,
                        VersionInfo = manager.GetVersionInfo(app.Key),
                    });
                }
            }
            return Applications;
        }

        private async Task Refresh()
        {
            TitleBar.ShowProgressRing();
            GetDevices();
            int index = DeviceComboBox.SelectedIndex;
            PackageManager manager = new PackageManager(new AdvancedAdbClient(), devices[DeviceComboBox.SelectedIndex]);
            Applications = await Task.Run(() => { return CheckAPP(manager.Packages, index); });
            ListView.ItemsSource = Applications;
            TitleBar.HideProgressRing();
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e) => this.RunOnUIThread(() => GetDevices());

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TitleBar.ShowProgressRing();
            int index = DeviceComboBox.SelectedIndex;
            PackageManager manager = new PackageManager(new AdvancedAdbClient(), devices[DeviceComboBox.SelectedIndex]);
            Applications = await Task.Run(() => { return CheckAPP(manager.Packages, index); });
            TitleBar.HideProgressRing();
        }

        private async void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => await Refresh();

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement Button = sender as FrameworkElement;
            switch (Button.Name)
            {
                case "Stop":
                    new AdvancedAdbClient().StopApp(devices[DeviceComboBox.SelectedIndex], Button.Tag.ToString());
                    break;
                case "Start":
                    new AdvancedAdbClient().StartApp(devices[DeviceComboBox.SelectedIndex], Button.Tag.ToString());
                    break;
                case "Uninstall":
                    break;
            }
        }

        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button Button = (sender as FrameworkElement).FindDescendant<Button>();
            //MenuFlyout Flyout = (MenuFlyout)Button.Content;
            //Flyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private async void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            DeviceComboBox = sender as ComboBox;
            await Task.Run(() => this.RunOnUIThread(() => GetDevices()));
        }
    }

    internal class ApplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "State": return (bool)value ? "Running" : "Stop";
                default: return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    internal class APKInfo
    {
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public VersionInfo? VersionInfo { get; set; }
    }
}
