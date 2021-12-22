using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using Application = System.Windows.Application;
using System.Windows.Forms;

namespace AutoUpdater
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Version LocalVersion { get; set; }
        public string AppName { get; set; }

        public MainWindow()
        {
            this.Hide();
            InitializeComponent();

            if (Application.Current.Resources["version"] != null && Application.Current.Resources["name"] != null)
            {
                LocalVersion = new Version((string)Application.Current.Resources["version"]);
                AppName = (string)Application.Current.Resources["name"];
                Task task = UpdateAsync();
            }
            else
            {
                this.Close();
            }
        }

        public async Task UpdateAsync()
        {
            if(LocalVersion != null && AppName != null)
            {
                try
                {
                    GitHubClient client = new GitHubClient(new ProductHeaderValue("robinsaillard"));
                    IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("robinsaillard", "Barcode");

                    Version latestGitHubVersion = new Version(releases[0].TagName.Trim(new Char[] { ' ', 'v', 'V' }));

                    int versionComparison = LocalVersion.CompareTo(latestGitHubVersion);
                    if (versionComparison < 0)
                    {
                        //The version on GitHub is more up to date than this local release.
                        string message = string.Format("Souhaitez vous mettre à jour {0} ? \n Version actuelle : {1} \n Nouvelle version : {2}",
                            AppName,
                            LocalVersion.ToString(),
                            latestGitHubVersion
                            );

                        string caption = AppName + " : Une mise à jour est disponible";
                        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                        DialogResult result;

                        // Displays the MessageBox.
                        result = MessageBox.Show(message, caption, buttons);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            this.Show();
                            WebClient webClient = new WebClient();
                            webClient.DownloadProgressChanged += (sender, argus) => {
                                progressBar.Value = argus.ProgressPercentage;
                            };

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
                                    Thread.Sleep(1000);
                                    file.Delete();
                                    await webClient.DownloadFileTaskAsync(new Uri(asset.BrowserDownloadUrl), path_dir + asset.Name);

                                }
                                catch (Exception)
                                {
                                    progressDownload.Text = "Impossible de remplacer le fichier " + asset.Name;
                                }
                                i++;
                            }
                            progressDownload.Text = "Mise à jour terminée !";
                            progressFiles.Content = "";
                            Thread.Sleep(1000);
                            this.Close();
                            Process.Start(AppName + ".exe");
                        }
                    }
                    else
                    {
                        this.Close();
                    }

                }
                catch (Exception)
                {

                    throw;
                }
            }else
            {
                this.Close();
            }
           
        }
    }
}
