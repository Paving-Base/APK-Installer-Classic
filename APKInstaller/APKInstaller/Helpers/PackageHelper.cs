using APPXManager.DeviceCommands;
using APPXManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace APKInstaller.Helpers
{
    internal static partial class PackageHelper
    {
        public static async Task<(bool isfound, IEnumerable<PackageInfo>? info)> FindPackagesByName(string PackageFamilyName)
        {
            try
            {
                (bool isfound, IEnumerable<PackageInfo> info) WSAList = await Task.Run(() => { return PackageManager.FindPackageByName(PackageFamilyName); });
                return WSAList;
            }
            catch
            {
                return (false, null);
            }
        }

        public static async void LaunchPackage(string packagefamilyname, string appname = "App") => await CommandHelper.ExecuteShellCommand($@"explorer.exe shell:appsFolder\{packagefamilyname}!{appname}");

        public static async void LaunchWSAPackage(string packagename = "")
        {
            (bool isfound, IEnumerable<PackageInfo>? info) result = await FindPackagesByName("MicrosoftCorporationII.WindowsSubsystemForAndroid_8wekyb3d8bbwe");
            if (result.isfound)
            {
                Process.Start($"wsa://{packagename}");
            }
        }
    }
}
