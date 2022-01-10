using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeReader.Models
{
    public class Historique
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }

        public string DateToString
        {
            get
            {
                return Date.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }
    }
}
