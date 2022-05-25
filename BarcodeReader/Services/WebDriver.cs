using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BarcodeReader.Services
{
    public class WebDriver
    {
        private static readonly string DRIVER_DIR = "./chromedriver/";
        private readonly string version_chrome;
        public IWebDriver driver;
        private ChromeOptions chromeOptions;
        public string Directory { get; set; }

        public WebDriver(string directory)
        {
            Directory = directory;
            if (driver == null) 
            {
                this.version_chrome = GetChromeVersion();
                if (this.version_chrome != null)
                {
                    GetDriver();
                }
                else
                {
                    new Exception("Impossible de trouver la version de chrome");
                }
            }
        }

        public void GetDriver()
        {
            var retry = true;
            var retryValue = 2;
            while (retry && retryValue > 0)
            {
                retry = false;

                try
                {
                    var driverService = ChromeDriverService.CreateDefaultService(DRIVER_DIR);
                    driverService.HideCommandPromptWindow = true;

                    CreateChromeOptions();

                    this.driver = new ChromeDriver(driverService, this.chromeOptions);
                    var windowWidth = SystemParameters.PrimaryScreenWidth;
                    this.driver.Manage().Window.Position = new System.Drawing.Point(Convert.ToInt32(windowWidth / 2), 1);
                }
                catch (Exception)
                {
                    DownloadDriver();
                    retry = true;
                }
                retryValue--;
            }

        }

        public static string CheckVersionDriver(IWebDriver driver)
        {
            ICapabilities capabilities = ((ChromeDriver)driver).Capabilities;
            return (string)capabilities.GetCapability("browserVersion");
        }


        public void DownloadDriver()
        {
            string base_driver_url = "https://chromedriver.storage.googleapis.com/";
            string file_name = "/chromedriver_win32.zip";
            string version = this.version_chrome;
            string path_dir = "./chromedriver";
            string path = path_dir + file_name;
            string chromedriver = "/chromedriver.exe";

            if (version != null)
            {
                WebClient client = new WebClient();
                string[] split_version = version.Split(".".ToCharArray());
                string link_last_version = base_driver_url + "LATEST_RELEASE_" + split_version[0];
                string last_version = client.DownloadString(link_last_version);
                string link = base_driver_url + last_version + file_name;
                try
                {
                  
                    client.DownloadFile(link, path);
                    
                    if (File.Exists(path_dir + chromedriver)) {
                        var chromeDriverProcesses = Process.GetProcesses().
                        Where(pr => pr.ProcessName == "chromedriver"); // without '.exe'

                        foreach (var process in chromeDriverProcesses)
                        {
                            process.Kill();
                        }
                        File.SetAttributes(path_dir + chromedriver, FileAttributes.Normal);
                        File.Delete(path_dir + chromedriver);
                        var stop = "stop";
                    }
                    if (File.Exists(path)) ZipFile.ExtractToDirectory(path, path_dir);

                }
                catch (Exception e)
                {
                    if (!System.IO.Directory.Exists(path_dir)) System.IO.Directory.CreateDirectory(path_dir);

                    var test = e.Message;
                    var coucou = "test";
                }
            }
        }

        public IWebDriver GetCurrentDriver()
        {
            return driver;
        }

        public string GetChromeVersion()
        {
            RegistryKey localKey;
            if (Environment.Is64BitOperatingSystem)
                localKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            else
                localKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

            string chrome_version = localKey.OpenSubKey("SOFTWARE").OpenSubKey("Google").OpenSubKey("Chrome").OpenSubKey("BLBeacon").GetValue("version").ToString();
            return chrome_version ?? null;
        }

        public void CloseGhostsChromeDriver()
        {

            if (this.driver != null)
            {
                try
                {
                    this.driver.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur :" + ex.Message);
                }
            }
           
            Process[] cmd = Process.GetProcessesByName("cmd");
            Process[] chromeDriver = Process.GetProcessesByName("chromedriver");

            Process[] workers = chromeDriver.Concat(cmd).ToArray();

            foreach (Process worker in workers)
            {
                worker.Kill();
                worker.WaitForExit();
                worker.Dispose();
            }
        }


        public void OpenLink(string url)
        {
            try
            {
                this.driver.Navigate().GoToUrl(url);
            }
            catch (Exception)
            {
                if(this.driver == null) GetDriver();
                MessageBox.Show(url);
               
            }         
        }

        public void CreateChromeOptions()
        {
            this.chromeOptions = new ChromeOptions();

            List<string> ls = new List<string>
            {
                "enable-logging"
            };
            this.chromeOptions.AddExcludedArguments(ls);
            this.chromeOptions.AddArgument("--ignore-certificate-errors");
            this.chromeOptions.AddArgument("--disable-popup-blocking");
            this.chromeOptions.AddArguments("--no-sandbox");
            this.chromeOptions.AddUserProfilePreference("download.default_directory", Directory);
            this.chromeOptions.AddUserProfilePreference("download.download_restrictions", 0);
            this.chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            this.chromeOptions.AddUserProfilePreference("download.directory_upgrade", true);
            this.chromeOptions.AddUserProfilePreference("safebrowsing_for_trusted_sources_enabled", true);
            this.chromeOptions.AddUserProfilePreference("safebrowsing.enabled", true);
            this.chromeOptions.AddUserProfilePreference("download.open_pdf_in_system_reader", false);
            this.chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

            this.chromeOptions.AddExtension(@"./Ressources/extension.crx");
        }
    }
}
