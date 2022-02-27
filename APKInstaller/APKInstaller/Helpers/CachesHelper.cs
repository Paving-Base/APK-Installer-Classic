using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKInstaller.Helpers
{
    public static class CachesHelper
    {
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), @"APKInstaller\Caches", $"{Process.GetCurrentProcess().Id}");
    
        public static void CleanCaches(bool isall)
        {
            if (isall)
            {
                if (Directory.Exists(TempPath.Substring(0, TempPath.LastIndexOf(@"\"))))
                {
                    try { Directory.Delete(TempPath.Substring(0, TempPath.LastIndexOf(@"\")), true); } catch { }
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
