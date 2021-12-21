using BarcodeReader.Services;
using BarcodeReader.ViewModels;
using System;
using System.Diagnostics;
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
            ProcessStartInfo updater = new ProcessStartInfo("WPF_Auto_Update.exe");
            updater.WindowStyle = ProcessWindowStyle.Normal;
            updater.Arguments = "name=" + ApplicationInfo.AppName + " version=" + localVersion;
            Process.Start(updater);
        }
    }
}
