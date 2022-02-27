using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using APKInstaller.ViewModel.ToolPages;
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

namespace APKInstaller.Pages.ToolPages
{
    /// <summary>
    /// ProcessesPage.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessesPage : Page
    {
        private ProcessesViewModel? Provider;

        public ProcessesPage()
        {
            InitializeComponent();
            Provider = new ProcessesViewModel(this);
            DataContext = Provider;
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e) => this.RunOnUIThread(() => Provider?.GetDevices());

        private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdvancedAdbClient client = new AdvancedAdbClient();
            Provider.Processes = DeviceExtensions.ListProcesses(client, Provider?.devices[(sender as ComboBox).SelectedIndex]);
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e)
        {
            TitleBar.ShowProgressRing();
            Provider?.GetDevices();
            TitleBar.ShowProgressRing();
            if (Provider?.devices == null) { return; }
            AdvancedAdbClient client = new AdvancedAdbClient();
            Provider.Processes = DeviceExtensions.ListProcesses(client, Provider?.devices[Provider.DeviceComboBox.SelectedIndex]);
            TitleBar.HideProgressRing();
        }

        private async void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.DeviceComboBox = sender as ComboBox;
            await Task.Run(() => this.RunOnUIThread(() => Provider?.GetDevices()));
        }
    }
}
