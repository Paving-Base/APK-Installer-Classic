using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using APKInstaller.ViewModel.ToolPages;
using ModernWpf.Controls;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Page = ModernWpf.Controls.Page;

namespace APKInstaller.Pages.ToolPages
{
    /// <summary>
    /// ProcessesPage.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessesPage : Page
    {
        private ProcessesViewModel? Provider;

        public ProcessesPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = new ProcessesViewModel(this);
            DataContext = Provider;
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e) => this.RunOnUIThread(() => Provider?.GetDevices());

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
            Provider.Processes = DeviceExtensions.ListProcesses(client, Provider.devices[(sender as ComboBox).SelectedIndex]);
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e)
        {
            TitleBar.ShowProgressRing();
            Provider?.GetDevices();
            TitleBar.ShowProgressRing();
            if (Provider.devices == null) { return; }
            AdvancedAdbClient client = new AdvancedAdbClient();
            Provider.Processes = DeviceExtensions.ListProcesses(client, Provider.devices[Provider.DeviceComboBox.SelectedIndex]);
            TitleBar.HideProgressRing();
        }

        private async void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.DeviceComboBox = sender as ComboBox;
            await Task.Run(() => this.RunOnUIThread(() => Provider.GetDevices()));
        }
    }
}
