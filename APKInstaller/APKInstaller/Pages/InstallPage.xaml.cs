using APKInstaller.ViewModel;
using ModernWpf.Controls;
using System.Windows;
using System.Windows.Navigation;

namespace APKInstaller.Pages
{
    /// <summary>
    /// InstallPage.xaml 的交互逻辑
    /// </summary>
    public partial class InstallPage : Page
    {
        internal InstallViewModel Provider;

        public InstallPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if !DEBUG
            string _path = string.Empty;
#else
            string _path = @"C:\Users\qq251\Downloads\Programs\Minecraft_1.17.40.06_sign.apk";
#endif
            Provider = new InstallViewModel(_path, this);
            DataContext = Provider;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void InitialLoadingUI_Loaded(object sender, RoutedEventArgs e)
        {
            await Provider.Refresh();
        }
    }
}
