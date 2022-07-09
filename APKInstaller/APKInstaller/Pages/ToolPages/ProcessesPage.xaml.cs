using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using APKInstaller.ViewModel.ToolPages;
using ModernWpf;
using ModernWpf.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Page = ModernWpf.Controls.Page;
using TitleBar = APKInstaller.Controls.TitleBar;

namespace APKInstaller.Pages.ToolPages
{
    /// <summary>
    /// ProcessesPage.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessesPage : Page
    {
        private ProcessesViewModel Provider;

        public ProcessesPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = new ProcessesViewModel(this);
            DataContext = Provider;
            Provider.TitleBar = TitleBar;
            ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e) => this.RunOnUIThread(() => _ = Provider?.GetDevices());

        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ = Provider.GetProcess();
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object args)
        {
            _ = Provider.GetDevices().ContinueWith((Task) => _ = Provider.GetProcess());
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.DeviceComboBox = sender as ComboBox;
            _ = Provider.GetDevices();
        }
    }
}
