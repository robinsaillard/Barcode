
using BarcodeReader.Commands;
using BarcodeReader.Services;
using BarcodeReader.Views;
using Djlastnight.Hid;
using Djlastnight.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BarcodeReader.ViewModels
{
    public class ScanViewModel : ViewModelBase
    {
        public CommandView<string> DriverStatut { get; private set; }
        public Window CurrentWindow { get; set; }
        public IIinputDevice Device { get; private set; }
        private HidDataReader reader;
        private readonly KeyConvertor keyConvertor = new KeyConvertor();
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
        }


        private void OnDriverStatut(string obj)
        {
            UpdateStatutScanCode();

            if (StartBtn)
            {
                StopBtn = true;
                StartBtn = false;
                reader = new HidDataReader(CurrentWindow);
                reader.HidDataReceived += OnHidDataReceived;
            }
            else
            {
                StartBtn = true;
                StopBtn = false;
                DeviceId = "";
                StatutColor = red;
                StatutText = "OFF";
            }
        }

        

        private void OnHidDataReceived(object sender, HidEvent e)
        {
            try
            {
                if (StartBtn)
                {
                    return;
                }

                if (e.Device == null)
                {
                    return;
                }
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
                        var web = "http://";
                       // AddHyperlinkText(web + this.log, this.log, "", "");
                      //  driver.OpenLink(web + this.log);
                        DbManager.InsertScanList(this.log);
                        this.log = "";

                    }
                }
            }
            catch (Exception ex)
            {
               // Rtb.Document.Blocks.Add(new Paragraph(new Run("Error: " + ex.Message) { Foreground = Brushes.Red, Background = Brushes.Black }));
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
                    RtbContent = "Test";
                }
            }

          /*  if (Device == null)
            {
                var color_red = (Brush)converter.ConvertFromString(red);
                StatutScanner.Background = color_red;
                StatutScanner.Content = "Off";
                BtnStart.IsEnabled = true;
                DeviceId.Text = null;
                BtnStop.IsEnabled = false;
                BtnStart.Content = "Réessayez";
            }*/

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

        private string _rtbContent;
        public string RtbContent
        {
            get => _rtbContent;
            set => SetProperty(ref _rtbContent, value);
        }
    }   
}
