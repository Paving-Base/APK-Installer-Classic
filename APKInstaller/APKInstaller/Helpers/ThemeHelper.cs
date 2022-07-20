using ModernWpf;
using System;
using System.Windows;

namespace APKInstaller.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        private static Window CurrentApplicationWindow;

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (ThemeManager.GetActualTheme(window) != ElementTheme.Default)
                    {
                        return ThemeManager.GetActualTheme(window);
                    }
                }

                return SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme) == ElementTheme.Default
                        && ThemeManager.Current.ActualApplicationTheme.ToString() == ThemeManager.GetActualTheme(window).ToString())
                    {
                        return ElementTheme.Default;
                    }
                    return ThemeManager.GetActualTheme(window);
                }

                return ElementTheme.Default;
            }
            set
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    ThemeManager.SetRequestedTheme(window, value);
                }

                SettingsHelper.Set(SettingsHelper.SelectedAppTheme, (int)value);
            }
        }

        public static void Initialize()
        {
            // Save reference as this might be null when the user is in another app
            CurrentApplicationWindow = UIHelper.MainWindow;
            RootTheme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
        }

        public static bool IsDarkTheme()
        {
            return RootTheme == ElementTheme.Default
                ? ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark
                : RootTheme == ElementTheme.Dark;
        }
    }
}
