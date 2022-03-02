using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace APKInstaller.Helpers
{
    public static class CachesHelper
    {
#if NET5_0_OR_GREATER && !NETCOREAPP
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), @"APKInstaller\Caches", $"{Environment.ProcessId}");
#else
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), @"APKInstaller\Caches", $"{Process.GetCurrentProcess().Id}");
#endif

        public static void CleanCaches(bool isall)
        {
            if (isall)
            {
                if (Directory.Exists(TempPath[..TempPath.LastIndexOf(@"\")]))
                {
                    try { Directory.Delete(TempPath[..TempPath.LastIndexOf(@"\")], true); } catch { }
                }
            }   
            else
            {
                if (Directory.Exists(TempPath))
                {
                    try { Directory.Delete(TempPath, true); } catch { }
                }
            }
        }

        public static void CleanAllCaches(bool isall)
        {
            CleanCaches(isall);
        }
    }
}
