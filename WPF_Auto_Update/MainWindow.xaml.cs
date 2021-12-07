using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Auto_Update
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _ = UpdateAsync(); 
        }

        public async Task UpdateAsync()
        {
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("robinsaillard"));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("robinsaillard", "Barcode");
                //var progressControl = new MainWindow();
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (sender, argus) => {
                    progressBar.Value = argus.ProgressPercentage;
                };

                //progressControl.Show();
                //CurrentApp.Close(); 
                Process[] cmd = Process.GetProcessesByName("cmd");
                Process[] chromeDriver = Process.GetProcessesByName("Barcode");

                Process[] workers = chromeDriver.Concat(cmd).ToArray();

                foreach (Process worker in workers)
                {
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }
                var assets = releases[0].Assets;
                var path_dir = "./";
                if (!Directory.Exists(path_dir)) Directory.CreateDirectory(path_dir);
                var i = 1;
                foreach (var asset in assets)
                {
                    progressDownload.Text = "Téléchargement " + i + "/" + assets.Count;
                    progressFiles.Content = asset.Name;
                    FileInfo file = new FileInfo(asset.Name);
                    try
                    {
                        Thread.Sleep(2000);
                        file.Delete();
                        await webClient.DownloadFileTaskAsync(new Uri(asset.BrowserDownloadUrl), path_dir + asset.Name);
                        
                    }
                    catch (Exception ex) 
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(asset.BrowserDownloadUrl), path_dir + asset.Name);
                    }
                    i++;
                   

                }
                progressDownload.Text = "Mise à jour terminée !";
                progressFiles.Content = "";
                //Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
