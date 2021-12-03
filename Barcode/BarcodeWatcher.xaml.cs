
using Barcode.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Djlastnight.Hid;
using Djlastnight.Input;
using Brushes = System.Windows.Media.Brushes;
using System.Diagnostics;
using Octokit;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Barcode
{
    /// <summary>
    /// Logique d'interaction pour BarcodeWatcher.xaml
    /// </summary>
    public partial class BarcodeWatcher : Window
    {
        private string green = "#32CD32";
        private string red = "#FF0000";
        private HidDataReader reader;
        private IIinputDevice device = null;
        private string log = "";
        private int i = 0;
        private KeyConvertor keyconvertor = new KeyConvertor();
        private WebDriver driver; 

        public BarcodeWatcher()
        {
            InitializeComponent();
            this.rtb.IsDocumentEnabled = true;
            this.rtb.Document.Blocks.FirstBlock.Margin = new Thickness(0);

        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            CheckUpdate();
            this.Title = ApplicationInfo.AppNameVersion;

        }

        private void OnStartScan(object sender, RoutedEventArgs e)
        {
            UpdateStatutScanCode();
            if(this.device != null)
            {
                this.driver = new WebDriver();
                var version = driver.GetChromeVersion();
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(string.Format("============ Démarrage {0} {1} ============ ", ApplicationInfo.AppNameVersion, version)));
                this.rtb.Document.Blocks.Add(paragraph);
                this.btnStart.Content = "Démarrer";
                this.reader = new HidDataReader(this);
                this.reader.HidDataReceived += this.OnHidDataReceived;
            }
            else
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(string.Format("Erreur : Verifiez les branchements USB de la zapette/douchette")));
                this.rtb.Document.Blocks.Add(paragraph);
            }
            
        }

        private void OnStopScan(object sender, RoutedEventArgs e)
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(string.Format("============    Stop  {0}    ============ ", ApplicationInfo.AppNameVersion)));
            this.rtb.Document.Blocks.Add(paragraph);
            this.btnStop.IsEnabled = false;
            this.btnStart.IsEnabled = true;
            this.deviceId.Text = null;
            this.driver.CloseGhostsChromeDriver();


        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnHidDataReceived(object sender, HidEvent e)
        {

            try
            {
                if (this.btnStart.IsEnabled)
                {
                    return;
                }

                if (e.Device == null)
                {
                    return;
                }
                var senderVid = e.Device.VendorId.ToString("X4");
                var senderPid = e.Device.ProductId.ToString("X4");
                if (this.device.Vendor != senderVid || this.device.Product != senderPid)
                {
                    return;
                }

                if (this.device is HidKeyboard && !e.IsButtonDown && e.RawInput.keyboard.MakeCode != 0)
                {
                    this.Activate();
                    var dict = keyconvertor.getDictionary();
                    if (this.i == 0 && e.RawInput.keyboard.MakeCode == 42)
                    {
                        this.i = 1;
                        return;
                    }
                    if(this.i >= 0 && e.RawInput.keyboard.MakeCode == 56)
                    {
                        this.i = 2;
                        return;
                    }

                    //this.log += ": " + e.RawInput.keyboard.MakeCode;
                    //this.log += Convert.ToChar(e.RawInput.keyboard.VKey + val);
                    this.log += dict[e.RawInput.keyboard.MakeCode][this.i];
                    this.i = 0; 
                    if (e.RawInput.keyboard.VKey == 13)
                    {
                        var web = "https://";
                        AddHyperlinkText(web + this.log, this.log, "", "");
                        this.log = "";
                    }

  
                }
                
            }
            catch (Exception ex)
            {
                this.rtb.Document.Blocks.Add(new Paragraph(new Run("Error: " + ex.Message) { Foreground = Brushes.Red, Background = Brushes.Black }));
            }
        }

        private void AddHyperlinkText(string linkURL, string linkName,
                  string TextBeforeLink, string TextAfterLink)
        {
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0); // remove indent between paragraphs

            Hyperlink link = new Hyperlink();
            link.IsEnabled = true;
            link.Inlines.Add(linkName);
            link.NavigateUri = new Uri(linkURL);
            link.RequestNavigate += (sender, args) => Process.Start(args.Uri.ToString());

            para.Inlines.Add(new Run("[" + DateTime.Now.ToLongTimeString() + "]: "));
            para.Inlines.Add(TextBeforeLink);
            para.Inlines.Add(link);
            para.Inlines.Add(new Run(TextAfterLink));

            this.rtb.Document.Blocks.Add(para);
            this.scrollviewer.ScrollToEnd();
        }


        private void UpdateStatutScanCode()
        {
            this.device = null;
            var converter = new BrushConverter();
            var devices = new List<IIinputDevice>();
            devices.Add(VirtualKeyboard.Instance);
            devices.AddRange(DeviceScanner.GetHidKeyboards());
            var devicesToRemove = new List<IIinputDevice>();
            foreach (var device in devices)
            {
                if (device.DeviceType != DeviceType.Other)
                {
                    continue;
                }
                ushort vid = Convert.ToUInt16(device.Vendor, 16);
                ushort pid = Convert.ToUInt16(device.Product, 16);
                var rawDevices = HidDataReader.GetDevices().Where(d => d.ProductId == pid && d.VendorId == vid);

                if (rawDevices.Count() == 0)
                {
                    devicesToRemove.Add(device);
                }
                var type = device.DeviceType;
               
            }
            foreach (var deviceToRemove in devicesToRemove)
            {
                devices.Remove(deviceToRemove);
            }
            foreach (var device in devices)
            {
                if (device.Vendor == "0581" && device.Product == "011A" && device.DeviceType == DeviceType.Keyboard)
                {
                    this.device = device;
                    var color_green = (Brush)converter.ConvertFromString(green);
                    this.statutScanner.Background = color_green;
                    this.statutScanner.Content = "On";
                    this.btnStart.IsEnabled = false;
                    this.btnStop.IsEnabled = true;
                    this.deviceId.Text = device.DeviceID;
                }
            }
            if (this.device == null)
            {
                var color_red = (Brush)converter.ConvertFromString(red);
                this.statutScanner.Background = color_red;
                this.statutScanner.Content = "Off";
                this.btnStart.IsEnabled = true;
                this.deviceId.Text = null;
                this.btnStop.IsEnabled = false;
                this.btnStart.Content = "Réessayez";
                this.deviceId.Text = null;

            }
        }

        private async void CheckUpdate()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("robinsaillard"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("robinsaillard", "Barcode");

            //Setup the versions
            Version latestGitHubVersion = new Version(releases[0].TagName.Trim(new Char[] { ' ', 'v' }));
            Version localVersion = ApplicationInfo.CurrentVersion; 

            int versionComparison = localVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                //The version on GitHub is more up to date than this local release.

                string message = string.Format("Souhaitez vous mettre à jour {0} ? \n Version actuelle : {1} \n Nouvelle version : {2}",
                    ApplicationInfo.AppName, 
                    ApplicationInfo.CurrentVersion,
                    latestGitHubVersion);

                string caption = ApplicationInfo.AppName + " : Une mise à jour est disponible";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                // Displays the MessageBox.
                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // Closes the parent form.
                    this.Close();
                }

            }
        }
    }
}