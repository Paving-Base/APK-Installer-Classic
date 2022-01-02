using System.Windows;

namespace APKInstaller.Helpers.Converter
{
    public partial class IsEqualToVisibilityConverter : IsEqualToObjectConverter
    {
        public IsEqualToVisibilityConverter()
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }
    }
}
