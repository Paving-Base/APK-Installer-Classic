using APKInstaller.Strings.CapabilitiesInfoControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace APKInstaller.Controls
{
    /// <summary>
    /// CapabilitiesInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class CapabilitiesInfoControl : UserControl
    {
        public CapabilitiesInfoControl() => InitializeComponent();

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
           "Header",
           typeof(string),
           typeof(CapabilitiesInfoControl),
           new PropertyMetadata(default(string), OnHeaderChanged));

        public static readonly DependencyProperty CapabilitiesListProperty = DependencyProperty.Register(
           "CapabilitiesList",
           typeof(List<string>),
           typeof(CapabilitiesInfoControl),
           new PropertyMetadata(default(List<string>), OnCapabilitiesListChanged));

        [Localizable(true)]
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public List<string> CapabilitiesList
        {
            get => (List<string>)GetValue(CapabilitiesListProperty);
            set => SetValue(CapabilitiesListProperty, value);
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CapabilitiesInfoControl)d).UpdateHead();
        }

        private static void OnCapabilitiesListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CapabilitiesInfoControl)d).GetTextBlock();
        }

        private void UpdateHead() => HeaderTextBlock.Text = Header;

        private void GetTextBlock()
        {
            if (CapabilitiesList == null || !CapabilitiesList.Any()) { return; }
            int Line = 0;
            foreach (string capability in CapabilitiesList)
            {
                if (!string.IsNullOrEmpty(capability))
                {
                    RichTextBlockFullCapabilities.Text += $"• {capability}\n";
                }
                if (Line < 3 && !string.IsNullOrEmpty(capability))
                {
                    RichTextBlockCapabilities.Text += $"• {capability}\n";
                    Line++;
                }
            }
            RichTextBlockCapabilities.Text = RichTextBlockCapabilities.Text[0..^2];
            RichTextBlockFullCapabilities.Text = RichTextBlockFullCapabilities.Text[0..^2];
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (RichTextBlockCapabilities.Visibility == Visibility.Visible)
            {
                Root.BorderThickness = new Thickness(1, 0, 0, 0);
                MoreButton.Content = CapabilitiesInfoStrings.Less;
                RichTextBlockCapabilities.Visibility = Visibility.Collapsed;
                RichTextBlockFullCapabilities.Visibility = Visibility.Visible;
                CapabilitiesHeight.Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                Root.BorderThickness = new Thickness(0, 0, 0, 0);
                MoreButton.Content = CapabilitiesInfoStrings.More;
                RichTextBlockCapabilities.Visibility = Visibility.Visible;
                RichTextBlockFullCapabilities.Visibility = Visibility.Collapsed;
                CapabilitiesHeight.Height = new GridLength(1, GridUnitType.Auto);
            }
        }
    }
}
