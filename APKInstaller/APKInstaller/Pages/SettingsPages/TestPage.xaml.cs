using APKInstaller.Helpers;
using APKInstaller.Pages.ToolPages;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using SharpCompress.Common;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Windows.System;
using Page = ModernWpf.Controls.Page;
using TitleBar = APKInstaller.Controls.TitleBar;
using WindowHelper = ModernWpf.Controls.Primitives.WindowHelper;

namespace APKInstaller.Pages.SettingsPages
{
    /// <summary>
    /// TestPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public TestPage() => InitializeComponent();

        private void ThemeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            //string a = XamlWriter.Save(Announcements);
            //UIElement b = (UIElement)XamlReader.Parse(a);
            ComboBox ComboBox = sender as ComboBox;
            ElementTheme Theme = ThemeHelper.RootTheme;
            ComboBox.SelectedIndex = 2 - (int)Theme;
        }

        private void LanguageComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            string lang = SettingsHelper.Get<string>(SettingsHelper.CurrentLanguage);
            lang = string.IsNullOrWhiteSpace(lang) || lang == LanguageHelper.AutoLanguageCode ? LanguageHelper.GetCurrentLanguage() : lang;
            CultureInfo culture = new(lang);
            ComboBox.SelectedItem = culture;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "OutPIP":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanResize;
                    break;
                case "EnterPIP":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.NoResize;
                    break;
                case "Processes":
                    _ = Frame.Navigate(typeof(ProcessesPage));
                    break;
                case "Applications":
                    _ = Frame.Navigate(typeof(ApplicationsPage));
                    break;
                case "WindowsColor":
                    _ = Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
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

        private void OverlayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ComboBox).SelectedItem as string)
            {
                case "No Resize":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.NoResize;
                    break;
                case "Can Resize":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanResize;
                    break;
                case "Can Minimize":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanMinimize;
                    break;
                case "Can Resize With Grip":
                    UIHelper.MainWindow.ResizeMode = ResizeMode.CanResizeWithGrip;
                    break;
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            ThemeHelper.RootTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), (2 - ComboBox.SelectedIndex).ToString());
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            CultureInfo culture = ComboBox.SelectedItem as CultureInfo;
            if (culture.Name != LanguageHelper.GetCurrentLanguage())
            {
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                SettingsHelper.Set(SettingsHelper.CurrentLanguage, culture.Name);
            }
            else
            {
                CultureInfo.DefaultThreadCurrentCulture = null;
                CultureInfo.DefaultThreadCurrentUICulture = null;
                SettingsHelper.Set(SettingsHelper.CurrentLanguage, string.Empty);
            }
        }

        private void BackdorpComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ComboBox).SelectedItem as string)
            {
                case "None":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.None);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, false);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, false);
                    break;
                case "Mica":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.Mica);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, false);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, false);
                    break;
                case "Tabbed":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.Tabbed);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, false);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, false);
                    break;
                case "Acrylic":
                    WindowHelper.SetSystemBackdropType(UIHelper.MainWindow, BackdropType.Acrylic);
                    WindowHelper.SetUseAcrylicBackdrop(UIHelper.MainWindow, true);
                    WindowHelper.SetUseAeroBackdrop(UIHelper.MainWindow, true);
                    break;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => TitleBar.SetProgressValue(e.NewValue);

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleSwitch).IsOn)
            {
                TitleBar.ShowProgressRing();
            }
            else
            {
                TitleBar.HideProgressRing();
            }
        }
    }
}
