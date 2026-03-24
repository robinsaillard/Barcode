using BarcodeReader.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace BarcodeReader.Services
{
    class BarcodeCom
    {
        public SerialPort SerialPort;
        public BarcodeCom()
        {
           
            SerialPort = new SerialPort("COM4");
            SerialPort.BaudRate = 9600;
            SerialPort.Parity = Parity.None;
            SerialPort.StopBits = StopBits.One;
            SerialPort.DataBits = 8;
            SerialPort.Handshake = Handshake.None;

            SerialPort.DataReceived += DataReceivedHandler;
            SerialPort.Open();

        }



        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            MessageBox.Show("Erreur :" + indata);
            var coucou = "";
        }
    }
}
