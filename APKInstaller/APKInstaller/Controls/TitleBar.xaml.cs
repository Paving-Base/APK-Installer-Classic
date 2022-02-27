using APKInstaller.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace APKInstaller.Controls
{
    /// <summary>
    /// TitleBar.xaml 的交互逻辑
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
           "Title",
           typeof(string),
           typeof(TitleBar),
           new PropertyMetadata(default(string), null));

        public static readonly DependencyProperty TitleHeightProperty = DependencyProperty.Register(
           "TitleHeight",
           typeof(double),
           typeof(TitleBar),
           new PropertyMetadata(UIHelper.PageTitleHeight, null));

        public static readonly DependencyProperty IsBackEnableProperty = DependencyProperty.Register(
           "IsBackEnable",
           typeof(bool),
           typeof(TitleBar),
           new PropertyMetadata(true, null));

        public static readonly DependencyProperty RightAreaContentProperty = DependencyProperty.Register(
           "RightAreaContent",
           typeof(object),
           typeof(TitleBar),
           null);

        public static readonly DependencyProperty BackgroundVisibilityProperty = DependencyProperty.Register(
           "BackgroundVisibility",
           typeof(Visibility),
           typeof(TitleBar),
           new PropertyMetadata(Visibility.Collapsed, null));

        public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register(
           "BackButtonVisibility",
           typeof(Visibility),
           typeof(TitleBar),
           new PropertyMetadata(Visibility.Visible, null));

        public static readonly DependencyProperty RefreshButtonVisibilityProperty = DependencyProperty.Register(
           "RefreshButtonVisibility",
           typeof(Visibility),
           typeof(TitleBar),
           new PropertyMetadata(Visibility.Collapsed, null));

        [Localizable(true)]
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public double TitleHeight
        {
            get => (double)GetValue(TitleHeightProperty);
            set => SetValue(TitleHeightProperty, value);
        }

        public bool IsBackEnable
        {
            get => (bool)GetValue(IsBackEnableProperty);
            set => SetValue(IsBackEnableProperty, value);
        }

        public object RightAreaContent
        {
            get => GetValue(RightAreaContentProperty);
            set => SetValue(RightAreaContentProperty, value);
        }

        public Visibility BackgroundVisibility
        {
            get => (Visibility)GetValue(BackgroundVisibilityProperty);
            set => SetValue(BackgroundVisibilityProperty, value);
        }

        public Visibility BackButtonVisibility
        {
            get => (Visibility)GetValue(BackButtonVisibilityProperty);
            set => SetValue(BackButtonVisibilityProperty, value);
        }

        public Visibility RefreshButtonVisibility
        {
            get => (Visibility)GetValue(RefreshButtonVisibilityProperty);
            set => SetValue(RefreshButtonVisibilityProperty, value);
        }

        public event RoutedEventHandler? RefreshEvent;
        public event RoutedEventHandler? BackRequested;

        public TitleBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshEvent?.Invoke(sender, e);

        private void BackButton_Click(object sender, RoutedEventArgs e) => BackRequested?.Invoke(sender, e);

        public void ShowProgressRing() => ProgressRing.Visibility = Visibility.Visible;

        public void HideProgressRing() => ProgressRing.Visibility = Visibility.Collapsed;
    }
}
