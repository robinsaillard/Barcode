using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

        public WebDriver()
        {
            IWebDriver driver = new ChromeDriver(DRIVER_DIR);

            this.version_chrome_driver = CheckVersionDriver(driver);
            this.version_chrome = GetChromeVersion();
            if(this.version_chrome_driver != this.version_chrome)
            {
               // DownloadDriver();
            }
        }

        public void GetDriver()
        {
            var retry = true;

            return;
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
            var version = GetChromeVersion();
            var path = "./chromedriver/"; 
            if(version != null)
            {
                WebClient myWebClient = new WebClient();
                var link = base_driver_url + version + file_name;
                myWebClient.DownloadFile(link, path);
            }

        }


        public string GetChromeVersion()
        {

           object path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null);
           if(path != null)
           {
                var chrome_version = FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion;
                return chrome_version;
           }

            return "Version"; 
        }
    }
}
