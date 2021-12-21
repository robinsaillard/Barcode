using System;
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
        private IWebDriver driver;
        private ChromeOptions chromeOptions;

        public WebDriver()
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

                    this.chromeOptions = new ChromeOptions();

                    //chromeOptions.AddAdditionalChromeOption("excludeSwitches", new List<string>() { "enable-logging" });
                    this.chromeOptions.AddUserProfilePreference("download.default_directory", @"C:\Users\dev\Documents");
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
                    if (File.Exists(path)) ZipFile.ExtractToDirectory(path, path_dir);
                    if (File.Exists(path_dir + chromedriver)) File.Delete(path);

                }
                catch (Exception)
                {
                    if (!Directory.Exists(path_dir)) Directory.CreateDirectory(path_dir);
                }
            }
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
                catch (Exception)
                {
                    throw;
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
            this.driver.Navigate().GoToUrl(url);
        }
    }
}
