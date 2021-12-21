using System;
using System.Diagnostics;
using System.Reflection;

namespace BarcodeReader.Services
{
    public static class ApplicationInfo
    {
        public const string AppName = "Barcode";

        public static readonly Version CurrentVersion;

        public static readonly string AppNameVersion = string.Empty;

        static ApplicationInfo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var info = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = info.FileVersion;
            CurrentVersion = new Version(version);
            AppNameVersion = AppName + " v" + version;
        }
    }
}
