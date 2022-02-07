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
using Windows.ApplicationModel;

namespace APKInstaller.ViewModel.SettingsPages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsPage _page;
        private readonly ResourceManager _loader = new ResourceManager(typeof(SettingsStrings));

        private IEnumerable<DeviceData> _deviceList;
        public IEnumerable<DeviceData> DeviceList
        {
            get => _deviceList;
            set
            {
                _deviceList = value;
                RaisePropertyChangedEvent();
                if (!IsOnlyWSA) { ChooseDevice(); }
            }
        }

        private bool _isOnlyWSA = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA) : Settings.Default.IsOnlyWSA;
        public bool IsOnlyWSA
        {
            get => _isOnlyWSA;
            set
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    SettingsHelper.Set(SettingsHelper.IsOnlyWSA, value);
                    _isOnlyWSA = SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
                }
                else
                {
                    Settings.Default.IsOnlyWSA = value;
                    Settings.Default.Save();
                    _isOnlyWSA = Settings.Default.IsOnlyWSA;
                }
                if (!value) { ChooseDevice(); }
                RaisePropertyChangedEvent();
            }
        }

        private bool _isCloseADB = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB) : Settings.Default.IsCloseADB;
        public bool IsCloseADB
        {
            get => _isCloseADB;
            set
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    SettingsHelper.Set(SettingsHelper.IsCloseADB, value);
                    _isCloseADB = SettingsHelper.Get<bool>(SettingsHelper.IsCloseADB);
                }
                else
                {
                    Settings.Default.IsCloseADB = value;
                    Settings.Default.Save();
                    _isCloseADB = Settings.Default.IsCloseADB;
                }
            }
        }

        private DateTime _updateDate = PackagedAppHelper.IsPackagedApp ? JsonSerializer.Deserialize<DateTime>(SettingsHelper.Get<string>(SettingsHelper.UpdateDate)) : Settings.Default.UpdateDate;
        public DateTime UpdateDate
        {
            get => _updateDate;
            set
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    SettingsHelper.Set(SettingsHelper.UpdateDate, JsonSerializer.Serialize(value));
                    _updateDate = JsonSerializer.Deserialize<DateTime>(SettingsHelper.Get<string>(SettingsHelper.UpdateDate));
                }
                else
                {
                    Settings.Default.UpdateDate = value;
                    Settings.Default.Save();
                    _updateDate = Settings.Default.UpdateDate;
                }
                RaisePropertyChangedEvent();
            }
        }

        private bool _autoGetNetAPK = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK): Settings.Default.AutoGetNetAPK;
        public bool AutoGetNetAPK
        {
            get => _autoGetNetAPK;
            set
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    SettingsHelper.Set(SettingsHelper.AutoGetNetAPK, value);
                    _autoGetNetAPK = SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK);
                }
                else
                {
                    Settings.Default.AutoGetNetAPK = value;
                    Settings.Default.Save();
                    _autoGetNetAPK = Settings.Default.AutoGetNetAPK;
                }
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

        private string _gotoUpdateTag;
        public string GotoUpdateTag
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

        private string _updateStateMessage;
        public string UpdateStateMessage
        {
            get => _updateStateMessage;
            set
            {
                _updateStateMessage = value;
                RaisePropertyChangedEvent();
            }
        }

        private InfoBarSeverity _updateStateSeverity;
        public InfoBarSeverity UpdateStateSeverity
        {
            get => _updateStateSeverity;
            set
            {
                _updateStateSeverity = value;
                RaisePropertyChangedEvent();
            }
        }

        private string _updateStateTitle;
        public string UpdateStateTitle
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
                string ver = PackagedAppHelper.IsPackagedApp ? $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}" : $"{Assembly.GetEntryAssembly()?.GetName().Version?.Major}.{Assembly.GetEntryAssembly()?.GetName().Version?.Minor}.{Assembly.GetEntryAssembly()?.GetName().Version?.Build}";
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
            string DefaultDevice = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<string>(SettingsHelper.DefaultDevice) : Settings.Default.DefaultDevice;
            DeviceData? device = string.IsNullOrEmpty(DefaultDevice) ? null : JsonSerializer.Deserialize<DeviceData>(DefaultDevice);
            if (device == null) { return; }
            foreach (DeviceData data in DeviceList)
            {
                if (data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                {
                    _page.SelectDeviceBox.SelectedItem = data;
                    break;
                }
            }
        }
    }
}
