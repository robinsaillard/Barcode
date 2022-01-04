using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows;

using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using BarcodeReader.Models;
using BarcodeReader.Services;
using Djlastnight.Hid;
using Djlastnight.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace BarcodeReader.Views
{
    /// <summary>
    /// Logique d'interaction pour ScanView.xaml
    /// </summary>
    public partial class ScanView : UserControl
    {
        private readonly string green = "#32CD32";
        private readonly string red = "#e53935";
        private HidDataReader reader;
        private IIinputDevice device = null;
        private readonly KeyConvertor keyConvertor = new KeyConvertor();
        private WebDriver driver;
        private Window window;
        private int i = 0;
        private string log = "";
        Dictionary<string, Options> options;

        public ScanView()
        {
            InitializeComponent();
           
            Rtb.IsDocumentEnabled = true;

            string postName = Environment.MachineName.ToString();

            if (!DbManager.PostNameExist(postName))
            {
                DbManager.InsertPost(postName);
            }

            options = DbManager.GetOptions(postName);
            string values = options["PDF_FILENAME"].Value;
            string directory = options["DOWNLOAD_DIRECTORY"].Value;
            string ext = options["PDF_EXTENSION"].Value;
            string printer = options["PRINTER_NAME"].Value;
            string[] listFile = values.Split(';');

            DirectoryWatcher watcher = new DirectoryWatcher(directory, listFile, ext, printer);
            

        }

        private void OnStartScan(object sender, RoutedEventArgs e)
        {
            UpdateStatutScanCode();
           
            if (device != null)
            {
                var converter = new BrushConverter();
                var color = (Brush)converter.ConvertFromString("#2196f3");
                string directory = options["DOWNLOAD_DIRECTORY"].Value;
                driver = new WebDriver(directory);
                string version = driver.GetChromeVersion();
                Rtb.Document.Blocks.Add(new Paragraph(
                new Run(string.Format(
                   "========================================= Démarrage {0} ========================================== ", ApplicationInfo.AppNameVersion
                ))
                { Foreground = Brushes.White, Background = color}));
                BtnStart.Content = "Démarrer";
                window = Window.GetWindow(this);
                reader = new HidDataReader(window);
                reader.HidDataReceived += OnHidDataReceived;
            }
            else
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(string.Format("Erreur : Verifiez les branchements USB de la zapette/douchette")));
                Rtb.Document.Blocks.Add(paragraph);
            }
        }

        private void OnStopScan(object sender, RoutedEventArgs e)
        {
            var converter = new BrushConverter();
            var color = (Brush)converter.ConvertFromString("#2196f3");
            Rtb.Document.Blocks.Add(new Paragraph(
                new Run(string.Format(
                    "==========================================    Stop  {0}     ==========================================", ApplicationInfo.AppNameVersion
                )) 
                { Foreground = Brushes.White, Background = color }));
            BtnStop.IsEnabled = false;
            BtnStart.IsEnabled = true;
           
            var color_red = (Brush)converter.ConvertFromString(red);
            DeviceId.Text = null;
            StatutScanner.Background = color_red;
            StatutScanner.Content = "Off";
            driver.CloseGhostsChromeDriver();
        }
        private void OnHidDataReceived(object sender, HidEvent e)
        {
            try
            {
                if (this.BtnStart.IsEnabled)
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
                    window.Activate();
                    var dict = keyConvertor.GetDictionary();
                    if (i == 0 && e.RawInput.keyboard.MakeCode == 42)
                    {
                        i = 1;
                        return;
                    }
                    if (i >= 0 && e.RawInput.keyboard.MakeCode == 56)
                    {
                        i = 2;
                        return;
                    }             
                     this.log += dict[e.RawInput.keyboard.MakeCode][i];        
                    i = 0;
                    if (e.RawInput.keyboard.VKey == 13)
                    {
                        var web = "http://";
                        AddHyperlinkText(web + this.log, this.log, "", "");
                        driver.OpenLink(web + this.log);
                        DbManager.InsertScanList(this.log);
                        this.log = "";

                    }
                }
            }
            catch (Exception ex)
            {
                Rtb.Document.Blocks.Add(new Paragraph(new Run("Error: " + ex.Message) { Foreground = Brushes.Red, Background = Brushes.Black }));
            }
        }

        private void AddHyperlinkText(string linkURL, string linkName,
          string TextBeforeLink, string TextAfterLink)
        {
            Paragraph para = new Paragraph
            {
                Margin = new Thickness(0) // remove indent between paragraphs
            };

            Hyperlink link = new Hyperlink
            {
                IsEnabled = true
            };
            link.Inlines.Add(linkName);
            link.NavigateUri = new Uri(linkURL);
            link.RequestNavigate += (sender, args) => driver.OpenLink(args.Uri.ToString());

            para.Inlines.Add(new Run("[" + DateTime.Now.ToLongTimeString() + "]: "));
            para.Inlines.Add(TextBeforeLink);
            para.Inlines.Add(link);
            para.Inlines.Add(new Run(TextAfterLink));

            Rtb.Document.Blocks.Add(para);
            Scrollviewer.ScrollToEnd();
        }


        private void UpdateStatutScanCode()
        {
            this.device = null;
            var converter = new BrushConverter();
            var devices = new List<IIinputDevice>
            {
                VirtualKeyboard.Instance
            };
            devices.AddRange(DeviceScanner.GetHidKeyboards());
            var devicesToRemove = new List<IIinputDevice>();
            foreach (IIinputDevice device in devices)
            {
                if (device.DeviceType != DeviceType.Other)
                {
                    continue;
                }
                ushort vid = Convert.ToUInt16(device.Vendor, 16);
                ushort pid = Convert.ToUInt16(device.Product, 16);
                IEnumerable<Device> rawDevices = HidDataReader.GetDevices().Where(d => d.ProductId == pid && d.VendorId == vid);

                if (rawDevices.Count() == 0)
                {
                    devicesToRemove.Add(device);
                }
                DeviceType type = device.DeviceType;

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
                    StatutScanner.Background = color_green;
                    StatutScanner.Content = "On";
                    BtnStart.IsEnabled = false;
                    BtnStop.IsEnabled = true;
                    DeviceId.Text = device.DeviceID;
                }
            }
            if (this.device == null)
            {
                var color_red = (Brush)converter.ConvertFromString(red);
                StatutScanner.Background = color_red;
                StatutScanner.Content = "Off";
                BtnStart.IsEnabled = true;
                DeviceId.Text = null;
                BtnStop.IsEnabled = false;
                BtnStart.Content = "Réessayez";
            }

        }
    }
}
