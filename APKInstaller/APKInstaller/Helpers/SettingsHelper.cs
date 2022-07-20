using AdvancedSharpAdbClient;
using APKInstaller.Properties;
using ModernWpf;
using SharpCompress.Common;
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
        public const string UpdateDate = "UpdateDate";
        public const string IsFirstRun = "IsFirstRun";
        public const string IsCloseADB = "IsCloseADB";
        public const string IsCloseAPP = "IsCloseAPP";
        public const string ShowDialogs = "ShowDialogs";
        public const string AutoGetNetAPK = "AutoGetNetAPK";
        public const string DefaultDevice = "DefaultDevice";
        public const string CurrentLanguage = "CurrentLanguage";
        public const string SelectedAppTheme = "SelectedAppTheme";

        public static Type Get<Type>(string key) => PackagedAppHelper.IsPackagedApp
                ? (Type)LocalSettings.Values[key]
                : (Type)Settings.Default.GetType().GetProperty(key).GetValue(Settings.Default);

        public static void Set(string key, object value)
        {
            if (PackagedAppHelper.IsPackagedApp)
            {
                LocalSettings.Values[key] = value;
            }
            else
            {
                Settings.Default.GetType().GetProperty(key).SetValue(Settings.Default, value);
                Settings.Default.Save();
            }
        }

        public static void SetDefaultSettings()
        {
            if (PackagedAppHelper.IsPackagedApp)
            {
                if (!LocalSettings.Values.ContainsKey(ADBPath))
                {
                    LocalSettings.Values.Add(ADBPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ADB\adb.exe"));
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
                if (!LocalSettings.Values.ContainsKey(IsCloseADB))
                {
                    LocalSettings.Values.Add(IsCloseADB, true);
                }
                if (!LocalSettings.Values.ContainsKey(IsCloseAPP))
                {
                    LocalSettings.Values.Add(IsCloseAPP, true);
                }
                if (!LocalSettings.Values.ContainsKey(ShowDialogs))
                {
                    LocalSettings.Values.Add(ShowDialogs, true);
                }
                if (!LocalSettings.Values.ContainsKey(AutoGetNetAPK))
                {
                    LocalSettings.Values.Add(AutoGetNetAPK, false);
                }
                if (!LocalSettings.Values.ContainsKey(DefaultDevice))
                {
                    LocalSettings.Values.Add(DefaultDevice, JsonSerializer.Serialize(new DeviceData()));
                }
                if (!LocalSettings.Values.ContainsKey(CurrentLanguage))
                {
                    LocalSettings.Values.Add(CurrentLanguage, string.Empty);
                }
                if (!LocalSettings.Values.ContainsKey(SelectedAppTheme))
                {
                    LocalSettings.Values.Add(SelectedAppTheme, (int)ElementTheme.Default);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Settings.Default.UpdateDate))
                {
                    Settings.Default.UpdateDate = JsonSerializer.Serialize(new DateTime());
                    Settings.Default.Save();
                }
                if (string.IsNullOrWhiteSpace(Settings.Default.DefaultDevice))
                {
                    Settings.Default.DefaultDevice = JsonSerializer.Serialize(new DeviceData());
                    Settings.Default.Save();
                }
            }
        }
    }

    internal static partial class SettingsHelper
    {
        private static readonly ApplicationDataContainer LocalSettings;

        static SettingsHelper()
        {
            if (PackagedAppHelper.IsPackagedApp)
            {
                LocalSettings = ApplicationData.Current.LocalSettings;
            }
            SetDefaultSettings();
        }
    }
}