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
using ModernWpf.Controls;
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
using Windows.Storage;
using Windows.System;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;
using ModernWpf;
using SharpCompress.Writers;

namespace APKInstaller.ViewModel
{
    internal class InstallViewModel : INotifyPropertyChanged, IDisposable
    {
        private DeviceData _device;
        private readonly InstallPage _page;

        private string APKTemp => Path.Combine(CachesHelper.TempPath, "NetAPKTemp.apk");
        private string ADBTemp => Path.Combine(CachesHelper.TempPath, "platform-tools.zip");

#if !DEBUG
        private Uri _url;
        private string _path = string.Empty;
#else
        private Uri _url = new Uri("apkinstaller:?source=https://dl.coolapk.com/down?pn=com.coolapk.market&id=NDU5OQ&h=46bb9d98&from=from-web");
        private string _path = @"C:\Users\qq251\Downloads\Programs\weixin8028android2240_arm64.apk";
#endif
        private bool NetAPKExist => _path != APKTemp || File.Exists(_path);

        private bool _disposedValue;
        private readonly ResourceManager _loader = new ResourceManager(typeof(InstallStrings));

        public static InstallViewModel Caches;
        public string InstallFormat => _loader.GetString("InstallFormat");
        public string VersionFormat => _loader.GetString("VersionFormat");
        public string PackageNameFormat => _loader.GetString("PackageNameFormat");

        private static bool IsOnlyWSA => SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA);
        private static bool IsCloseAPP => SettingsHelper.Get<bool>(SettingsHelper.IsCloseAPP);
        private static bool ShowDialogs => SettingsHelper.Get<bool>(SettingsHelper.ShowDialogs);
        private static bool AutoGetNetAPK => SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK);

        private ApkInfo _apkInfo = null;
        public ApkInfo ApkInfo
        {
            get => _apkInfo;
            set
            {
                if (_apkInfo != value)
                {
                    _apkInfo = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public string ADBPath
        {
            get => SettingsHelper.Get<string>(SettingsHelper.ADBPath);
            set
            {
                if (ADBPath != value)
                {
                    SettingsHelper.Set(SettingsHelper.ADBPath, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public static bool IsOpenApp
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsOpenApp);
            set => SettingsHelper.Set(SettingsHelper.IsOpenApp, value);
        }

        private bool _isInstalling;
        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                if (_isInstalling != value)
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
        }

        private bool _isInitialized;
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                if (_isInitialized != value)
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
        }

        private string _appName;
        public string AppName
        {
            get => _appName;
            set
            {
                if (_appName != value)
                {
                    _appName = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _appVersion;
        public string AppVersion
        {
            get => _appVersion;
            set
            {
                if (_appVersion != value)
                {
                    _appVersion = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _packageName;
        public string PackageName
        {
            get => _packageName;
            set
            {
                if (_packageName != value)
                {
                    _packageName = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _textOutput;
        public string TextOutput
        {
            get => _textOutput;
            set
            {
                if (_textOutput != value)
                {
                    _textOutput = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _infoMessage;
        public string InfoMessage
        {
            get => _infoMessage;
            set
            {
                if (_infoMessage != value)
                {
                    _infoMessage = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set
            {
                if (_progressText != value)
                {
                    _progressText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _actionButtonEnable;
        public bool ActionButtonEnable
        {
            get => _actionButtonEnable;
            set
            {
                if (_actionButtonEnable != value)
                {
                    _actionButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _secondaryActionButtonEnable;
        public bool SecondaryActionButtonEnable
        {
            get => _secondaryActionButtonEnable;
            set
            {
                if (_secondaryActionButtonEnable != value)
                {
                    _secondaryActionButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _fileSelectButtonEnable;
        public bool FileSelectButtonEnable
        {
            get => _fileSelectButtonEnable;
            set
            {
                if (_fileSelectButtonEnable != value)
                {
                    _fileSelectButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _downloadButtonEnable;
        public bool DownloadButtonEnable
        {
            get => _downloadButtonEnable;
            set
            {
                if (_downloadButtonEnable != value)
                {
                    _downloadButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _deviceSelectButtonEnable;
        public bool DeviceSelectButtonEnable
        {
            get => _deviceSelectButtonEnable;
            set
            {
                if (_deviceSelectButtonEnable != value)
                {
                    _deviceSelectButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _cancelOperationButtonEnable;
        public bool CancelOperationButtonEnable
        {
            get => _cancelOperationButtonEnable;
            set
            {
                if (_cancelOperationButtonEnable != value)
                {
                    _cancelOperationButtonEnable = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _waitProgressText;
        public string WaitProgressText
        {
            get => _waitProgressText;
            set
            {
                if (_waitProgressText != value)
                {
                    _waitProgressText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private double _waitProgressValue = 0;
        public double WaitProgressValue
        {
            get => _waitProgressValue;
            set
            {
                if (_waitProgressValue != value)
                {
                    _waitProgressValue = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private double _appxInstallBarValue = 0;
        public double AppxInstallBarValue
        {
            get => _appxInstallBarValue;
            set
            {
                if (_appxInstallBarValue != value)
                {
                    _appxInstallBarValue = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _waitProgressIndeterminate = true;
        public bool WaitProgressIndeterminate
        {
            get => _waitProgressIndeterminate;
            set
            {
                if (_waitProgressIndeterminate != value)
                {
                    _waitProgressIndeterminate = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _appxInstallBarIndeterminate = true;
        public bool AppxInstallBarIndeterminate
        {
            get => _appxInstallBarIndeterminate;
            set
            {
                if (_appxInstallBarIndeterminate != value)
                {
                    _appxInstallBarIndeterminate = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _actionButtonText;
        public string ActionButtonText
        {
            get => _actionButtonText;
            set
            {
                if (_actionButtonText != value)
                {
                    _actionButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _secondaryActionButtonText;
        public string SecondaryActionButtonText
        {
            get => _secondaryActionButtonText;
            set
            {
                if (_secondaryActionButtonText != value)
                {
                    _secondaryActionButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _fileSelectButtonText;
        public string FileSelectButtonText
        {
            get => _fileSelectButtonText;
            set
            {
                if (_fileSelectButtonText != value)
                {
                    _fileSelectButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _downloadButtonText;
        public string DownloadButtonText
        {
            get => _downloadButtonText;
            set
            {
                if (_downloadButtonText != value)
                {
                    _downloadButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _deviceSelectButtonText;
        public string DeviceSelectButtonText
        {
            get => _deviceSelectButtonText;
            set
            {
                if (_deviceSelectButtonText != value)
                {
                    _deviceSelectButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _cancelOperationButtonText;
        public string CancelOperationButtonText
        {
            get => _cancelOperationButtonText;
            set
            {
                if (_cancelOperationButtonText != value)
                {
                    _cancelOperationButtonText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _textOutputVisibility = Visibility.Collapsed;
        public Visibility TextOutputVisibility
        {
            get => _textOutputVisibility;
            set
            {
                if (_textOutputVisibility != value)
                {
                    _textOutputVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _installOutputVisibility = Visibility.Collapsed;
        public Visibility InstallOutputVisibility
        {
            get => _installOutputVisibility;
            set
            {
                if (_installOutputVisibility != value)
                {
                    _installOutputVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _actionVisibility = Visibility.Collapsed;
        public Visibility ActionVisibility
        {
            get => _actionVisibility;
            set
            {
                if (_actionVisibility != value)
                {
                    _actionVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _secondaryActionVisibility = Visibility.Collapsed;
        public Visibility SecondaryActionVisibility
        {
            get => _secondaryActionVisibility;
            set
            {
                if (_secondaryActionVisibility != value)
                {
                    _secondaryActionVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _fileSelectVisibility = Visibility.Collapsed;
        public Visibility FileSelectVisibility
        {
            get => _fileSelectVisibility;
            set
            {
                if (_fileSelectVisibility != value)
                {
                    _fileSelectVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _downloadVisibility = Visibility.Collapsed;
        public Visibility DownloadVisibility
        {
            get => _downloadVisibility;
            set
            {
                if (_downloadVisibility != value)
                {
                    _downloadVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _deviceSelectVisibility = Visibility.Collapsed;
        public Visibility DeviceSelectVisibility
        {
            get => _deviceSelectVisibility;
            set
            {
                if (_deviceSelectVisibility != value)
                {
                    _deviceSelectVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _cancelOperationVisibility = Visibility.Collapsed;
        public Visibility CancelOperationVisibility
        {
            get => _cancelOperationVisibility;
            set
            {
                if (_cancelOperationVisibility != value)
                {
                    _cancelOperationVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _messagesToUserVisibility = Visibility.Collapsed;
        public Visibility MessagesToUserVisibility
        {
            get => _messagesToUserVisibility;
            set
            {
                if (_messagesToUserVisibility != value)
                {
                    _messagesToUserVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _launchWhenReadyVisibility = Visibility.Collapsed;
        public Visibility LaunchWhenReadyVisibility
        {
            get => _launchWhenReadyVisibility;
            set
            {
                if (_launchWhenReadyVisibility != value)
                {
                    _launchWhenReadyVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _appVersionVisibility;
        public Visibility AppVersionVisibility
        {
            get => _appVersionVisibility;
            set
            {
                if (_appVersionVisibility != value)
                {
                    _appVersionVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _appPublisherVisibility;
        public Visibility AppPublisherVisibility
        {
            get => _appPublisherVisibility;
            set
            {
                if (_appPublisherVisibility != value)
                {
                    _appPublisherVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _appCapabilitiesVisibility;
        public Visibility AppCapabilitiesVisibility
        {
            get => _appCapabilitiesVisibility;
            set
            {
                if (_appCapabilitiesVisibility != value)
                {
                    _appCapabilitiesVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        public InstallViewModel(Uri Url, InstallPage Page)
        {
            _url = Url;
            _page = Page;
            Caches = this;
            _path = APKTemp;
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        public InstallViewModel(string Path, InstallPage Page)
        {
            _page = Page;
            Caches = this;
            _path = string.IsNullOrEmpty(Path) ? _path : Path;
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public async Task Refresh(bool force = true)
        {
            IsInitialized = false;
            try
            {
                if (force)
                {
                    await InitilizeADB();
                    await InitilizeUI();
                }
                else
                {
                    await ReinitilizeUI();
                    IsInitialized = true;
                }
            }
            catch (Exception ex)
            {
                IsInitialized = true;
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
                StackPanel StackPanel = new StackPanel();
                StackPanel.Children.Add(
                    new TextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = _loader.GetString("AboutADB")
                    });
                StackPanel.Children.Add(
                    new HyperlinkButton
                    {
                        Content = _loader.GetString("ClickToRead"),
                        NavigateUri = new Uri("https://developer.android.google.cn/studio/releases/platform-tools?hl=zh-cn")
                    });
                ContentDialog dialog = new ContentDialog
                {
                    Title = _loader.GetString("ADBMissing"),
                    PrimaryButtonText = _loader.GetString("Download"),
                    SecondaryButtonText = _loader.GetString("Select"),
                    CloseButtonText = _loader.GetString("Cancel"),
                    Content = new ScrollViewer()
                    {
                        Content = StackPanel
                    },
                    DefaultButton = ContentDialogButton.Primary
                };
                ProgressHelper.SetState(ProgressState.None, true);
                ContentDialogResult result = await dialog.ShowAsync();
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                if (result == ContentDialogResult.Primary)
                {
                downloadadb:
                    if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        try
                        {
                            await DownloadADB();
                        }
                        catch (Exception ex)
                        {
                            ContentDialog dialogs = new ContentDialog
                            {
                                Title = _loader.GetString("DownloadFailed"),
                                PrimaryButtonText = _loader.GetString("Retry"),
                                CloseButtonText = _loader.GetString("Cancel"),
                                Content = new TextBlock { Text = ex.Message },
                                DefaultButton = ContentDialogButton.Primary
                            };
                            ProgressHelper.SetState(ProgressState.None, true);
                            ContentDialogResult results = await dialogs.ShowAsync();
                            ProgressHelper.SetState(ProgressState.Indeterminate, true);
                            if (results == ContentDialogResult.Primary)
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
                    else
                    {
                        ContentDialog dialogs = new ContentDialog
                        {
                            Title = _loader.GetString("NoInternet"),
                            PrimaryButtonText = _loader.GetString("Retry"),
                            CloseButtonText = _loader.GetString("Cancel"),
                            Content = new TextBlock { Text = _loader.GetString("NoInternetInfo") },
                            DefaultButton = ContentDialogButton.Primary
                        };
                        ProgressHelper.SetState(ProgressState.None, true);
                        ContentDialogResult results = await dialogs.ShowAsync();
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                        if (results == ContentDialogResult.Primary)
                        {
                            goto checkadb;
                        }
                        else
                        {
                            Application.Current.Shutdown();
                            return;
                        }
                    }
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    OpenFileDialog FileOpen = new OpenFileDialog();
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
            if (!Directory.Exists(ADBTemp[..ADBTemp.LastIndexOf(@"\")]))
            {
                _ = Directory.CreateDirectory(ADBTemp[..ADBTemp.LastIndexOf(@"\")]);
            }
            else if (Directory.Exists(ADBTemp))
            {
                Directory.Delete(ADBTemp, true);
            }
            using (DownloadService downloader = new DownloadService(DownloadHelper.Configuration))
            {
                bool IsCompleted = false;
                long ReceivedBytesSize = 0;
                Exception exception = null;
                long TotalBytesToReceive = 0;
                double ProgressPercentage = 0;
                double BytesPerSecondSpeed = 0;
                void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
                {
                    exception = e.Error;
                    IsCompleted = true;
                }
                void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
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
                    ContentDialog dialog = new ContentDialog
                    {
                        Content = exception.Message,
                        Title = _loader.GetString("DownloadFailed"),
                        PrimaryButtonText = _loader.GetString("Retry"),
                        CloseButtonText = _loader.GetString("Cancel"),
                        DefaultButton = ContentDialogButton.Primary
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == ContentDialogResult.Primary)
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
            string LocalData = PackagedAppHelper.IsPackagedApp ? ApplicationData.Current.LocalFolder.Path : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "APKInstaller");
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
            WaitProgressText = _loader.GetString("Loading");
            if (!string.IsNullOrEmpty(_path) || _url != null)
            {
                IAdbServer ADBServer = AdbServer.Instance;
                if (!ADBServer.GetStatus().IsRunning)
                {
                    WaitProgressText = _loader.GetString("CheckingADB");
                    await CheckADB();
                    Process[] processes = Process.GetProcessesByName("adb");
                startadb:
                    WaitProgressText = _loader.GetString("StartingADB");
                    try
                    {
                        await Task.Run(() => ADBServer.StartServer((processes != null && processes.Any()) ? processes.First().MainModule?.FileName : ADBPath, restartServerIfNewer: false));
                    }
                    catch
                    {
                        if (processes != null && processes.Any())
                        {
                            foreach (Process process in processes)
                            {
                                process.Kill();
                            }
                            processes = null;
                        }
                        await CheckADB(true);
                        goto startadb;
                    }
                }
                WaitProgressText = _loader.GetString("Loading");
                if (!CheckDevice())
                {
                    if (IsOnlyWSA)
                    {
                        if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                        {
                            await AddressHelper.ConnectHyperV();
                            if (!CheckDevice())
                            {
                                new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                            }
                        }
                        else
                        {
                            new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                        }
                    }
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
                        AppVersion = string.Format(_loader.GetString("VersionFormat") ?? string.Empty, ApkInfo.VersionName);
                        PackageName = string.Format(_loader.GetString("PackageNameFormat") ?? string.Empty, ApkInfo.PackageName);
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
                if (string.IsNullOrEmpty(ApkInfo.PackageName) && NetAPKExist)
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
                            AppName = string.Format(_loader.GetString("WaitingForInstallFormat") ?? string.Empty, ApkInfo.AppName);
                        }
                        else
                        {
                            CheckOnlinePackage();
                        }
                        if (ShowDialogs && await _page.ExecuteOnUIThreadAsync(ShowDeviceDialog))
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
                if ((await PackageHelper.FindPackagesByName("MicrosoftCorporationII.WindowsSubsystemForAndroid_8wekyb3d8bbwe")).isfound)
                {
                    WaitProgressText = _loader.GetString("FoundWSA");
                    ContentDialog dialog = new ContentDialog
                    {
                        DefaultButton = ContentDialogButton.Close,
                        Title = _loader.GetString("HowToConnect"),
                        Content = _loader.GetString("HowToConnectInfo"),
                        CloseButtonText = _loader.GetString("IKnow"),
                        PrimaryButtonText = _loader.GetString("StartWSA"),
                    };
                    ProgressHelper.SetState(ProgressState.None, true);
                    ContentDialogResult result = await dialog.ShowAsync();
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == ContentDialogResult.Primary)
                    {
                        WaitProgressText = _loader.GetString("LaunchingWSA");
#if NET5_OR_GREATER
                        _ = await Launcher.LaunchUriAsync(new Uri("wsa://"));
#else
                        _ = Process.Start("wsa://");
#endif
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
                            if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                            {
                                await AddressHelper.ConnectHyperV();
                            }
                            else if (IsOnlyWSA)
                            {
                                new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                            }
                            await Task.Delay(100);
                        }
                        WaitProgressText = _loader.GetString("WSARunning");
                        return true;
                    }
                }
                else
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = _loader.GetString("NoDevice"),
                        DefaultButton = ContentDialogButton.Close,
                        CloseButtonText = _loader.GetString("IKnow"),
                        PrimaryButtonText = _loader.GetString("GoToSetting"),
                        Content = _loader.GetString("NoDeviceInfo"),
                    };
                    ProgressHelper.SetState(ProgressState.None, true);
                    ContentDialogResult result = await dialog.ShowAsync();
                    ProgressHelper.SetState(ProgressState.Indeterminate, true);
                    if (result == ContentDialogResult.Primary)
                    {
                        UIHelper.Navigate(typeof(SettingsPage), null);
                    }
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = _loader.GetString("NoDevice"),
                    DefaultButton = ContentDialogButton.Close,
                    CloseButtonText = _loader.GetString("IKnow"),
                    PrimaryButtonText = _loader.GetString("GoToSetting"),
                    Content = _loader.GetString("NoDeviceInfo10"),
                };
                ProgressHelper.SetState(ProgressState.None, true);
                ContentDialogResult result = await dialog.ShowAsync();
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                if (result == ContentDialogResult.Primary)
                {
                    UIHelper.Navigate(typeof(SettingsPage), null);
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
                else if (ShowDialogs && await _page.ExecuteOnUIThreadAsync(ShowDeviceDialog))
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
            VersionInfo info = null;
            if (ApkInfo != null)
            {
                info = manager.GetVersionInfo(ApkInfo.PackageName);
            }
            if (info == null)
            {
                ActionButtonText = _loader.GetString("Install");
                AppName = string.Format(_loader.GetString("InstallFormat") ?? string.Empty, ApkInfo.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else if (info.VersionCode < int.Parse(ApkInfo.VersionCode ?? "0"))
            {
                ActionButtonText = _loader.GetString("Update");
                AppName = string.Format(_loader.GetString("UpdateFormat") ?? string.Empty, ApkInfo.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else
            {
                ActionButtonText = _loader.GetString("Reinstall");
                SecondaryActionButtonText = _loader.GetString("Launch");
                AppName = string.Format(_loader.GetString("ReinstallFormat") ?? string.Empty, ApkInfo.AppName);
                TextOutput = string.Format(_loader.GetString("ReinstallOutput") ?? string.Empty, ApkInfo.AppName);
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
                AppVersion = string.Format(_loader.GetString("VersionFormat") ?? string.Empty, ApkInfo.VersionName);
                PackageName = string.Format(_loader.GetString("PackageNameFormat") ?? string.Empty, ApkInfo.PackageName);
            }
            catch (Exception ex)
            {
                PackageError(ex.Message);
                IsInstalling = false;
                return;
            }

            if (string.IsNullOrEmpty(ApkInfo.PackageName))
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
                    AppName = string.Format(_loader.GetString("WaitingForInstallFormat"), ApkInfo.AppName);
                    ActionVisibility = DeviceSelectVisibility = MessagesToUserVisibility = Visibility.Visible;
                }
            }
            IsInstalling = false;
        }

        public async Task DownloadAPK()
        {
            if (_url != null)
            {
                if (!Directory.Exists(APKTemp[..APKTemp.LastIndexOf(@"\")]))
                {
                    Directory.CreateDirectory(APKTemp[..APKTemp.LastIndexOf(@"\")]);
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
                    void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
                    {
                        exception = e.Error;
                        IsCompleted = true;
                    }
                    void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
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
                        ContentDialog dialog = new ContentDialog
                        {
                            Content = exception.Message,
                            Title = _loader.GetString("DownloadFailed"),
                            PrimaryButtonText = _loader.GetString("Retry"),
                            CloseButtonText = _loader.GetString("Cancel"),
                            DefaultButton = ContentDialogButton.Primary
                        };
                        ContentDialogResult result = await dialog.ShowAsync();
                        ProgressHelper.SetState(ProgressState.Indeterminate, true);
                        if (result == ContentDialogResult.Primary)
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

        private void PackageError(string message)
        {
            ResetUI();
            TextOutput = message;
            ApkInfo ??= new ApkInfo();
            AppName = _loader.GetString("CannotOpenPackage");
            ProgressHelper.SetState(ProgressState.Error, true);
            TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
            AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
        }

        private void OnDeviceChanged(object sender, DeviceDataEventArgs e)
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
                    string DefaultDevice = SettingsHelper.Get<string>(SettingsHelper.DefaultDevice);
                    DeviceData data = string.IsNullOrEmpty(DefaultDevice) ? null : JsonSerializer.Deserialize<DeviceData>(DefaultDevice);
                    if (data != null && data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                    {
                        _device = data;
                        return true;
                    }
                }
            }
            return false;
        }

        public void OpenAPP() => new AdvancedAdbClient().StartApp(_device, ApkInfo.PackageName);

        public async void InstallAPP()
        {
            try
            {
                IsInstalling = true;
                ProgressText = _loader.GetString("Installing");
                CancelOperationButtonText = _loader.GetString("Cancel");
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = InstallOutputVisibility = Visibility.Collapsed;
                if (ApkInfo.IsSplit)
                {
                    await Task.Run(() => { new AdvancedAdbClient().InstallMultiple(_device, new Stream[] { File.Open(ApkInfo.FullPath, FileMode.Open, FileAccess.Read) }, ApkInfo.PackageName); });
                }
                else if (ApkInfo.IsBundle)
                {
                    await Task.Run(() =>
                    {
                        Stream[] streams = ApkInfo.SplitApks.Select(x => File.Open(x.FullPath, FileMode.Open, FileAccess.Read)).ToArray();
                        new AdvancedAdbClient().InstallMultiple(_device, File.Open(ApkInfo.FullPath, FileMode.Open, FileAccess.Read), streams);
                    });
                }
                else
                {
                    await Task.Run(() => { new AdvancedAdbClient().Install(_device, File.Open(ApkInfo.FullPath, FileMode.Open, FileAccess.Read)); });
                }
                AppName = string.Format(_loader.GetString("InstalledFormat"), ApkInfo.AppName);
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

        public async Task OpenAPK(string path)
        {
            if (path != null)
            {
                _path = path;
                await Refresh();
            }
        }

        public async void OpenAPK()
        {
            OpenFileDialog FileOpen = new OpenFileDialog();
            FileOpen.Filter = ".apk|*.apk|.apks|*.apks|.apkm|*.apkm|.xapk|*.xapk";
            FileOpen.Title = _loader.GetString("OpenAPK");
            if (FileOpen.ShowDialog() == false)
            {
                return;
            }

            _path = FileOpen.FileName;
            await Refresh();
        }
        
        public async void OpenAPK(IDataObject data)
        {
            bool finnish = false;

            _ = Task.Run(async () =>
            {
                await Task.Delay(300);
                if (!finnish)
                {
                    _page?.RunOnUIThread(() =>
                    {
                        WaitProgressText = _loader.GetString("CheckingPath");
                        IsInitialized = finnish;
                    });
                }
            });

            await Task.Run(async () =>
            {
                if (data.GetDataPresent(DataFormats.FileDrop))
                {
                    var items = data.GetData(DataFormats.FileDrop) as Array;
                    if (items.Length == 1)
                    {
                        await OpenPath(items.GetValue(0).ToString());
                    }
                    else if (items.Length >= 1)
                    {
                        await CreateAPKS(items);
                    }
                }
                else if (data.GetDataPresent(DataFormats.Text))
                {
                    await OpenPath(data.GetData(DataFormats.Text) as string);
                }
                else if (data.GetDataPresent(DataFormats.UnicodeText))
                {
                    await OpenPath(data.GetData(DataFormats.UnicodeText) as string);
                }
            });

            finnish = true;
            IsInitialized = true;

            async Task OpenPath(string item)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (Directory.Exists(item))
                    {
                        List<string> apks = new();
                        var files = Directory.GetFiles(item);
                        await CreateAPKS(files);
                    }
                    else if (File.Exists(item))
                    {
                        if (item.ToLower().EndsWith(".apk"))
                        {
                            finnish = true;
                            await OpenAPK(item);
                            return;
                        }
                        try
                        {
                            using (IArchive archive = ArchiveFactory.Open(item))
                            {
                                foreach (IArchiveEntry entry in archive.Entries.Where(x => !x.Key.Contains('/')))
                                {
                                    if (entry.Key.ToLower().EndsWith(".apk"))
                                    {
                                        finnish = true;
                                        await OpenAPK(item);
                                        return;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            finnish = true;
                            return;
                        }
                    }
                }
            }

            async Task CreateAPKS(Array items)
            {
                List<string> apks = new();
                foreach (object i in items)
                {
                    string item = i.ToString();
                    if (!string.IsNullOrEmpty(item) && File.Exists(item))
                    {
                        if (item.ToLower().EndsWith(".apk"))
                        {
                            apks.Add(item);
                            continue;
                        }
                        try
                        {
                            using (IArchive archive = ArchiveFactory.Open(item))
                            {
                                foreach (IArchiveEntry entry in archive.Entries.Where(x => !x.Key.Contains('/')))
                                {
                                    if (entry.Key.ToLower().EndsWith(".apk"))
                                    {
                                        finnish = true;
                                        await OpenAPK(item);
                                        return;
                                    }
                                }
                            }
                            apks.Add(item);
                            continue;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                if (apks.Count == 1)
                {
                    finnish = true;
                    await OpenAPK(apks.First());
                    return;
                }
                else if (apks.Count >= 1)
                {
                    var apklist = apks.Where(x => x.EndsWith(".apk"));
                    if (apklist.Any())
                    {
                        string temp = Path.Combine(CachesHelper.TempPath, "NetAPKTemp.apks");

                        if (!Directory.Exists(temp[..temp.LastIndexOf(@"\")]))
                        {
                            _ = Directory.CreateDirectory(temp[..temp.LastIndexOf(@"\")]);
                        }
                        else if (Directory.Exists(temp))
                        {
                            Directory.Delete(temp, true);
                        }

                        if (File.Exists(temp))
                        {
                            File.Delete(temp);
                        }

                        using (FileStream zip = File.OpenWrite(temp))
                        {
                            using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
                            {
                                foreach (string apk in apks.Where(x => x.EndsWith(".apk")))
                                {
                                    zipWriter.Write(Path.GetFileName(apk), apk);
                                }
                                finnish = true;
                                await OpenAPK(temp);
                                return;
                            }
                        }
                    }
                    else
                    {
                        var apkslist = apks.Where(x => !x.EndsWith(".apk"));
                        if (apkslist.Count() == 1)
                        {
                            finnish = true;
                            await OpenAPK(apkslist.First());
                            return;
                        }
                    }
                }
            }
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
