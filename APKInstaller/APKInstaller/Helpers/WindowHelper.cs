using ModernWpf;
using System.Collections.Generic;
using System.Windows;

namespace APKInstaller.Helpers
{
    // Helpers class to allow the app to find the Window that contains an
    // arbitrary UIElement (GetWindowForElement).  To do this, we keep track
    // of all active Windows.  The app code must call WindowHelper.CreateWindow
    // rather than "new Window" so we can keep track of all the relevant
    // windows.  In the future, we would like to support this in platform APIs.
    public static class WindowHelper
    {
        public static Window CreateWindow()
        {
            Window newWindow = new Window();
            TrackWindow(newWindow);
            return newWindow;
        }

        public static void TrackWindow(this Window window)
        {
            window.Closed += (sender, args) => {
                _activeWindows.Remove(window);
            };
            _activeWindows.Add(window);
        }

        public static Window GetWindowForElement(UIElement element)
        {
            var window = element.FindAscendant<Window>();
            return window;
            if (_activeWindows.Contains(window))
            {
                return window;
            }
            return null;
        }

        public static List<Window> ActiveWindows => _activeWindows;

        private static List<Window> _activeWindows = new List<Window>();
    }
}
