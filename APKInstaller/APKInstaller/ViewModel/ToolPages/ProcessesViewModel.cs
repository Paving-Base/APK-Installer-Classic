﻿using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Controls;
using APKInstaller.Helpers;
using APKInstaller.Pages.ToolPages;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace APKInstaller.ViewModel.ToolPages
{
    public class ProcessesViewModel : INotifyPropertyChanged
    {
        public TitleBar TitleBar;
        public ComboBox DeviceComboBox;
        public List<DeviceData> devices;
        private readonly ProcessesPage _page;

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

        private IEnumerable<AndroidProcess> processes;
        public IEnumerable<AndroidProcess> Processes
        {
            get => processes;
            set
            {
                processes = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ProcessesViewModel(ProcessesPage page)
        {
            _page = page;
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
                else if (Processes != null)
                {
                    await _page?.ExecuteOnUIThreadAsync(() => Processes = null);
                }
                _page?.RunOnUIThread(TitleBar.HideProgressRing);
            });
        }

        public async Task GetProcess()
        {
            await Task.Run(async () =>
            {
                _page?.RunOnUIThread(TitleBar.ShowProgressRing);
                AdvancedAdbClient client = new();
                DeviceData device = await _page?.ExecuteOnUIThreadAsync(() => { return devices[DeviceComboBox.SelectedIndex]; });
                IEnumerable<AndroidProcess> list = DeviceExtensions.ListProcesses(client, device);
                await _page?.ExecuteOnUIThreadAsync(() => Processes = list);
                _page?.RunOnUIThread(TitleBar.HideProgressRing);
            });
        }
    }

    internal class ProcesseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "Size": return ((double)(int)value).GetSizeString();
                case "Name": return ((string)value).Split('/').Last().Split(':').First().Split('@').First();
                case "State":
                    switch ((AndroidProcessState)value)
                    {
                        case AndroidProcessState.Unknown: return "Unknown";
                        case AndroidProcessState.D: return "Sleep(D)";
                        case AndroidProcessState.R: return "Running";
                        case AndroidProcessState.S: return "Sleep(S)";
                        case AndroidProcessState.T: return "Stopped";
                        case AndroidProcessState.W: return "Paging";
                        case AndroidProcessState.X: return "Dead";
                        case AndroidProcessState.Z: return "Defunct";
                        case AndroidProcessState.K: return "Wakekill";
                        case AndroidProcessState.P: return "Parked";
                        default: return value.ToString();
                    }
                default: return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
