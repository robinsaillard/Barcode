using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Barcode
{
    class WebDriver
    {
        private static string DRIVER_DIR = "./chromedriver/";
        private string version_chrome_driver;
        private string version_chrome; 
        private IWebDriver driver;
        private ChromeOptions chromeOptions;

        public WebDriver()
        {
            this.version_chrome = GetChromeVersion();
            if(this.version_chrome != null)
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
            while(retry && retryValue > 0)
            {
                retry = false; 
                
                try
                {
                    var driverService = ChromeDriverService.CreateDefaultService(DRIVER_DIR);
                    driverService.HideCommandPromptWindow = true;

                    var chromeOptions = new ChromeOptions();
                    
                    //chromeOptions.AddAdditionalChromeOption("excludeSwitches", new List<string>() { "enable-logging" });
                    chromeOptions.AddUserProfilePreference("download.default_directory", @"C:\Users\dev\Documents");
                    this.driver = new ChromeDriver(driverService, chromeOptions);
                    var windowWidth = SystemParameters.PrimaryScreenWidth;
                    this.driver.Manage().Window.Position = new System.Drawing.Point(Convert.ToInt32(windowWidth /2), 1);
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
            return (string) capabilities.GetCapability("browserVersion");
        }

        public string GetVersionDriver()
        {
            return this.version_chrome_driver;
        }

        public void DownloadDriver()
        {
            var base_driver_url = "https://chromedriver.storage.googleapis.com/";
            var file_name = "/chromedriver_win32.zip";
            var version = this.version_chrome;
            var path_dir = "./chromedriver";
            var path = path_dir + file_name ;
            var chromedriver = "/chromedriver.exe";
            if(version != null)
            {
                WebClient myWebClient = new WebClient();
                var link = base_driver_url + version + file_name;
                try
                {
                    myWebClient.DownloadFile(link, path);
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
            if(chrome_version != null)
            {
                return chrome_version;
            }

            return null; 
        }

        public void CloseGhostsChromeDriver()
        {
            try
            {
                this.driver.Close();
            }
            catch (Exception)
            {
                throw;
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
    }
}
