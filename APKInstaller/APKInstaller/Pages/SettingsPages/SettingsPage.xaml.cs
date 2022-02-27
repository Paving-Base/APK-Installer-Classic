using AdvancedSharpAdbClient;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Properties;
using APKInstaller.ViewModel.SettingsPages;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        internal SettingsViewModel? Provider;

        public SettingsPage()
        {
            InitializeComponent();
            Provider = new SettingsViewModel(this);
            DataContext = Provider;
            //#if DEBUG
            GoToTestPage.Visibility = Visibility.Visible;
            //#endif
            if (SettingsViewModel.UpdateDate == DateTime.MinValue) { Provider.CheckUpdate(); }
            ADBHelper.Monitor.DeviceChanged += Provider.OnDeviceChanged;
            Provider.DeviceList = new AdvancedAdbClient().GetDevices();
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
                    _ = NavigationService.Navigate(new TestPage());
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
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void SelectDeviceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object vs = (sender as ListView).SelectedItem;
            if (vs != null && vs is DeviceData device)
            {
                Settings.Default.DefaultDevice = JsonSerializer.Serialize(device);
                Settings.Default.Save();
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

        private void GotoUpdate_Click(object sender, RoutedEventArgs e) => Process.Start((sender as FrameworkElement).Tag.ToString());

        private void Expander_Loaded(object sender, RoutedEventArgs e)
        {
            Expander Expander = sender as Expander;
            ToggleButton ToggleButton = Expander.FindDescendant<ToggleButton>();
            if (ToggleButton != null)
            {
                ToggleButton.Margin = new Thickness(11, 0, 11, 0);
                Grid Grid = ToggleButton.FindDescendant<Grid>();
                if (Grid != null)
                {
                    ColumnDefinitionCollection? ColumnDefinitions = Grid.ColumnDefinitions;
                    if (ColumnDefinitions.Count >= 2)
                    {
                        Grid.ColumnDefinitions.RemoveAt(0);
                        Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    }
                    Path Path = Grid.FindDescendant<Path>();
                    if (Path != null)
                    {
                        Grid.SetColumn(Path, 1);
                        Path.Margin = new Thickness(4, 0, 4, 0);
                    }
                    Ellipse Ellipse = Grid.FindDescendant<Ellipse>();
                    if (Ellipse != null)
                    {
                        Grid.SetColumn(Ellipse, 1);
                        Ellipse.Margin = new Thickness(4, 0, 4, 0);
                    }
                    ContentPresenter ContentPresenter = Grid.FindDescendant<ContentPresenter>();
                    if (ContentPresenter != null)
                    {
                        Grid.SetColumn(ContentPresenter, 0);
                        ContentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    }
                }
            }
        }
    }
}
