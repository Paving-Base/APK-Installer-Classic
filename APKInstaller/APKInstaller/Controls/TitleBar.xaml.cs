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
using ModernWpf.Controls;

namespace APKInstaller.Controls
{
    /// <summary>
    /// TitleBar.xaml 的交互逻辑
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public bool IsBackButtonEnabled { get => BackButton.IsEnabled; set => BackButton.IsEnabled = value; }

        public string Title
        {
            get => TitleBlock.Text;
            set => TitleBlock.Text = value ?? string.Empty;
        }

        public event RoutedEventHandler? RefreshEvent;
        public event RoutedEventHandler? BackRequested;

        public Visibility BackButtonVisibility { get => BackButton.Visibility; set => BackButton.Visibility = value; }
        public Visibility RefreshButtonVisibility { get => RefreshButton.Visibility; set => RefreshButton.Visibility = value; }
        public Visibility BackgroundVisibility { get => TitleBackground.Visibility; set => TitleBackground.Visibility = value; }

        public bool IsBackEnable { get => BackButton.IsEnabled; set => BackButton.IsEnabled = value; }
        public double TitleHeight { get => TitleGrid.Height; set => TitleGrid.Height = value; }
        public object RightAreaContent { get => UserContentPresenter.Content; set => UserContentPresenter.Content = value; }

        public TitleBar() => InitializeComponent();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshEvent?.Invoke(sender, e);

        private void BackButton_Click(object sender, RoutedEventArgs e) => BackRequested?.Invoke(sender, e);

        public void ShowProgressRing()
        {
            ProgressRing.IsActive = true;
            ProgressRing.Visibility = Visibility.Visible;
        }

        public void HideProgressRing()
        {
            ProgressRing.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
        }
    }
}
