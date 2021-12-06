using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_Auto_Update
{
    public class Updater
    { 
        public IReadOnlyList<Release> Releases { get; set; }
        public static Duration UpdateTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public Window CurrentApp { get; set; }

        public async void CheckForUpdates()
        {
            try
            {
                var progressControl = new MainWindow();
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (sender, args) => {
                    progressControl.progressBar.Value = args.ProgressPercentage;
                };

                progressControl.Show();
                CurrentApp.Close(); 
                var assets = Releases[0].Assets;
                var path_dir = ".";
                if (!Directory.Exists(path_dir)) Directory.CreateDirectory(path_dir);
                var i = 1;
                foreach (var asset in assets)
                {
                    progressControl.progressDownload.Text = "Téléchargement " + i + "/" + assets.Count;
                    await webClient.DownloadFileTaskAsync(new Uri(asset.BrowserDownloadUrl), path_dir + asset.Name);
                    progressControl.progressFiles.Content = asset.Name;
                    i++;

                }
                progressControl.progressDownload.Text = "Mise à jour terminée !";
                progressControl.progressFiles.Content = "";
                progressControl.Close();
            }
            catch (Exception)
            {
              
                throw;
            }
        }
    }
}
