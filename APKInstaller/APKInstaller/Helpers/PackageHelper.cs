using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.System;

namespace APKInstaller.Helpers
{
    internal static partial class PackageHelper
    {
        public static async Task<(bool isfound, IEnumerable<Package>? info)> FindPackagesByName(string PackageFamilyName)
        {
            try
            {
                PackageManager manager = new PackageManager();
                IEnumerable<Package> WSAList = await Task.Run(() => { return manager.FindPackagesForUser("", PackageFamilyName); });
                return (WSAList != null && WSAList.Any(), WSAList);
            }
            catch
            {
                return (false, null);
            }
        }

        public static void LaunchPackage(string packagefamilyname, string appname = "App") => CommandHelper.ExecuteShellCommand($@"explorer.exe shell:appsFolder\{packagefamilyname}!{appname}");

        public static async void LaunchWSAPackage(string packagename = "")
        {
            (bool isfound, IEnumerable<Package>? info) result = await FindPackagesByName("MicrosoftCorporationII.WindowsSubsystemForAndroid_8wekyb3d8bbwe");
            if (result.isfound)
            {
#if NET5_OR_GREATER
                _ = await Launcher.LaunchUriAsync(new Uri($"wsa://{packagename}"));
#else
                _ = Process.Start($"wsa://{packagename}");
#endif
            }
        }
    }
}
