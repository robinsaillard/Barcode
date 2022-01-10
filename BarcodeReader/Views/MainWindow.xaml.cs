using BarcodeReader.Services;
using BarcodeReader.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;


namespace BarcodeReader.Views
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckUpdate();
        }

        private void CheckUpdate()
        {
            Version localVersion = ApplicationInfo.CurrentVersion;
            ProcessStartInfo processStartInfo = new ProcessStartInfo("AutoUpdater.exe")
            {
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = "name=" + ApplicationInfo.AppName + " version=" + localVersion
            };
            _ = Process.Start(processStartInfo);
        }
    }
}
