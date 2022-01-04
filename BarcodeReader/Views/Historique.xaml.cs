using BarcodeReader.Models;
using BarcodeReader.Services;
using System;
using System.Collections.Generic;
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

namespace BarcodeReader.Views
{
    /// <summary>
    /// Logique d'interaction pour OptionView.xaml
    /// </summary>
    public partial class HistoriqueView : UserControl
    {
        public HistoriqueView()
        {

            List<Historique> historique = DbManager.GetScanList();

            InitializeComponent();
            DataGridHistoriqueView.ItemsSource = historique;
        }
    }
}
