using AAPTForNet;
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
using PortableDownloader;
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

namespace APKInstaller.ViewModel
{
    internal class InstallViewModel : INotifyPropertyChanged, IDisposable
    {
        private DeviceData? _device;
        private readonly InstallPage _page;

        private readonly string TempPath = Path.Combine(Path.GetTempPath(), @$"APKInstaller\Caches\{Environment.ProcessId}");
        private string APKTemp => Path.Combine(TempPath, @"NetAPKTemp.apk");
        private string ADBTemp => Path.Combine(TempPath, @"platform-tools.zip");

#if !DEBUG
        private Uri? _url;
        private string _path = string.Empty;
#else
        private Uri? _url = new Uri("apkinstaller:?source=https://dl.coolapk.com/down?pn=com.coolapk.market&id=NDU5OQ&h=46bb9d98&from=from-web");
        private string _path = @"C:\Users\qq251\Downloads\Programs\Skit_com,pavelrekun,skit,premium_2,4,1.apks";
#endif
        private bool NetAPKExist => _path != APKTemp || File.Exists(_path);

        private bool _disposedValue;
        private static bool IsOnlyWSA => PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.IsOnlyWSA) : Settings.Default.IsOnlyWSA;
        private readonly ResourceManager _loader = new ResourceManager(typeof(InstallStrings));

        public string? InstallFormat => _loader.GetString("InstallFormat");
        public string? VersionFormat => _loader.GetString("VersionFormat");
        public string? PackageNameFormat => _loader.GetString("PackageNameFormat");

        private readonly bool AutoGetNetAPK = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.AutoGetNetAPK) : Settings.Default.AutoGetNetAPK;

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

        private string _ADBPath = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<string>(SettingsHelper.ADBPath) : Settings.Default.ADBPath;
        public string ADBPath
        {
            get => _ADBPath;
            set
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    SettingsHelper.Set(SettingsHelper.ADBPath, value);
                    _ADBPath = SettingsHelper.Get<string>(SettingsHelper.ADBPath);
                }
                else
                {
                    Settings.Default.ADBPath = value;
                    Settings.Default.Save();
                    _ADBPath = Settings.Default.ADBPath;
                }
                RaisePropertyChangedEvent();
            }
        }

        private bool _isOpenApp = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<bool>(SettingsHelper.IsOpenApp) : Settings.Default.IsOpenApp;
        public bool IsOpenApp
        {
            get => _isOpenApp;
            set
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    SettingsHelper.Set(SettingsHelper.ADBPath, value);
                    _ADBPath = SettingsHelper.Get<string>(SettingsHelper.ADBPath);
                }
                else
                {
                    Settings.Default.IsOpenApp = value;
                    Settings.Default.Save();
                    _isOpenApp = Settings.Default.IsOpenApp;
                }
                RaisePropertyChangedEvent();
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
            if (!Directory.Exists(ADBTemp[..ADBTemp.LastIndexOf(@"\")]))
            {
                _ = Directory.CreateDirectory(ADBTemp[..ADBTemp.LastIndexOf(@"\")]);
            }
            else if (Directory.Exists(ADBTemp))
            {
                Directory.Delete(ADBTemp, true);
            }
            using Downloader downloader = new Downloader(new DownloaderOptions()
            {
                Uri = new Uri("https://dl.google.com/android/repository/platform-tools-latest-windows.zip?hl=zh-cn"),
                Stream = File.OpenWrite(ADBTemp)
            });
        downloadadb:
            _ = downloader.Start();
            WaitProgressText = _loader.GetString("WaitDownload");

            while (downloader.TotalSize <= 0 && downloader.IsStarted)
            {
                await Task.Delay(1);
            }
            WaitProgressIndeterminate = false;
            ProgressHelper.SetState(ProgressState.Normal, true);
            while (downloader.IsStarted)
            {
                WaitProgressText = $"{(double)downloader.CurrentSize * 100 / downloader.TotalSize:N2}%\n{((double)downloader.BytesPerSecond).GetSizeString()}/s";
                ProgressHelper.SetValue(Convert.ToInt32(downloader.CurrentSize), Convert.ToInt32(downloader.TotalSize), true);
                WaitProgressValue = (double)downloader.CurrentSize * 100 / downloader.TotalSize;
                await Task.Delay(1);
            }
            if (downloader.State != DownloadState.Finished)
            {
                ProgressHelper.SetState(ProgressState.Error, true);
                ContentDialog dialog = new ContentDialog
                {
                    Title = _loader.GetString("DownloadFailed"),
                    PrimaryButtonText = _loader.GetString("Retry"),
                    CloseButtonText = _loader.GetString("Cancel"),
                    Content = new TextBlock { Text = _loader.GetString("DownloadFailedInfo") },
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
            WaitProgressValue = 0;
            WaitProgressIndeterminate = true;
            ProgressHelper.SetState(ProgressState.Indeterminate, true);
            WaitProgressText = _loader.GetString("UnzipADB");
            await Task.Delay(1);
            IArchive archive = ArchiveFactory.Open(ADBTemp);
            string LocalData = PackagedAppHelper.IsPackagedApp ? ApplicationData.Current.LocalFolder.Path : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "APKInstaller");
            if (!Directory.Exists(LocalData))
            {
                _ = Directory.CreateDirectory(LocalData);
            }
            ProgressHelper.SetState(ProgressState.Normal, true);
            WaitProgressIndeterminate = false;
            foreach (IArchiveEntry entry in archive.Entries)
            {
                WaitProgressValue = archive.Entries.GetProgressValue(entry);
                ProgressHelper.SetValue(archive.Entries.ToList().IndexOf(entry) + 1, archive.Entries.Count(), true);
                WaitProgressText = string.Format(_loader.GetString("UnzippingFormat") ?? string.Empty, archive.Entries.ToList().IndexOf(entry) + 1, archive.Entries.Count());
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(LocalData, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                }
                await Task.Delay(1);
            }
            WaitProgressValue = 0;
            WaitProgressIndeterminate = true;
            ProgressHelper.SetState(ProgressState.Indeterminate, true);
            WaitProgressText = _loader.GetString("UnzipComplete");
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
                    await CheckADB();
                    WaitProgressText = _loader.GetString("StartingADB");
                    Process[] processes = Process.GetProcessesByName("adb");
                    if (processes != null && processes.Length > 1)
                    {
                        foreach (Process process in processes)
                        {
                            process.Kill();
                        }
                    }
                    if (processes != null && processes.Length == 1)
                    {
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
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    _ = AddressHelper.ConnectHyperV();
                }
                else if (IsOnlyWSA)
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
                                    _ = await Launcher.LaunchUriAsync(new Uri("wsa://"));
                                    bool IsWSARunning = false;
                                    while (!IsWSARunning)
                                    {
                                        await Task.Run(() =>
                                        {
                                            Process[] ps = Process.GetProcessesByName("vmmemWSA");
                                            IsWSARunning = ps != null && ps.Length > 0;
                                        });
                                    }
                                    WaitProgressText = _loader.GetString("WaitingADBStart");
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
                                    goto checkdevice;
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

        public void CheckAPK()
        {
            ResetUI();
            AdvancedAdbClient client = new AdvancedAdbClient();
            if (_device == null)
            {
                ActionButtonEnable = false;
                ActionButtonText = _loader.GetString("Install");
                InfoMessage = _loader.GetString("WaitingDevice");
                ActionVisibility = MessagesToUserVisibility = Visibility.Visible;
                AppName = string.Format(_loader.GetString("WaitingForInstallFormat") ?? string.Empty, ApkInfo?.AppName);
                return;
            }
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
                if (!Directory.Exists(APKTemp[..APKTemp.LastIndexOf(@"\")]))
                {
                    Directory.CreateDirectory(APKTemp[..APKTemp.LastIndexOf(@"\")]);
                }
                else if (Directory.Exists(APKTemp))
                {
                    Directory.Delete(APKTemp, true);
                }
                using Downloader downloader = new Downloader(new DownloaderOptions()
                {
                    Uri = _url,
                    Stream = File.OpenWrite(APKTemp)
                });
            downloadapk:
                _ = downloader.Start();
                ProgressText = _loader.GetString("WaitDownload");

                while (downloader.TotalSize <= 0 && downloader.IsStarted)
                {
                    await Task.Delay(1);
                }
                AppxInstallBarIndeterminate = false;
                ProgressHelper.SetState(ProgressState.Normal, true);
                while (downloader.IsStarted)
                {
                    ProgressText = $"{(double)downloader.CurrentSize * 100 / downloader.TotalSize:N2}% {((double)downloader.BytesPerSecond).GetSizeString()}/s";
                    ProgressHelper.SetValue(Convert.ToInt32(downloader.CurrentSize), Convert.ToInt32(downloader.TotalSize), true);
                    AppxInstallBarValue = (double)downloader.CurrentSize * 100 / downloader.TotalSize;
                    await Task.Delay(1);
                }
                ProgressHelper.SetState(ProgressState.Indeterminate, true);
                AppxInstallBarIndeterminate = true;
                AppxInstallBarValue = 0;
                ProgressText = _loader.GetString("Loading");
                if (downloader.State != DownloadState.Finished)
                {
                    ProgressHelper.SetState(ProgressState.Error, true);
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = _loader.GetString("DownloadFailed"),
                        PrimaryButtonText = _loader.GetString("Retry"),
                        CloseButtonText = _loader.GetString("Cancel"),
                        Content = new TextBlock { Text = _loader.GetString("DownloadFailedInfo") },
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
                    string DefaultDevice = PackagedAppHelper.IsPackagedApp ? SettingsHelper.Get<string>(SettingsHelper.DefaultDevice) : Settings.Default.DefaultDevice;
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
                    new AdvancedAdbClient().Install(_device, File.Open(_path, FileMode.Open, FileAccess.Read));
                });
                if (IsOpenApp)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);// 据说如果安装完直接启动会崩溃。。。
                        OpenAPP();
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
