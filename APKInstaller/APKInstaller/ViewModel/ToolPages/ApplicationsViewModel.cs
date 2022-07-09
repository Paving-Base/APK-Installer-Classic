using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Pages.ToolPages;
using IWshRuntimeLibrary;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using File = System.IO.File;
using TitleBar = APKInstaller.Controls.TitleBar;

namespace APKInstaller.ViewModel.ToolPages
{
    internal class ApplicationsViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ApplicationsPage _page;
        private Dictionary<string, (string Name, BitmapImage Icon)> PackageInfos;

        private List<string> deviceList = new List<string>();
        public List<string> DeviceList
        {
            get => deviceList;
            set
            {
                deviceList = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<APKInfo> applications;
        public List<APKInfo> Applications
        {
            get => applications;
            set
            {
                applications = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ApplicationsViewModel(ApplicationsPage page)
        {
            _page = page;
        }

        private async Task GetInfos()
        {
            await Task.Run(() =>
            {
                string ProgramFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
                string[] InkInfos = Directory.GetFiles(ProgramFolder, "*.lnk");
                PackageInfos = new Dictionary<string, (string Name, BitmapImage Icon)>();
                foreach (string file in InkInfos)
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    WshShell shell = new();
                    WshShortcut shortcut = shell.CreateShortcut(file);
                    string args = shortcut.Arguments;
                    string icon = shortcut.IconLocation.Replace(",0", string.Empty);
                    string path = shortcut.TargetPath;
                    if (Path.GetFileNameWithoutExtension(path) == "WsaClient")
                    {
                        string pic = Path.ChangeExtension(icon, "png");
                        if (File.Exists(pic)) { icon = pic; }
                        Uri imageuri = new(pic);
                        BitmapImage image = null;
                        _page?.RunOnUIThread(() => { image = new BitmapImage(imageuri); });
                        PackageInfos.Add(args.Replace("/launch wsa://", string.Empty), (Path.GetFileNameWithoutExtension(file), image));
                    }
                }
            });
        }

        public async Task GetDevices()
        {
            await Task.Run(async () =>
            {
                _page?.RunOnUIThread(TitleBar.ShowProgressRing);
                devices = new AdvancedAdbClient().GetDevices();
                await _page?.ExecuteOnUIThreadAsync(DeviceList.Clear);
                if (devices.Count > 0)
                {
                    foreach (DeviceData device in devices)
                    {
                        if (!string.IsNullOrEmpty(device.Name))
                        {
                            await _page?.ExecuteOnUIThreadAsync(() => DeviceList.Add(device.Name));
                        }
                        else if (!string.IsNullOrEmpty(device.Model))
                        {
                            await _page?.ExecuteOnUIThreadAsync(() => DeviceList.Add(device.Model));
                        }
                        else if (!string.IsNullOrEmpty(device.Product))
                        {
                            await _page?.ExecuteOnUIThreadAsync(() => DeviceList.Add(device.Product));
                        }
                        else if (!string.IsNullOrEmpty(device.Serial))
                        {
                            await _page?.ExecuteOnUIThreadAsync(() => DeviceList.Add(device.Serial));
                        }
                        else
                        {
                            await _page?.ExecuteOnUIThreadAsync(() => DeviceList.Add("Device"));
                        }
                    }
                    await _page?.ExecuteOnUIThreadAsync(() =>
                    {
                        DeviceComboBox.ItemsSource = DeviceList;
                        if (DeviceComboBox.SelectedIndex == -1)
                        {
                            DeviceComboBox.SelectedIndex = 0;
                        }
                    });
                }
                else if (Applications != null)
                {
                    await _page?.ExecuteOnUIThreadAsync(() => Applications = null);
                }
                _page?.RunOnUIThread(TitleBar.HideProgressRing);
            });
        }

        public async Task<List<APKInfo>> CheckAPP(Dictionary<string, string> apps, int index)
        {
            List<APKInfo> Applications = new();
            await Task.Run(async () =>
            {
                AdvancedAdbClient client = new();
                PackageManager manager = new(client, devices[index]);
                if (PackageInfos == null) { await GetInfos(); }
                foreach (KeyValuePair<string, string> app in apps)
                {
                    _page?.RunOnUIThread(() => TitleBar.SetProgressValue((double)apps.ToList().IndexOf(app) * 100 / apps.Count));
                    if (!string.IsNullOrEmpty(app.Key))
                    {
                        ConsoleOutputReceiver receiver = new();
                        client.ExecuteRemoteCommand($"pidof {app.Key}", devices[index], receiver);
                        bool isactive = !string.IsNullOrEmpty(receiver.ToString());
                        if (PackageInfos.ContainsKey(app.Key))
                        {
                            (string Name, BitmapImage Icon) = PackageInfos[app.Key];
                            ImageIcon source = await _page?.ExecuteOnUIThreadAsync(() => { return new ImageIcon { Source = Icon, Width = 20, Height = 20 }; });
                            Applications.Add(new APKInfo
                            {
                                Name = Name,
                                Icon = source,
                                PackageName = app.Key,
                                IsActive = isactive,
                                VersionInfo = manager.GetVersionInfo(app.Key),
                            });
                        }
                        else
                        {
                            FontIcon source = await _page?.ExecuteOnUIThreadAsync(() => { return new FontIcon { Glyph = "\xECAA" }; });
                            Applications.Add(new APKInfo
                            {
                                Name = app.Key,
                                Icon = source,
                                IsActive = isactive,
                                VersionInfo = manager.GetVersionInfo(app.Key),
                            });
                        }
                    }
                    break;
                }
            });
            return Applications;
        }

        public async Task GetApps()
        {
            await Task.Run(async () =>
            {
                _page?.RunOnUIThread(TitleBar.ShowProgressRing);
                AdvancedAdbClient client = new();
                int index = await _page?.ExecuteOnUIThreadAsync(() => { return DeviceComboBox.SelectedIndex; });
                PackageManager manager = new(new AdvancedAdbClient(), devices[index]);
                List<APKInfo> list = await CheckAPP(manager.Packages, index);
                _page?.RunOnUIThread(() => Applications = list);
                _page?.RunOnUIThread(TitleBar.HideProgressRing);
            });
        }
    }

    internal class ApplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "State": return (bool)value ? "Running" : "Stop";
                default: return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    internal class APKInfo
    {
        public string Name { get; set; }
        public IconElement Icon { get; set; }
        public string PackageName { get; set; }
        public bool IsActive { get; set; }
        public VersionInfo VersionInfo { get; set; }
    }
}
