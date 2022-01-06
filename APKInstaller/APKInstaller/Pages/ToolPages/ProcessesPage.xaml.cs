using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
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
using Page = ModernWpf.Controls.Page;

namespace APKInstaller.Pages.ToolPages
{
    /// <summary>
    /// ProcessesPage.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessesPage : Page, INotifyPropertyChanged
    {
        private ComboBox DeviceComboBox;
        private List<DeviceData>? devices;

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

        private IEnumerable<AndroidProcess> processes;
        internal IEnumerable<AndroidProcess> Processes
        {
            get => processes;
            set
            {
                processes = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ProcessesPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = this;
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
        }

        private void GetDevices()
        {
            TitleBar.ShowProgressRing();
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
            else if (Processes != null)
            {
                Processes = null;
            }
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdvancedAdbClient client = new AdvancedAdbClient();
            Processes = DeviceExtensions.ListProcesses(client, devices[(sender as ComboBox).SelectedIndex]);
            DataGrid.ItemsSource = Processes;
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e)
        {
            TitleBar.ShowProgressRing();
            GetDevices();
            TitleBar.ShowProgressRing();
            if(devices == null) { return; }
            AdvancedAdbClient client = new AdvancedAdbClient();
            Processes = DeviceExtensions.ListProcesses(client, devices[DeviceComboBox.SelectedIndex]);
            DataGrid.ItemsSource = Processes;
            TitleBar.HideProgressRing();
        }

        private async void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            DeviceComboBox = sender as ComboBox;
            await Task.Run(() => this.RunOnUIThread(() => GetDevices()));
        }
    }

    internal class ProcesseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "Size": return ((double)(int)value).GetSizeString();
                case "Name": return ((string)value).Split('/').Last().Split(':').First().Split('@').First();
                case "State":
                    switch ((AndroidProcessState)value)
                    {
                        case AndroidProcessState.Unknown: return "Unknown";
                        case AndroidProcessState.D: return "Sleep(D)";
                        case AndroidProcessState.R: return "Running";
                        case AndroidProcessState.S: return "Sleep(S)";
                        case AndroidProcessState.T: return "Stopped";
                        case AndroidProcessState.W: return "Paging";
                        case AndroidProcessState.X: return "Dead";
                        case AndroidProcessState.Z: return "Defunct";
                        case AndroidProcessState.K: return "Wakekill";
                        case AndroidProcessState.P: return "Parked";
                        default: return value.ToString();
                    }
                default: return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
