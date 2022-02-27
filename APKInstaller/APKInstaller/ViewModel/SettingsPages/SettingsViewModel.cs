using AdvancedSharpAdbClient;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Models;
using APKInstaller.Pages.SettingsPages;
using APKInstaller.Properties;
using APKInstaller.Strings.SettingsPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Windows;

namespace APKInstaller.ViewModel.SettingsPages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsPage _page;
        private readonly ResourceManager _loader = new ResourceManager(typeof(SettingsStrings));

        private IEnumerable<DeviceData>? _deviceList;
        public IEnumerable<DeviceData>? DeviceList
        {
            get => _deviceList;
            set
            {
                _deviceList = value;
                RaisePropertyChangedEvent();
                if (!IsOnlyWSA) { ChooseDevice(); }
            }
        }

        public bool IsOnlyWSA
        {
            get => Settings.Default.IsOnlyWSA;
            set
            {
                Settings.Default.IsOnlyWSA = value;
                Settings.Default.Save();
                if (!value) { ChooseDevice(); }
            }
        }

        public static bool IsCloseADB
        {
            get => Settings.Default.IsCloseADB;
            set
            {
                Settings.Default.IsCloseADB = value;
                Settings.Default.Save();
            }
        }

        public static bool IsCloseAPP
        {
            get => Settings.Default.IsCloseAPP;
            set
            {
                Settings.Default.IsCloseAPP = value;
                Settings.Default.Save();
            }
        }

        public static bool ShowDialogs
        {
            get => Settings.Default.ShowDialogs;
            set
            {
                Settings.Default.ShowDialogs = value;
                Settings.Default.Save();
            }
        }

        public static DateTime UpdateDate
        {
            get => Settings.Default.UpdateDate;
            set
            {
                Settings.Default.UpdateDate = value;
                Settings.Default.Save();
            }
        }

        public static bool AutoGetNetAPK
        {
            get => Settings.Default.AutoGetNetAPK;
            set
            {
                Settings.Default.AutoGetNetAPK = value;
                Settings.Default.Save();
            }
        }

        private bool _checkingUpdate;
        public bool CheckingUpdate
        {
            get => _checkingUpdate;
            set
            {
                _checkingUpdate = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _gotoUpdateTag;
        public string? GotoUpdateTag
        {
            get => _gotoUpdateTag;
            set
            {
                _gotoUpdateTag = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _gotoUpdateVisibility;
        public Visibility GotoUpdateVisibility
        {
            get => _gotoUpdateVisibility;
            set
            {
                _gotoUpdateVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _updateStateIsOpen;
        public bool UpdateStateIsOpen
        {
            get => _updateStateIsOpen;
            set
            {
                _updateStateIsOpen = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _updateStateMessage;
        public string? UpdateStateMessage
        {
            get => _updateStateMessage;
            set
            {
                _updateStateMessage = value;
                RaisePropertyChangedEvent();
            }
        }

        private InfoBarSeverity? _updateStateSeverity;
        public InfoBarSeverity? UpdateStateSeverity
        {
            get => _updateStateSeverity;
            set
            {
                _updateStateSeverity = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _updateStateTitle;
        public string? UpdateStateTitle
        {
            get => _updateStateTitle;
            set
            {
                _updateStateTitle = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public static string VersionTextBlockText
        {
            get
            {
                string ver = $"{Assembly.GetEntryAssembly()?.GetName().Version?.Major}.{Assembly.GetEntryAssembly()?.GetName().Version?.Minor}.{Assembly.GetEntryAssembly()?.GetName().Version?.Build}";
                string name = Resources.AppName ?? "APK Installer";
                return $"{name} v{ver}";
            }
        }

        public SettingsViewModel(SettingsPage Page) => _page = Page;

        public void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            _page.RunOnUIThread(() =>
            {
                DeviceList = new AdvancedAdbClient().GetDevices();
            });
        }

        public async void CheckUpdate()
        {
            CheckingUpdate = true;
            UpdateInfo? info = null;
            try
            {
                info = await UpdateHelper.CheckUpdateAsync("Paving-Base", "APK-Installer-Classic");
            }
            catch (Exception ex)
            {
                UpdateStateIsOpen = true;
                UpdateStateMessage = ex.Message;
                UpdateStateSeverity = InfoBarSeverity.Error;
                GotoUpdateVisibility = Visibility.Collapsed;
                UpdateStateTitle = _loader.GetString("CheckFailed");
            }
            if (info != null)
            {
                if (info.IsExistNewVersion)
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateTag = info.ReleaseUrl;
                    GotoUpdateVisibility = Visibility.Visible;
                    UpdateStateSeverity = InfoBarSeverity.Warning;
                    UpdateStateTitle = _loader.GetString("FindUpdate");
                    UpdateStateMessage = $"{VersionTextBlockText} -> {info.TagName}";
                }
                else
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateVisibility = Visibility.Collapsed;
                    UpdateStateSeverity = InfoBarSeverity.Success;
                    UpdateStateTitle = _loader.GetString("UpToDate");
                }
            }
            UpdateDate = DateTime.Now;
            CheckingUpdate = false;
        }

        public void ChooseDevice()
        {
            string DefaultDevice = Settings.Default.DefaultDevice;
            DeviceData? device = string.IsNullOrEmpty(DefaultDevice) ? null : JsonSerializer.Deserialize<DeviceData>(DefaultDevice);
            if (device == null) { return; }
            foreach (DeviceData data in DeviceList)
            {
                if (data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                {
                    //_page.SelectDeviceBox.SelectedItem = data;
                    break;
                }
            }
        }
    }
}
