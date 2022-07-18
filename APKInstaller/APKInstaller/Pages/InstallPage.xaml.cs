using APKInstaller.Helpers;
using APKInstaller.Pages.SettingsPages;
using APKInstaller.ViewModel;
using ModernWpf.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Page = ModernWpf.Controls.Page;

namespace APKInstaller.Pages
{
    /// <summary>
    /// InstallPage.xaml 的交互逻辑
    /// </summary>
    public partial class InstallPage : Page
    {
        private bool IsCaches;
        internal InstallViewModel Provider;

        public InstallPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (InstallViewModel.Caches != null)
            {
                IsCaches = true;
                Provider = InstallViewModel.Caches;
            }
            else
            {
                IsCaches = false;
                string _path = string.Empty;
                string[] arguments = Environment.GetCommandLineArgs();

                if (arguments.GetLength(0) > 1)
                {
                    if (arguments[1].EndsWith(".apk") || arguments[1].EndsWith(".apks") || arguments[1].EndsWith(".mapk") || arguments[1].EndsWith(".xapk"))
                    {
                        string filePathFormMainArgs = arguments[1];
                        if (File.Exists(filePathFormMainArgs))
                        {
                            _path = filePathFormMainArgs;
                            Provider = new InstallViewModel(_path, this);
                        }
                    }
                    else if (arguments[1].Contains(":?source=") || arguments[1].Contains("://"))
                    {
                        Provider = new InstallViewModel(new Uri(arguments[1]), this);
                    }
                }

                Provider ??= new InstallViewModel(_path, this);
            }
            DataContext = Provider;

            //ModernWpf.MessageBox.Show(string.Join('\n', arguments), "Arguments", MessageBoxButton.OK);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider?.Dispose();
        }

        private async void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e)
        {
            await Provider.Refresh(!IsCaches);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "ActionButton":
                    Provider?.InstallAPP();
                    break;
                case "DownloadButton":
                    Provider?.LoadNetAPK();
                    break;
                case "FileSelectButton":
                    Provider?.OpenAPK();
                    break;
                case "DeviceSelectButton":
                    _ = Frame.Navigate(typeof(SettingsPage));
                    break;
                case "SecondaryActionButton":
                    Provider?.OpenAPP();
                    break;
                case "CancelOperationButton":
                    Application.Current.Shutdown();
                    break;
            }
        }

        private void CopyFileItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem element = sender as MenuItem;
            ClipboardHelper.CopyFile(element.Tag.ToString());
        }

        private void CopyStringItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem element = sender as MenuItem;
            ClipboardHelper.CopyText(element.Tag.ToString());
        }

        private void CopyBitmapItem_Click(object sender, RoutedEventArgs e)
        {
            ClipboardHelper.CopyBitmap(((Image)AppIcon).Source as BitmapImage);
        }

        private void Page_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            Provider.OpenAPK(e.Data);
            e.Handled = true;
        }
    }
}
