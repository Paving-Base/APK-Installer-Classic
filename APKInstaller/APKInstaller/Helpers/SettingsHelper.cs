using AdvancedSharpAdbClient;
using System;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace APKInstaller.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string ADBPath = "ADBPath";
        public const string IsOpenApp = "IsOpenApp";
        public const string IsOnlyWSA = "IsOnlyWSA";
        public const string IsDarkMode = "IsDarkMode";
        public const string UpdateDate = "UpdateDate";
        public const string IsFirstRun = "IsFirstRun";
        public const string IsCloseADB = "IsCloseADB";
        public const string AutoGetNetAPK = "AutoGetNetAPK";
        public const string DefaultDevice = "DefaultDevice";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";

        public static Type Get<Type>(string key) => (Type)LocalSettings.Values[key];
        public static void Set(string key, object value) => LocalSettings.Values[key] = value;

        public static void SetDefaultSettings()
        {
            if (!LocalSettings.Values.ContainsKey(ADBPath))
            {
                LocalSettings.Values.Add(ADBPath, Path.Combine(ApplicationData.Current.LocalFolder.Path, @"platform-tools\adb.exe"));
            }
            if (!LocalSettings.Values.ContainsKey(IsOpenApp))
            {
                LocalSettings.Values.Add(IsOpenApp, true);
            }
            if (!LocalSettings.Values.ContainsKey(IsOnlyWSA))
            {
                LocalSettings.Values.Add(IsOnlyWSA, OSVersionHelper.IsWindows11OrGreater);
            }
            if (!LocalSettings.Values.ContainsKey(UpdateDate))
            {
                LocalSettings.Values.Add(UpdateDate, JsonSerializer.Serialize(new DateTime()));
            }
            if (!LocalSettings.Values.ContainsKey(IsFirstRun))
            {
                LocalSettings.Values.Add(IsFirstRun, true);
            }
            if (!LocalSettings.Values.ContainsKey(IsDarkMode))
            {
                LocalSettings.Values.Add(IsDarkMode, false);
            }
            if (!LocalSettings.Values.ContainsKey(IsCloseADB))
            {
                LocalSettings.Values.Add(IsCloseADB, false);
            }
            if (!LocalSettings.Values.ContainsKey(AutoGetNetAPK))
            {
                LocalSettings.Values.Add(AutoGetNetAPK, false);
            }
            if (!LocalSettings.Values.ContainsKey(DefaultDevice))
            {
                LocalSettings.Values.Add(DefaultDevice, JsonSerializer.Serialize(new DeviceData()));
            }
            if (!LocalSettings.Values.ContainsKey(IsBackgroundColorFollowSystem))
            {
                LocalSettings.Values.Add(IsBackgroundColorFollowSystem, true);
            }
        }
    }

    internal static partial class SettingsHelper
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        static SettingsHelper()
        {
            if (PackagedAppHelper.IsPackagedApp)
            {
                SetDefaultSettings();
            }
        }
    }
}