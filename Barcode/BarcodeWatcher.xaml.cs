
using Barcode.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;

namespace Barcode
{
    /// <summary>
    /// Logique d'interaction pour BarcodeWatcher.xaml
    /// </summary>
    public partial class BarcodeWatcher : Window
    {
        private string green = "#32CD32";
        private string red = "#FF0000";

        public BarcodeWatcher()
        {
            InitializeComponent(); 
            
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Title = ApplicationInfo.AppNameVersion;
        }

        private void OnStartScan(object sender, RoutedEventArgs e)
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(string.Format("============ Démarage {0}  ============ ", ApplicationInfo.AppNameVersion)));
            this.rtb.Document.Blocks.Add(paragraph);
            this.btnStart.IsEnabled = false;
            this.btnStop.IsEnabled = true;

            var converter = new BrushConverter();
            var color_green = (Brush)converter.ConvertFromString(green);
            this.statutScanner.Background = color_green;
            this.statutScanner.Content = "On";
        }
        private void OnStopScan(object sender, RoutedEventArgs e)
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(string.Format("============    Stop  {0}    ============ ", ApplicationInfo.AppNameVersion)));
            this.rtb.Document.Blocks.Add(paragraph);
            this.btnStop.IsEnabled = false;
            this.btnStart.IsEnabled = true;

            var converter = new BrushConverter();
            var color_green = (Brush)converter.ConvertFromString(red);
            this.statutScanner.Background = color_green;
            this.statutScanner.Content = "Off";
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {

        }


    }
}