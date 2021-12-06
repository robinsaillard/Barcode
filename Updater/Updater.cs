using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    public static class Updater
    {
        public static string ServiceURI { get; set; }
        public static string RemoteFileURI { get; set; }
        public static IReadOnlyList<Release> Releases { get; set; }
        public static Duration UpdateTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public static async Task CheckForUpdates(bool Silent)
        {
            try
            {
                var progressControl = new MainWindow();
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (sender, args) => {
                    progressControl.progressBar.Value = args.ProgressPercentage;
                };

                progressControl.Show();
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
