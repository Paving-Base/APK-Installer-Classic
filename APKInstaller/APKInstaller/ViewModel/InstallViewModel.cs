﻿using AAPTForNet;
using AAPTForNet.Models;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using APKInstaller.Pages.SettingsPages;
using APKInstaller.Properties;
using APKInstaller.Strings.InstallPage;
using Microsoft.Win32;
using Downloader;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace APKInstaller.ViewModel
{
    internal class InstallViewModel : INotifyPropertyChanged, IDisposable
    {
        private DeviceData? _device;
        private readonly InstallPage _page;

        private string APKTemp => Path.Combine(CachesHelper.TempPath, "NetAPKTemp.apk");
        private string ADBTemp => Path.Combine(CachesHelper.TempPath, "platform-tools.zip");

#if !DEBUG
        private Uri? _url;
        private string _path = string.Empty;
#else
        private Uri? _url = new Uri("apkinstaller:?source=https://dl.coolapk.com/down?pn=com.coolapk.market&id=NDU5OQ&h=46bb9d98&from=from-web");
        private string _path = @"C:\Users\qq251\Downloads\Programs\Skit_com,pavelrekun,skit,premium_2,4,1.apks";
#endif
        private bool NetAPKExist => _path != APKTemp || File.Exists(_path);

        private bool _disposedValue;
        private readonly ResourceManager _loader = new ResourceManager(typeof(InstallStrings));

        public string? InstallFormat => _loader.GetString("InstallFormat");
        public string? VersionFormat => _loader.GetString("VersionFormat");
        public string? PackageNameFormat => _loader.GetString("PackageNameFormat");

        private static bool IsOnlyWSA => Settings.Default.IsOnlyWSA;
        private static bool IsCloseAPP => Settings.Default.IsCloseAPP;
        private static bool ShowDialogs => Settings.Default.ShowDialogs;
        private static bool AutoGetNetAPK => Settings.Default.AutoGetNetAPK;

        private ApkInfo? _apkInfo = null;
        public ApkInfo? ApkInfo
        {
            get => _apkInfo;
            set
            {
                _apkInfo = value;
                RaisePropertyChangedEvent();
            }
        }

        public string ADBPath
        {
            get => Settings.Default.ADBPath;
            set
            {
                Settings.Default.ADBPath = value;
                Settings.Default.Save();
                RaisePropertyChangedEvent();
            }
        }

        public bool IsOpenApp
        {
            get => Settings.Default.IsOpenApp;
            set
            {
                Settings.Default.IsOpenApp = value;
                Settings.Default.Save();
            }
        }

        private bool _isInstalling;
        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                if (value)
                {
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                }
                else
                {
                    ProgressHelper.SetState(ProgressState.None, true);
                }
                RaisePropertyChangedEvent();
            }
        }

        private bool _isInitialized;
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                if (value)
                {
                    ProgressHelper.SetState(ProgressState.None, true);
                }
                else
                {
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                }
                RaisePropertyChangedEvent();
            }
        }

        private string? _appName;
        public string? AppName
        {
            get => _appName;
            set
            {
                _appName = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _appVersion;
        public string? AppVersion
        {
            get => _appVersion;
            set
            {
                _appVersion = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _packageName;
        public string? PackageName
        {
            get => _packageName;
            set
            {
                _packageName = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _textOutput;
        public string? TextOutput
        {
            get => _textOutput;
            set
            {
                _textOutput = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _infoMessage;
        public string? InfoMessage
        {
            get => _infoMessage;
            set
            {
                _infoMessage = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _progressText;
        public string? ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _actionButtonEnable;
        public bool ActionButtonEnable
        {
            get => _actionButtonEnable;
            set
            {
                _actionButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _secondaryActionButtonEnable;
        public bool SecondaryActionButtonEnable
        {
            get => _secondaryActionButtonEnable;
            set
            {
                _secondaryActionButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _fileSelectButtonEnable;
        public bool FileSelectButtonEnable
        {
            get => _fileSelectButtonEnable;
            set
            {
                _fileSelectButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _downloadButtonEnable;
        public bool DownloadButtonEnable
        {
            get => _downloadButtonEnable;
            set
            {
                _downloadButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _deviceSelectButtonEnable;
        public bool DeviceSelectButtonEnable
        {
            get => _deviceSelectButtonEnable;
            set
            {
                _deviceSelectButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _cancelOperationButtonEnable;
        public bool CancelOperationButtonEnable
        {
            get => _cancelOperationButtonEnable;
            set
            {
                _cancelOperationButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _waitProgressText;
        public string? WaitProgressText
        {
            get => _waitProgressText;
            set
            {
                _waitProgressText = value;
                RaisePropertyChangedEvent();
            }
        }

        private double _waitProgressValue = 0;
        public double WaitProgressValue
        {
            get => _waitProgressValue;
            set
            {
                _waitProgressValue = value;
                RaisePropertyChangedEvent();
            }
        }

        private double _appxInstallBarValue = 0;
        public double AppxInstallBarValue
        {
            get => _appxInstallBarValue;
            set
            {
                _appxInstallBarValue = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _waitProgressIndeterminate = true;
        public bool WaitProgressIndeterminate
        {
            get => _waitProgressIndeterminate;
            set
            {
                _waitProgressIndeterminate = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _appxInstallBarIndeterminate = true;
        public bool AppxInstallBarIndeterminate
        {
            get => _appxInstallBarIndeterminate;
            set
            {
                _appxInstallBarIndeterminate = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _actionButtonText;
        public string? ActionButtonText
        {
            get => _actionButtonText;
            set
            {
                _actionButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _secondaryActionButtonText;
        public string? SecondaryActionButtonText
        {
            get => _secondaryActionButtonText;
            set
            {
                _secondaryActionButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _fileSelectButtonText;
        public string? FileSelectButtonText
        {
            get => _fileSelectButtonText;
            set
            {
                _fileSelectButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _downloadButtonText;
        public string? DownloadButtonText
        {
            get => _downloadButtonText;
            set
            {
                _downloadButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _deviceSelectButtonText;
        public string? DeviceSelectButtonText
        {
            get => _deviceSelectButtonText;
            set
            {
                _deviceSelectButtonText = value;
                RaisePropertyChangedEvent();
            }
        }


        private string? _cancelOperationButtonText;
        public string? CancelOperationButtonText
        {
            get => _cancelOperationButtonText;
            set
            {
                _cancelOperationButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _textOutputVisibility = Visibility.Collapsed;
        public Visibility TextOutputVisibility
        {
            get => _textOutputVisibility;
            set
            {
                _textOutputVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _installOutputVisibility = Visibility.Collapsed;
        public Visibility InstallOutputVisibility
        {
            get => _installOutputVisibility;
            set
            {
                _installOutputVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _actionVisibility = Visibility.Collapsed;
        public Visibility ActionVisibility
        {
            get => _actionVisibility;
            set
            {
                _actionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _secondaryActionVisibility = Visibility.Collapsed;
        public Visibility SecondaryActionVisibility
        {
            get => _secondaryActionVisibility;
            set
            {
                _secondaryActionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _fileSelectVisibility = Visibility.Collapsed;
        public Visibility FileSelectVisibility
        {
            get => _fileSelectVisibility;
            set
            {
                _fileSelectVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _downloadVisibility = Visibility.Collapsed;
        public Visibility DownloadVisibility
        {
            get => _downloadVisibility;
            set
            {
                _downloadVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _deviceSelectVisibility = Visibility.Collapsed;
        public Visibility DeviceSelectVisibility
        {
            get => _deviceSelectVisibility;
            set
            {
                _deviceSelectVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _cancelOperationVisibility = Visibility.Collapsed;
        public Visibility CancelOperationVisibility
        {
            get => _cancelOperationVisibility;
            set
            {
                _cancelOperationVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _messagesToUserVisibility = Visibility.Collapsed;
        public Visibility MessagesToUserVisibility
        {
            get => _messagesToUserVisibility;
            set
            {
                _messagesToUserVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _launchWhenReadyVisibility = Visibility.Collapsed;
        public Visibility LaunchWhenReadyVisibility
        {
            get => _launchWhenReadyVisibility;
            set
            {
                _launchWhenReadyVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appVersionVisibility;
        public Visibility AppVersionVisibility
        {
            get => _appVersionVisibility;
            set
            {
                _appVersionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appPublisherVisibility;
        public Visibility AppPublisherVisibility
        {
            get => _appPublisherVisibility;
            set
            {
                _appPublisherVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appCapabilitiesVisibility;
        public Visibility AppCapabilitiesVisibility
        {
            get => _appCapabilitiesVisibility;
            set
            {
                _appCapabilitiesVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        public InstallViewModel(Uri Url, InstallPage Page)
        {
            _url = Url;
            _page = Page;
            _path = APKTemp;
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        public InstallViewModel(string Path, InstallPage Page)
        {
            _page = Page;
            _path = string.IsNullOrEmpty(Path) ? _path : Path;
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public async Task Refresh()
        {
            IsInitialized = false;
            try
            {
                await InitilizeADB();
                await InitilizeUI();
            }
            catch (Exception ex)
            {
                PackageError(ex.Message);
            }
        }

        public async Task CheckADB(bool force = false)
        {
        checkadb:
            if (!File.Exists(ADBPath))
            {
                ADBPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ADB\adb.exe");
            }
            if (!force && File.Exists(ADBPath))
            {
                WaitProgressText = _loader.GetString("ADBExist");
            }
            else
            {
                ProgressHelper.SetState(ProgressState.None, true);
                MessageBoxResult result = MessageBox.Show(_loader.GetString("ADBMissing"), _loader.GetString("AboutADB"), MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                if (result == MessageBoxResult.Yes)
                {
                downloadadb:
                    try
                    {
                        await DownloadADB();
                    }
                    catch (Exception ex)
                    {
                        ProgressHelper.SetState(ProgressState.None, true);
                        MessageBoxResult results = MessageBox.Show(ex.Message, _loader.GetString("DownloadFailed"), MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                        if (results == MessageBoxResult.OK)
                        {
                            goto downloadadb;
                        }
                        else
                        {
                            Application.Current.Shutdown();
                            return;
                        }
                    }
                }
                else if (result == MessageBoxResult.No)
                {
                    OpenFileDialog? FileOpen = new OpenFileDialog();
                    FileOpen.Filter = ".exe|*.exe";
                    FileOpen.Title = _loader.GetString("ChooseADB");
                    if (FileOpen.ShowDialog() == false)
                    {
                        return;
                    }

                    ADBPath = FileOpen.FileName;
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
        }

        public async Task DownloadADB()
        {
            if (!Directory.Exists(ADBTemp.Substring(0, ADBTemp.LastIndexOf(@"\"))))
            {
                _ = Directory.CreateDirectory(ADBTemp.Substring(0, ADBTemp.LastIndexOf(@"\")));
            }
            else if (Directory.Exists(ADBTemp))
            {
                Directory.Delete(ADBTemp, true);
            }
            using (DownloadService downloader = new DownloadService(DownloadHelper.Configuration))
            {
                bool IsCompleted = false;
                long ReceivedBytesSize = 0;
                Exception? exception = null;
                long TotalBytesToReceive = 0;
                double ProgressPercentage = 0;
                double BytesPerSecondSpeed = 0;
                void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
                {
                    exception = e.Error;
                    IsCompleted = true;
                }
                void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
                {
                    ReceivedBytesSize = e.ReceivedBytesSize;
                    ProgressPercentage = e.ProgressPercentage;
                    TotalBytesToReceive = e.TotalBytesToReceive;
                    BytesPerSecondSpeed = e.BytesPerSecondSpeed;
                }
                downloader.DownloadFileCompleted += OnDownloadFileCompleted;
                downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            downloadadb:
                WaitProgressText = _loader.GetString("WaitDownload");
                _ = downloader.DownloadFileTaskAsync("https://dl.google.com/android/repository/platform-tools-latest-windows.zip", ADBTemp);
                while (TotalBytesToReceive <= 0)
                {
                    await Task.Delay(1);
                }
                WaitProgressIndeterminate = false;
                ProgressHelper.SetState(ProgressState.Normal, true);
                while (!IsCompleted)
                {
                    ProgressHelper.SetValue(Convert.ToInt32(ReceivedBytesSize), Convert.ToInt32(TotalBytesToReceive), true);
                    WaitProgressText = $"{((double)BytesPerSecondSpeed).GetSizeString()}/s";
                    WaitProgressValue = ProgressPercentage;
                    await Task.Delay(1);
                }
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                WaitProgressIndeterminate = true;
                WaitProgressValue = 0;
                if (exception != null)
                {
                    ProgressHelper.SetState(ProgressState.Error, true);
                    MessageBoxResult result = MessageBox.Show(_loader.GetString("DownloadFailed"), exception.Message, MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == MessageBoxResult.OK)
                    {
                        goto downloadadb;
                    }
                    else
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                }
                downloader.DownloadProgressChanged -= OnDownloadProgressChanged;
                downloader.DownloadFileCompleted -= OnDownloadFileCompleted;
            }
            WaitProgressText = _loader.GetString("UnzipADB");
            await Task.Delay(1);
            string LocalData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "APKInstaller");
            if (!Directory.Exists(LocalData))
            {
                _ = Directory.CreateDirectory(LocalData);
            }
            using (IArchive archive = ArchiveFactory.Open(ADBTemp))
            {
                ProgressHelper.SetState(ProgressState.Normal, true);
                WaitProgressIndeterminate = false;
                int Progressed = 0;
                bool IsCompleted = false;
                double ProgressPercentage = 0;
                int TotalCount = archive.Entries.Count();
                _ = Task.Run(() =>
                {
                    foreach (IArchiveEntry entry in archive.Entries)
                    {
                        Progressed = archive.Entries.ToList().IndexOf(entry) + 1;
                        ProgressPercentage = archive.Entries.GetProgressValue(entry);
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(LocalData, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                    IsCompleted = true;
                });
                while (!IsCompleted)
                {
                    WaitProgressValue = ProgressPercentage;
                    ProgressHelper.SetValue(Progressed, TotalCount, true);
                    WaitProgressText = string.Format(_loader.GetString("UnzippingFormat"), Progressed, TotalCount);
                    await Task.Delay(1);
                }
                WaitProgressValue = 0;
                WaitProgressIndeterminate = true;
                WaitProgressText = _loader.GetString("UnzipComplete");
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
            }
            ADBPath = Path.Combine(LocalData, @"platform-tools\adb.exe");
        }

        public async Task InitilizeADB()
        {
            if (!string.IsNullOrEmpty(_path) || _url != null)
            {
                AdbServer ADBServer = new AdbServer();
                if (!ADBServer.GetStatus().IsRunning)
                {
                    WaitProgressText = _loader.GetString("CheckingADB");
                    Process[] processes = Process.GetProcessesByName("adb");
                    if (processes != null)
                    {
                        WaitProgressText = _loader.GetString("StartingADB");
                        try
                        {
                            await Task.Run(() => ADBServer.StartServer(processes.First().MainModule?.FileName, restartServerIfNewer: false));
                        }
                        catch
                        {
                            foreach (Process process in processes)
                            {
                                process.Kill();
                            }
                            await CheckADB();
                            try
                            {
                                await Task.Run(() => ADBServer.StartServer(ADBPath, restartServerIfNewer: false));
                            }
                            catch
                            {
                                await CheckADB(true);
                                WaitProgressText = _loader.GetString("StartingADB");
                                await Task.Run(() => ADBServer.StartServer(ADBPath, restartServerIfNewer: false));
                            }
                        }
                    }
                    else
                    {
                        await CheckADB();
                        WaitProgressText = _loader.GetString("StartingADB");
                        try
                        {
                            await Task.Run(() => ADBServer.StartServer(ADBPath, restartServerIfNewer: false));
                        }
                        catch
                        {
                            await CheckADB(true);
                            WaitProgressText = _loader.GetString("StartingADB");
                            await Task.Run(() => ADBServer.StartServer(ADBPath, restartServerIfNewer: false));
                        }
                    }
                }
                WaitProgressText = _loader.GetString("Loading");
                if (IsOnlyWSA)
                {
                    new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                }
                ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            }
        }

        public async Task InitilizeUI()
        {
            if (!string.IsNullOrEmpty(_path) || _url != null)
            {
                WaitProgressText = _loader.GetString("Loading");
                if (NetAPKExist)
                {
                    try
                    {
                        ApkInfo = await Task.Run(() => { return AAPTool.Decompile(_path); });
                        AppVersion = string.Format(_loader.GetString("VersionFormat") ?? string.Empty, ApkInfo?.VersionName);
                        PackageName = string.Format(_loader.GetString("PackageNameFormat") ?? string.Empty, ApkInfo?.PackageName);
                    }
                    catch (Exception ex)
                    {
                        PackageError(ex.Message);
                        IsInitialized = true;
                        return;
                    }
                }
                else
                {
                    ApkInfo = new ApkInfo();
                }
                if (string.IsNullOrEmpty(ApkInfo?.PackageName) && NetAPKExist)
                {
                    PackageError(_loader.GetString("InvalidPackage"));
                }
                else
                {
                checkdevice:
                    WaitProgressText = _loader.GetString("Checking");
                    if (CheckDevice() && _device != null && NetAPKExist)
                    {
                        if (NetAPKExist)
                        {
                            CheckAPK();
                        }
                        else
                        {
                            ResetUI();
                            CheckOnlinePackage();
                        }
                    }
                    else
                    {
                        ResetUI();
                        if (NetAPKExist)
                        {
                            ActionButtonEnable = false;
                            ActionButtonText = _loader.GetString("Install");
                            InfoMessage = _loader.GetString("WaitingDevice");
                            DeviceSelectButtonText = _loader.GetString("Devices");
                            ActionVisibility = DeviceSelectVisibility = MessagesToUserVisibility = Visibility.Visible;
                            AppName = string.Format(_loader.GetString("WaitingForInstallFormat") ?? string.Empty, ApkInfo?.AppName);
                        }
                        else
                        {
                            CheckOnlinePackage();
                        }
                        if (ShowDialogs && await ShowDeviceDialog())
                        {
                            goto checkdevice;
                        }
                    }
                }
                WaitProgressText = _loader.GetString("Finished");
            }
            else
            {
                ResetUI();
                ApkInfo = new ApkInfo();
                AppName = _loader.GetString("NoPackageWranning");
                FileSelectButtonText = _loader.GetString("Select");
                CancelOperationButtonText = _loader.GetString("Close");
                FileSelectVisibility = CancelOperationVisibility = Visibility.Visible;
                AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
            }
            IsInitialized = true;
        }

        private async Task<bool> ShowDeviceDialog()
        {
            if (IsOnlyWSA)
            {
                WaitProgressText = _loader.GetString("FindingWSA");
                if ((await PackageHelper.FindPackagesByName("MicrosoftCorporationII.WindowsSubsystemForAndroid")).isfound)
                {
                    WaitProgressText = _loader.GetString("FoundWSA");
                    ProgressHelper.SetState(ProgressState.None, true);
                    MessageBoxResult result = MessageBox.Show(_loader.GetString("HowToConnectInfo"), _loader.GetString("HowToConnect"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == MessageBoxResult.OK)
                    {
                        WaitProgressText = _loader.GetString("LaunchingWSA");
                        _ = Process.Start("wsa://");
                        bool IsWSARunning = false;
                        while (!IsWSARunning)
                        {
                            await Task.Run(() =>
                            {
                                Process[] ps = Process.GetProcessesByName("vmmemWSA");
                                IsWSARunning = ps != null && ps.Length > 0;
                            });
                        }
                        WaitProgressText = _loader.GetString("WaitingWSAStart");
                        while (!CheckDevice())
                        {
                            new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                            await Task.Delay(100);
                        }
                        WaitProgressText = _loader.GetString("WSARunning");
                        return true;
                    }
                }
                else
                {
                    ProgressHelper.SetState(ProgressState.None, true);
                    MessageBoxResult result = MessageBox.Show(_loader.GetString("NoDeviceInfo"), _loader.GetString("NoDevice"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == MessageBoxResult.OK)
                    {
                        _page.NavigationService.Navigate(new SettingsPage());
                    }
                }
            }
            else
            {
                ProgressHelper.SetState(ProgressState.None, true);
                MessageBoxResult result = MessageBox.Show(_loader.GetString("NoDeviceInfo10"), _loader.GetString("NoDevice"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                if (result == MessageBoxResult.OK)
                {
                    _page.NavigationService.Navigate(new SettingsPage());
                }
            }
            return false;
        }

        public async Task ReinitilizeUI()
        {
            if ((!string.IsNullOrEmpty(_path) || _url != null) && NetAPKExist)
            {
            checkdevice:
                if (CheckDevice() && _device != null)
                {
                    CheckAPK();
                }
                else if (ShowDialogs && await ShowDeviceDialog())
                {
                    goto checkdevice;
                }
            }
        }

        public void CheckAPK()
        {
            ResetUI();
            AdvancedAdbClient client = new AdvancedAdbClient();
            PackageManager manager = new PackageManager(client, _device);
            VersionInfo? info = null;
            if (ApkInfo != null)
            {
                info = manager.GetVersionInfo(ApkInfo?.PackageName);
            }
            if (info == null)
            {
                ActionButtonText = _loader.GetString("Install");
                AppName = string.Format(_loader.GetString("InstallFormat") ?? string.Empty, ApkInfo?.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else if (info.VersionCode < int.Parse(ApkInfo?.VersionCode ?? "0"))
            {
                ActionButtonText = _loader.GetString("Update");
                AppName = string.Format(_loader.GetString("UpdateFormat") ?? string.Empty, ApkInfo?.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else
            {
                ActionButtonText = _loader.GetString("Reinstall");
                SecondaryActionButtonText = _loader.GetString("Launch");
                AppName = string.Format(_loader.GetString("ReinstallFormat") ?? string.Empty, ApkInfo?.AppName);
                TextOutput = string.Format(_loader.GetString("ReinstallOutput") ?? string.Empty, ApkInfo?.AppName);
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = Visibility.Visible;
            }
        }

        public void CheckOnlinePackage()
        {
            Regex[] UriRegex = new Regex[] { new Regex(@":\?source=(.*)"), new Regex(@"://(.*)") };
            string Uri = UriRegex[0].IsMatch(_url.ToString()) ? UriRegex[0].Match(_url.ToString()).Groups[1].Value : UriRegex[1].Match(_url.ToString()).Groups[1].Value;
            Uri Url = Uri.ValidateAndGetUri();
            if (Url != null)
            {
                _url = Url;
                AppName = _loader.GetString("OnlinePackage");
                DownloadButtonText = _loader.GetString("Download");
                CancelOperationButtonText = _loader.GetString("Close");
                DownloadVisibility = CancelOperationVisibility = Visibility.Visible;
                AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
                if (AutoGetNetAPK)
                {
                    LoadNetAPK();
                }
            }
            else
            {
                PackageError(_loader.GetString("InvalidURL"));
            }
        }

        public async void LoadNetAPK()
        {
            IsInstalling = true;
            DownloadVisibility = Visibility.Collapsed;
            try
            {
                await DownloadAPK();
            }
            catch (Exception ex)
            {
                PackageError(ex.Message);
                IsInstalling = false;
                return;
            }

            try
            {
                ApkInfo = await Task.Run(() => { return AAPTool.Decompile(_path); });
                AppVersion = string.Format(_loader.GetString("VersionFormat") ?? string.Empty, ApkInfo?.VersionName);
                PackageName = string.Format(_loader.GetString("PackageNameFormat") ?? string.Empty, ApkInfo?.PackageName);
            }
            catch (Exception ex)
            {
                PackageError(ex.Message);
                IsInstalling = false;
                return;
            }

            if (string.IsNullOrEmpty(ApkInfo?.PackageName))
            {
                PackageError(_loader.GetString("InvalidPackage"));
            }
            else
            {
                if (CheckDevice() && _device != null)
                {
                    CheckAPK();
                }
                else
                {
                    ResetUI();
                    ActionButtonEnable = false;
                    ActionButtonText = _loader.GetString("Install");
                    InfoMessage = _loader.GetString("WaitingDevice");
                    DeviceSelectButtonText = _loader.GetString("Devices");
                    AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), ApkInfo?.AppName);
                    ActionVisibility = DeviceSelectVisibility = MessagesToUserVisibility = Visibility.Visible;
                }
            }
            IsInstalling = false;
        }

        public async Task DownloadAPK()
        {
            if (_url != null)
            {
                if (!Directory.Exists(APKTemp.Substring(0, APKTemp.LastIndexOf(@"\"))))
                {
                    Directory.CreateDirectory(APKTemp.Substring(0, APKTemp.LastIndexOf(@"\")));
                }
                else if (Directory.Exists(APKTemp))
                {
                    Directory.Delete(APKTemp, true);
                }
                using (DownloadService downloader = new DownloadService(DownloadHelper.Configuration))
                {
                    bool IsCompleted = false;
                    Exception exception = null;
                    long ReceivedBytesSize = 0;
                    long TotalBytesToReceive = 0;
                    double ProgressPercentage = 0;
                    double BytesPerSecondSpeed = 0;
                    void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
                    {
                        exception = e.Error;
                        IsCompleted = true;
                    }
                    void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
                    {
                        ReceivedBytesSize = e.ReceivedBytesSize;
                        ProgressPercentage = e.ProgressPercentage;
                        TotalBytesToReceive = e.TotalBytesToReceive;
                        BytesPerSecondSpeed = e.BytesPerSecondSpeed;
                    }
                    downloader.DownloadFileCompleted += OnDownloadFileCompleted;
                    downloader.DownloadProgressChanged += OnDownloadProgressChanged;
                downloadapk:
                    ProgressText = _loader.GetString("WaitDownload");
                    _ = downloader.DownloadFileTaskAsync(_url.ToString(), APKTemp);
                    while (TotalBytesToReceive <= 0)
                    {
                        await Task.Delay(1);
                    }
                    AppxInstallBarIndeterminate = false;
                    ProgressHelper.SetState(ProgressState.Normal, true);
                    while (!IsCompleted)
                    {
                        ProgressHelper.SetValue(Convert.ToInt32(ReceivedBytesSize), Convert.ToInt32(TotalBytesToReceive), true);
                        ProgressText = $"{ProgressPercentage:N2}% {((double)BytesPerSecondSpeed).GetSizeString()}/s";
                        AppxInstallBarValue = ProgressPercentage;
                        await Task.Delay(1);
                    }
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    ProgressText = _loader.GetString("Loading");
                    AppxInstallBarIndeterminate = true;
                    AppxInstallBarValue = 0;
                    if (exception != null)
                    {
                        ProgressHelper.SetState(ProgressState.Error, true);
                        MessageBoxResult result = MessageBox.Show(exception.Message, _loader.GetString("DownloadFailed"), MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                        if (result == MessageBoxResult.OK)
                        {
                            goto downloadapk;
                        }
                        else
                        {
                            Application.Current.Shutdown();
                            return;
                        }
                    }
                    downloader.DownloadProgressChanged -= OnDownloadProgressChanged;
                    downloader.DownloadFileCompleted -= OnDownloadFileCompleted;
                }
            }
        }

        private void ResetUI()
        {
            ActionVisibility =
            SecondaryActionVisibility =
            FileSelectVisibility =
            DownloadVisibility =
            DeviceSelectVisibility =
            CancelOperationVisibility =
            TextOutputVisibility =
            InstallOutputVisibility =
            LaunchWhenReadyVisibility =
            MessagesToUserVisibility = Visibility.Collapsed;
            AppVersionVisibility =
            AppPublisherVisibility =
            AppCapabilitiesVisibility = Visibility.Visible;
            AppxInstallBarIndeterminate =
            ActionButtonEnable =
            SecondaryActionButtonEnable =
            FileSelectButtonEnable =
            DownloadButtonEnable =
            DeviceSelectButtonEnable =
            CancelOperationButtonEnable = true;
        }

        private void PackageError(string? message)
        {
            ResetUI();
            TextOutput = message;
            ApkInfo ??= new ApkInfo();
            AppName = _loader.GetString("CannotOpenPackage");
            ProgressHelper.SetState(ProgressState.Error, true);
            TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
            AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
        }

        private void OnDeviceChanged(object? sender, DeviceDataEventArgs e)
        {
            if (IsInitialized && !IsInstalling)
            {
                _page.RunOnUIThread(() =>
                {
                    if (CheckDevice() && _device != null)
                    {
                        CheckAPK();
                    }
                });
            }
        }

        public bool CheckDevice()
        {
            AdvancedAdbClient client = new AdvancedAdbClient();
            List<DeviceData> devices = client.GetDevices();
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            if (devices.Count <= 0) { return false; }
            foreach (DeviceData device in devices)
            {
                if (device == null || device.State == DeviceState.Offline) { continue; }
                if (IsOnlyWSA)
                {
                    client.ExecuteRemoteCommand("getprop ro.boot.hardware", device, receiver);
                    if (receiver.ToString().Contains("windows"))
                    {
                        _device = device ?? _device;
                        return true;
                    }
                }
                else
                {
                    string DefaultDevice = Settings.Default.DefaultDevice;
                    DeviceData? data = string.IsNullOrEmpty(DefaultDevice) ? null : JsonSerializer.Deserialize<DeviceData>(DefaultDevice);
                    if (data != null && data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                    {
                        _device = data;
                        return true;
                    }
                }
            }
            return false;
        }

        public void OpenAPP() => new AdvancedAdbClient().StartApp(_device, ApkInfo?.PackageName);

        public async void InstallAPP()
        {
            try
            {
                IsInstalling = true;
                ProgressText = _loader.GetString("Installing");
                CancelOperationButtonText = _loader.GetString("Cancel");
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = InstallOutputVisibility = Visibility.Collapsed;
                await Task.Run(() =>
                {
                    new AdvancedAdbClient().Install(_device, File.Open(ApkInfo.FullPath, FileMode.Open, FileAccess.Read));
                });
                AppName = string.Format(_loader.GetString("InstalledFormat"), ApkInfo?.AppName);
                if (IsOpenApp)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);// 据说如果安装完直接启动会崩溃。。。
                        OpenAPP();
                        if (IsCloseAPP)
                        {
                            await Task.Delay(5000);
                            _page.RunOnUIThread(() => Application.Current.Shutdown());
                        }
                    });
                }
                IsInstalling = false;
                SecondaryActionVisibility = Visibility.Visible;
                SecondaryActionButtonText = _loader.GetString("Launch");
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                IsInstalling = false;
                TextOutput = ex.Message;
                TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
            }
        }

        public async void OpenAPK()
        {
            OpenFileDialog? FileOpen = new OpenFileDialog();
            FileOpen.Filter = ".apk|*.apk|.apks|*.apks|.apkm|*.apkm";
            FileOpen.Title = _loader.GetString("OpenAPK");
            if (FileOpen.ShowDialog() == false)
            {
                return;
            }

            _path = FileOpen.FileName;
            await Refresh();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
