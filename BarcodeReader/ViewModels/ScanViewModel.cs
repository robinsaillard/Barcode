
using BarcodeReader.Commands;
using BarcodeReader.Models;
using BarcodeReader.Services;
using Djlastnight.Hid;
using Djlastnight.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BarcodeReader.ViewModels
{
    public class ScanViewModel : ViewModelBase
    {
        public CommandView<string> DriverStatut { get; private set; }
        public Window CurrentWindow { get; set; }
        public IIinputDevice Device { get; private set; }
        public HidDataReader Reader { get; private set; }
        private readonly KeyConvertor keyConvertor = new KeyConvertor();
        public Dictionary<string, Options> Options { get; set; }
        public WebDriver Driver { get; private set; }
        public string PostName { get; set; }
        private int i = 0;
        private string log = "";
        private readonly string green = "#32CD32";
        private readonly string red = "#e53935";

        public ScanViewModel()
        {
            DriverStatut = new CommandView<string>(OnDriverStatut);
            StartBtn = true;
            StopBtn = false;
            Device = null;
            CurrentWindow = Application.Current.MainWindow;
            StatutColor = red;
            StatutText = "OFF";
            AutoSroll = true;
            StartBtnContent = "Démarrer";
            StopBtnContent = "Stop";
            PostName = Environment.MachineName.ToString();
            if (!DbManager.PostNameExist(PostName))
            {
                DbManager.InsertPost(PostName);
            }
            Options = DbManager.GetOptions(PostName);
        }


        private void OnDriverStatut(string obj)
        {
            UpdateStatutScanCode();
            switch (obj)
            {
                case "start":
                    OnStartAction();
                    break;
                case "stop":
                    OnStopAction();
                    break;
                default:
                    break;
            }
        }

        private void OnStartAction()
        {
            if (StartBtn && Device != null)
            {
                StopBtn = true;
                StartBtn = false;
                StartDriver();
                StartDirectoryWatcher();
                Reader = new HidDataReader(CurrentWindow);
                Reader.HidDataReceived += OnHidDataReceived;

                Run run = new Run(string.Format(
                   "================ Démarrage {0} ================", ApplicationInfo.AppNameVersion
                ))
                {
                    Foreground = Brushes.White,
                    Background = GetColor("#2196f3")
                };
                RtbContent = run;
            }else
            {
                Run run = new Run("Erreur : Verifiez les branchements USB de la zapette/douchette")
                {
                    Foreground = Brushes.Red,
                    Background = Brushes.Black
                };
                RtbContent = run;
                StartBtnContent = "Réessayez";
            }
        }
        private void OnStopAction()
        {
            if (!(!StopBtn || Device == null))
            {
                Run run = new Run(string.Format(
                "================     Stop  {0}      ================", ApplicationInfo.AppNameVersion
                ))
                {
                    Foreground = Brushes.White,
                    Background = GetColor("#2196f3")
                };
                RtbContent = run;
            }
            if(Driver != null)
            {
                Driver.CloseGhostsChromeDriver();
            }
            StartBtn = true;
            StopBtn = false;
            DeviceId = "";
            StatutColor = red;
            StatutText = "OFF";
        }

        private void OnHidDataReceived(object sender, HidEvent e)
        {
            try
            {
                if (StartBtn) { return;}
                if (e.Device == null) { return; }

                var senderVid = e.Device.VendorId.ToString("X4");
                var senderPid = e.Device.ProductId.ToString("X4");
                if (Device.Vendor != senderVid || Device.Product != senderPid)
                {
                    return;
                }

                if (Device is HidKeyboard && !e.IsButtonDown && e.RawInput.keyboard.MakeCode != 0)
                {
                    CurrentWindow.Activate();
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
                        Uri link = new Uri(this.log);
                        AddHyperlinkText(link, this.log, "", "");                    
                        Driver.OpenLink(link);
                        DbManager.InsertScanList(this.log);
                        this.log = "";

                    }
                }
            }
            catch (Exception ex)
            {
                Run run = new Run("Error: " + ex.Message)
                {
                    Foreground = Brushes.Red,
                    Background = Brushes.Black
                };
                RtbContent = run;
            }
        }

        private void UpdateStatutScanCode()
        {
            Device = null;
            BrushConverter converter = new BrushConverter();
            List<IIinputDevice> devices = new List<IIinputDevice>
            {
                VirtualKeyboard.Instance
            };
            devices.AddRange(DeviceScanner.GetHidKeyboards());
            List<IIinputDevice> devicesToRemove = new List<IIinputDevice>();
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
            foreach (IIinputDevice deviceToRemove in devicesToRemove)
            {
                devices.Remove(deviceToRemove);
            }
            foreach (IIinputDevice device in devices)
            {
                if (device.Vendor == "0581" && device.Product == "011A" && device.DeviceType == DeviceType.Keyboard)
                {
                    Device = device;
                    DeviceId = device.DeviceID;
                    StatutText = "ON";
                    StatutColor = green;
                }
            }
        }

        private void AddHyperlinkText(Uri linkURL, string linkName, string TextBeforeLink, string TextAfterLink)
        {
            Paragraph para = new Paragraph
            {
                Margin = new Thickness(0)
            };

            Hyperlink link = new Hyperlink
            {
                IsEnabled = true
            };
            link.Inlines.Add(linkName);
            link.NavigateUri = linkURL;
            link.RequestNavigate += (sender, args) => Driver.OpenLink(args.Uri);

            para.Inlines.Add(new Run("[" + DateTime.Now.ToLongTimeString() + "]: "));
            para.Inlines.Add(TextBeforeLink);
            para.Inlines.Add(link);
            para.Inlines.Add(new Run(TextAfterLink));
            RtbContent = para;
        }

        private void StartDirectoryWatcher()
        {
            try
            {
                string values = Options["PDF_FILENAME"].Value;
                string directory = Options["DOWNLOAD_DIRECTORY"].Value;
                string ext = Options["PDF_EXTENSION"].Value;
                string printer = Options["PRINTER_NAME"].Value;
                string[] listFile = values.Split(';');
                _ = new DirectoryWatcher(directory, listFile, ext, printer);
            }
            catch (Exception ex)
            {
                Run run = new Run("Error: " + ex.Message)
                {
                    Foreground = Brushes.Red,
                    Background = Brushes.Black
                };
                RtbContent = run;
            }

        }

        private void StartDriver()
        {
            try
            {
                string directory = Options["DOWNLOAD_DIRECTORY"].Value;
                Driver = new WebDriver(directory);
                string version = Driver.GetChromeVersion();
            }
            catch (Exception ex)
            {
                Run run = new Run("Error: " + ex.Message)
                {
                    Foreground = Brushes.Red,
                    Background = Brushes.Black
                };
                RtbContent = run;
            }
        }

        private Brush GetColor(string color)
        {
            var converter = new BrushConverter();
            return (Brush)converter.ConvertFromString(color);
        }

        private bool _startBtn;
        public bool StartBtn
        {
            get => _startBtn;
            set => SetProperty(ref _startBtn, value);
        }

        private bool _stopBtn;
        public bool StopBtn
        {
            get => _stopBtn;
            set => SetProperty(ref _stopBtn, value);
        }

        private string _deviceId;
        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        private string _statutColor;
        public string StatutColor
        {
            get => _statutColor;
            set => SetProperty(ref _statutColor, value);
        }

        private string _statutText;
        public string StatutText
        {
            get => _statutText;
            set => SetProperty(ref _statutText, value);
        }

        private object _rtbContent;
        public object RtbContent
        {
            get => _rtbContent;
            set => SetProperty(ref _rtbContent, value);
        }

        private bool _autoSroll;
        public bool AutoSroll 
        { 
            get => _autoSroll; 
            set => SetProperty(ref _autoSroll, value); 
        }

        private string _startBtnContent;
        public string StartBtnContent
        {
            get => _startBtnContent;
            set => SetProperty(ref _startBtnContent, value);
        }

        private string _stopBtnContent;
        public string StopBtnContent
        {
            get => _stopBtnContent;
            set => SetProperty(ref _stopBtnContent, value);
        }
    }   
}
